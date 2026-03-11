using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppInterop.Runtime.Attributes;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Chat;

/// <summary>
/// Manages the chat interaction with the AI, including UI and game state
/// </summary>
[MonoSingleton]
[RegisterTypeInIl2Cpp]
public class ChatManager : MonoBehaviour {
  public static ChatManager Instance => MonoSingletonManager.Get<ChatManager>();

  /// <summary>
  /// Indicates if a chat session is currently active
  /// </summary>
  public bool IsChatActive { get; private set; } = false;
  private bool isInConversation = false;

  private readonly AiAdapter aiAdapter;
  private readonly ChatParser parser;
  private readonly ChatWriter writer;
  private readonly ChatExecutor executor;
  private readonly Action<string> processUserInput;

  private Il2CppFungus.NarrativeLog narrativeLog;

  private string chatTitle = "Say something to the bot";
  private readonly string chatDescription = string.Join("\n", [
    "First request may take a while.",
    "",
    "Commands:",
    "/exit: Force exit the chat (or just say goodbye)",
    "/reset: Reset bot memory",
  ]);

  /// <summary>
  /// Initializes chat components and handlers
  /// </summary>
  public ChatManager() : base() {
    aiAdapter = new AiAdapter();
    writer = ChatWriter.Instance;
    executor = new ChatExecutor(this, writer);
    parser = new ChatParser(writer, executor);
    processUserInput = new(ProcessUserInput);
  }

  /// <summary>
  /// Validates singleton initialization
  /// </summary>
  public void Awake() {
    this.ValidateSingleton();
    SaveStorage.EventBus.AddListener("cache-invalidated", ReloadHistoryFromSave);
    ReloadHistoryFromSave();
  }

  private void ReloadHistoryFromSave() {
    var nonSystem = SaveStorage.Instance.GetValue<List<AiProviders.AiProvider.ChatMessage>>("chat-history", []);
    aiAdapter.RestoreNonSystemMessages(nonSystem);
  }

  private void PersistHistoryToSave() {
    var nonSystem = aiAdapter.GetNonSystemMessagesSnapshot();
    SaveStorage.Instance.SetValue("chat-history", nonSystem);
  }

  /// <summary>
  /// Begins a new chat session
  /// </summary>
  public void StartChat() {
    MelonCoroutines.Start(ChatLoop());
  }

  /// <summary>
  /// Ends the current chat session
  /// </summary>
  /// <param name="waitForInput">If true, waits for user input before closing</param>
  [HideFromIl2Cpp]
  public async Task StopChat(bool waitForInput = false) {
    await writer.Stop(waitForInput);

    IsChatActive = false;
    isInConversation = false;

    // Reset bot emotes just in case
    ChatExecutor.ResetBotEmotes();
  }

  /// <summary>
  /// Resets the chat history and AI state
  /// </summary>
  public void ResetChat() {
    aiAdapter.ResetChat();
    SaveStorage.Instance.RemoveValue("chat-history");
  }

  /// <summary>
  /// Adds a message to the game's narrative log
  /// </summary>
  /// <param name="characterId">ID of the speaking character</param>
  /// <param name="text">Message text</param>
  public void AddToNarrativeLog(string characterId, string text) {
    if (narrativeLog != null) {
      var character = ConversationSingleton.Instance.GetCharacter(characterId);
      narrativeLog.AddLine(character, text);
    }
  }

  /// <summary>
  /// Switches the game to ADV (Story) mode with a bottom dialogue window.
  /// </summary>
  private static void EnterStoryState() {
    var gameScript = GameScript.Instance;
    var live2DController = Live2DControllerSingleton.Instance;
    var currentController = live2DController.GetReadyController("bot");
    var currentBrain = currentController.CurrentBrain;

    currentController.PrepareForDialogue();
    gameScript.ChangeToStoryState();
    currentBrain.EnableBrain();
    currentBrain.ChangeState(new StoryBrainState());
    currentController.SetEnabled(true);
  }

  private static void ExitStoryState() {
    InteractState.OnStoryStateFinished();
  }

  /// <summary>
  /// Main chat loop coroutine that handles the conversation flow
  /// </summary>
  /// <remarks>
  /// Manages the input popup display and waits for user responses.
  /// Runs until IsChatActive becomes false
  /// </remarks>
  [HideFromIl2Cpp]
  private IEnumerator ChatLoop() {
    var uiOverlay = UiOverlay.Instance;
    var gameScript = GameScript.Instance;
    var gameVariables = gameScript.GameVariables;
    var isCuddling = Utils.GameState.IsStateType<CuddleState>();

    narrativeLog = FindObjectOfType<Il2CppFungus.NarrativeLog>();
    IsChatActive = true;

    if (!isCuddling) {
      EnterStoryState();
    }

    // Provider preflight must happen BEFORE warmup (e.g. Ollama model download prompt).
    // If preflight fails or is rejected, we still open the cha
    var preflightTask = aiAdapter.EnsureReadyForChat();
    while (!preflightTask.IsCompleted) {
      yield return null;
    }

    // Warm up the AI provider as soon as user clicks the chat button
    aiAdapter.WarmUp();

    while (IsChatActive) {
      isInConversation = true;
      chatTitle = $"Say something to {gameVariables.botName}";

      // Show input popup
      uiOverlay.InputPopup(chatTitle, chatDescription, processUserInput);

      while (isInConversation) {
        // Wait for bot to finish speaking
        yield return null;
      }
    }

    if (!isCuddling) {
      ExitStoryState();
    }
  }

  private static bool ValidateUserInput(string userInput) {
    return userInput.Trim() != "";
  }

  private async void ProcessUserInput(string userInput) {
    try {
      // The actual logic is moved to a separate method so that
      // the code in the finally block is executed anyway
      await ProcessUserInputInternal(userInput);
    } finally {
      // Add a small delay so the window has time to close
      await Task.Delay(500);
      isInConversation = false;
    }
  }

  [HideFromIl2Cpp]
  private async Task ProcessUserInputInternal(string userInput) {
    if (userInput.StartsWith("/")) {
      switch (userInput.ToLower()) {
        case "/exit":
          await StopChat();
          return;
        case "/reset":
          ResetChat();
          return;
        case "/clear":
          ChatExecutor.ResetBotEmotes();
          return;
        case "/pack":
          // TODO: Implement history packing
          return;
      }
    }

    if (!ValidateUserInput(userInput)) {
      return;
    }

    AddToNarrativeLog("You", userInput);

    await parser.Prepare();
    await foreach (var chunk in aiAdapter.SendMessage(userInput)) {
      await parser.Parse(chunk);
    }
    await parser.Flush();

    PersistHistoryToSave();
  }
}
