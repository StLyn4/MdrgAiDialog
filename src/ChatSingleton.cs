using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;

namespace MdrgAiDialog;

public class ChatSingleton : StoryMonoBehaviour {
  public static ChatSingleton Instance { get; private set; }

  public bool IsChatActive { get; private set; } = false;
  private bool IsInConversation = false;

  private AiAdapter AiAdapter;
  private Action<string> ProcessUserInputAction;

  // TODO: Remove later
  private readonly BepInEx.Logging.ManualLogSource _logger;

  public ChatSingleton() {
    if (Instance != null) {
      throw new Exception("ChatSingleton instance already exists. Cannot create a new one.");
    }

    Instance = this;
    AiAdapter = new AiAdapter();
    ProcessUserInputAction = new Action<string>(ProcessUserInput);
    _logger = BepInEx.Logging.Logger.CreateLogSource("ChatSingleton");
  }

  public void StartChat() {
    IsChatActive = true;
    StartStory();
  }

  public void StopChat() {
    IsChatActive = false;
    IsInConversation = false;

    // Clear bot expression just in case
    this.StartCoroutine(ClearBotExpression());
  }

  public override void Story(Action stopStory) {
    this.StartCoroutine(ChatLoop(stopStory));
  }

  private IEnumerator ChatLoop(Action stopStory) {
    var uiOverlay = UiOverlay.Instance;
    var gameScript = GameScript.Instance;
    var botName = gameScript.GameVariables.botName;

    var title = "Chat";
    var description = $"Say something to {botName}.\n\nTo end the conversation, just say goodbye.\n(Type \"exit\" to force exit)";

    while (IsChatActive) {
      IsInConversation = true;
      uiOverlay.InputPopup(title, description, ProcessUserInputAction);

      while (IsInConversation) {
        yield return null;
      }
    }

    stopStory();
  }

  private bool ValidateUserInput(string userInput) {
    return userInput.Trim() != "";
  }

  private async void ProcessUserInput(string userInput) {
    if (userInput.ToLower() == "exit") {
      StopChat();
      return;
    }

    if (!ValidateUserInput(userInput)) {
      IsInConversation = false;
      return;
    }

    _logger.LogInfo($"userInput: '{userInput}'");
    var messages = await AiAdapter.GetChatMessages(userInput);
    _logger.LogInfo($"messages: '{string.Join(", ", messages)}'");
    this.StartCoroutine(Conversation(messages));
  }

  private string ProcessMessage(string message) {
    var gameState = GameStateSingleton.Instance;
    var isCuddle = gameState.IsStateType<CuddleState>();

    message = message.Trim();

    // Process flow commands
    if (message.StartsWith("#flow.")) {
      switch (message) {
        case "#flow.ExitChat":
          IsChatActive = false; // Will exit after current message processing
          return "";
        case "#flow.ResetChat":
          AiAdapter.ResetChat();
          return "";
      }
    }

    // Filter arm motions in cuddle state
    if (isCuddle && message.StartsWith("#bot.Arm")) {
      return "";
    }

    // Return empty string for commands that were filtered out
    // Return the message for valid commands or text messages
    return message;
  }

  private IEnumerator Conversation(string[] messages) {
    foreach (var message in messages) {
      var processedMessage = ProcessMessage(message);

      if (string.IsNullOrEmpty(processedMessage)) {
        continue;
      }

      var isCommand = processedMessage.StartsWith("#");
      var finalMessage = isCommand ? processedMessage : $"Bot: {processedMessage}";
      _logger.LogInfo($"isCommand: {isCommand}; message: '{processedMessage}'");

      yield return StartCoroutine(BetterConversationManager.DoConversation(finalMessage));

      if (!IsChatActive) {
        break; // Exit if chat was stopped by #flow.ExitChat
      }
    }

    IsInConversation = false;
  }

  private IEnumerator ClearBotExpression() {
    yield return this.StartCoroutine(
      Conversation(["#bot.Expression.Clear", "#bot.ArmBoth.DownNormal"])
    );
  }
}
