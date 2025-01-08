using System.Collections.Generic;
using System.Threading.Tasks;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Chat;

/// <summary>
/// Executes commands embedded in AI responses
/// </summary>
public class ChatExecutor(ChatManager chatManager, ChatWriter chatWriter) {
  public readonly ChatManager chatManager = chatManager;
  public readonly ChatWriter chatWriter = chatWriter;

  private readonly Logger logger = new("ChatExecutor");

  public static readonly HashSet<string> validExpressions = [
    "#!bot.Expression.VerySad", "#!bot.Expression.Sad",
    "#!bot.Expression.Happy", "#!bot.Expression.VeryHappy",
    "#!bot.Expression.Shock","#!bot.Expression.VeryShock",
    "#!bot.Expression.Angry", "#!bot.Expression.VeryAngry"
  ];

  public static readonly HashSet<string> validBlush = [
    "#!bot.Expression.Blush", "#!bot.Expression.VeryBlush"
  ];

  public static readonly HashSet<string> validArmL = [
    "#!bot.ArmL.UpPoint", "#!bot.ArmL.UpHi", "#!bot.ArmL.UpLecture",
    "#!bot.ArmL.DownNormal", "#!bot.ArmL.DownClenched"
  ];

  public static readonly HashSet<string> validArmR = [
    "#!bot.ArmR.UpPoint", "#!bot.ArmR.UpHi", "#!bot.ArmR.UpLecture",
    "#!bot.ArmR.DownNormal", "#!bot.ArmR.DownClenched"
  ];

  public static readonly HashSet<string> validArmBoth = [
    "#!bot.ArmBoth.UpPoint", "#!bot.ArmBoth.UpHi", "#!bot.ArmBoth.UpLecture",
    "#!bot.ArmBoth.DownNormal", "#!bot.ArmBoth.DownClenched"
  ];

  public static readonly HashSet<string> validFlow = [
    "#!flow.ExitChat", "#!flow.ResetChat", "#!flow.SplitMessage"
  ];

  /// <summary>
  /// Executes a command string
  /// </summary>
  /// <param name="command">Command in format "category.action" or "category.slot.item"</param>
  /// <remarks>
  /// Supported categories:
  /// - bot: Controls bot expressions and movements
  /// - flow: Controls chat flow (exit, reset, split message)
  /// </remarks>
  public async Task Run(string command) {
    logger.Log($"Executing command: {command}");
    var parts = command.Split('.');

    switch (parts[0]) {
      case "bot":
        if (parts.Length == 3) {
          SetEmote(parts[0], parts[1], parts[2]);
        }
        break;

      case "flow":
        if (parts.Length == 2) {
          switch (parts[1]) {
            case "ResetChat":
              chatManager.ResetChat();
              break;
            case "ExitChat":
              await chatManager.StopChat(waitForInput: true);
              break;
            case "SplitMessage":
              chatWriter.Pause();
              await chatWriter.WaitForInput();
              SetEmote("bot", "ArmBoth", "DownNormal");
              chatWriter.Clear();
              chatWriter.Resume();
              break;
          }
        }
        break;
    }

    // Unknown commands will be ignored
  }

  /// <summary>
  /// Sets a bot emote (expression or arm position)
  /// </summary>
  /// <param name="characterId">Target character ID</param>
  /// <param name="slotId">Emote slot (Expression/ArmL/ArmR/ArmBoth)</param>
  /// <param name="itemId">Emote item ID</param>
  /// <remarks>
  /// Arm movements are ignored in cuddle state to prevent visual glitches
  /// </remarks>
  public static void SetEmote(string characterId, string slotId, string itemId) {
    var isCuddling = Utils.GameState.IsStateType<CuddleState>();

    if (isCuddling && slotId.StartsWith("Arm")) {
      // Ignore arm motions in cuddle state
      return;
    }

    var emote = new EmoteData(slotId, itemId) {
      instant = false
    };

    // Implicit conversion of Il2CppSystem objects is slightly... broken.
    // Basically, it's `var emotes = [emote]`
    var emotesArray = new Il2CppSystem.Collections.Generic.List<EmoteData>(1);
    emotesArray.Add(emote);
    var emotes = emotesArray.Cast<Il2CppSystem.Collections.Generic.IEnumerable<EmoteData>>();

    MainThreadRunner.Run(() => {
      var live2DController = Live2DControllerSingleton.Instance;
      var character = live2DController.GetReadyController(characterId);
      character?.SetEmote(emotes);
    });
  }

  /// <summary>
  /// Resets bot expression and arm positions to default
  /// </summary>
  public static void ResetBotEmotes(bool instantly = false) {
    MainThreadRunner.Run(() => {
      var live2DController = Live2DControllerSingleton.Instance;
      var character = live2DController.GetReadyController("bot");
      character?.SetDefaultEmote(instantly);
    });
  }

  /// <summary>
  /// Resets bot expression to default
  /// </summary>
  public static void ResetBotExpression() {
    SetEmote("bot", "Expression", "Clear");
  }

  /// <summary>
  /// Resets bot arms to default
  /// </summary>
  public static void ResetBotArms() {
    SetEmote("bot", "ArmBoth", "DownNormal");
  }
}
