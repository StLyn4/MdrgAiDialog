using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;

namespace MdrgAiDialog;

public class ChatSingleton : StoryMonoBehaviour {
  public static ChatSingleton Instance { get; private set; }

  public bool isChatActive { get; private set; } = false;
  private bool isInConversation = false;

  private AiAdapter aiAdapter;
  private Action<string> processUserInput;

  public ChatSingleton() {
    if (Instance != null) {
      throw new Exception("ChatSingleton instance already exists. Cannot create a new one.");
    }

    Instance = this;
    aiAdapter = new AiAdapter();
    processUserInput = new Action<string>(ProcessUserInput);
  }

  public void Update() {
    if (Input.GetKeyDown("c")) {
      StartChat();
    }
  }

  public void StartChat() {
    isChatActive = true;
    StartStory();
  }

  public void StopChat() {
    isChatActive = false;
    isInConversation = false;

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

    while (isChatActive) {
      isInConversation = true;
      uiOverlay.InputPopup( title, description, processUserInput);

      while (isInConversation) {
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
      isInConversation = false;
      return;
    }

    var messages = await aiAdapter.GetChatMessages(userInput);
    this.StartCoroutine(Conversation(messages));
  }

  private string[] ProcessMessages(string[] messages) {
    var gameState = GameStateSingleton.Instance;
    var isCuddle = gameState.IsStateType<CuddleState>();

    if (isCuddle) {
      // Should not show arm motion while in bed
      messages = messages.Where((message) => !message.StartsWith("#bot.Arm")).ToArray();
    }

    return messages;
  }

  private IEnumerator Conversation(string[] messages) {
    var processedMessages = ProcessMessages(messages);

    foreach (var message in processedMessages) {
      var isCommand = message.StartsWith("#");
      var finalMessage = isCommand ? message : $"Bot: {message}";
      yield return StartCoroutine(BetterConversationManager.DoConversation(finalMessage));
    }

    isInConversation = false;
  }

  private IEnumerator ClearBotExpression() {
    yield return this.StartCoroutine(
      Conversation(["#bot.Expression.Clear", "#bot.ArmBoth.DownNormal"])
    );
  }
}
