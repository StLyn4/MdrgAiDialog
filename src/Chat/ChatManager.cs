using BepInEx.Unity.IL2CPP.Utils;
using System;
using System.Collections;
using System.Threading.Tasks;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Chat;

/// <summary>
/// Manages the chat interaction with the AI, including UI and game state
/// </summary>
[MonoSingleton]
public class ChatManager : StoryMonoBehaviour {
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

  private Fungus.NarrativeLog narrativeLog;

  private string chatTitle = "Say something to the bot";
  private readonly string chatDescription = string.Join("\n", [
    "First request may take a while.",
    "",
    "Commands:",
    "exit: Force exit the chat (or just say goodbye)",
    "reset: Reset bot memory",
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
  }

  /// <summary>
  /// Begins a new chat session
  /// </summary>
  public void StartChat() {
    StartStory();
  }

  /// <summary>
  /// Ends the current chat session
  /// </summary>
  /// <param name="waitForInput">If true, waits for user input before closing</param>
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
  /// Implements the story interaction loop
  /// </summary>
  /// <param name="stopStory">Action to call when story should end</param>
  protected override void Story(Action stopStory) {
    this.StartCoroutine(ChatLoop(stopStory));
  }

  private IEnumerator ChatLoop(Action stopStory) {
    var uiOverlay = UiOverlay.Instance;
    var gameVariables = GameScript.Instance.GameVariables;

    narrativeLog = FindObjectOfType<Fungus.NarrativeLog>();
    IsChatActive = true;

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

    stopStory();
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

  private async Task ProcessUserInputInternal(string userInput) {
    switch (userInput.ToLower()) {
      case "exit":
        await StopChat();
        return;
      case "reset":
        ResetChat();
        return;
      case "clear":
        ChatExecutor.ResetBotEmotes();
        return;
    }

    if (!ValidateUserInput(userInput)) {
      return;
    }

    AddToNarrativeLog("You", userInput);

    await parser.Prepare();
    await foreach (var chunk in aiAdapter.GetChatStream(userInput)) {
      await parser.Parse(chunk);
    }
    await parser.Flush();
  }
}
