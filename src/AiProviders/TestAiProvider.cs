using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for testing various message formats and commands
/// </summary>
public class TestAiProvider : EchoAiProvider {
  /// <summary>
  /// Returns predefined responses for test cases, otherwise echoes the input
  /// </summary>
  /// <param name="message">Test case number or message to echo</param>
  public override Task<string> SendMessage(string message) {
    return message switch {
      "0" => Task.FromResult(message),
      "1" => base.SendMessage("Hi! #!bot.Expression.VeryHappy #!bot.ArmR.UpHi #!bot.Expression.Blush I am so happy to see you! #!bot.ArmBoth.DownNormal #!bot.Expression.NoBlush"),
      "2" => base.SendMessage("New lines: \r1. \\\\r \r\n2. \\\\r\\\\n \n3. \\\\n"),
      "3" => base.SendMessage("  Too \t  many    \n\n  spaces  #!flow.SplitMessage \n\n\n\n  And \t some more\t\t\t\tspaces"),
      "4" => base.SendMessage("#!bot.Expression.BROKEN-EMOTE"),
      "5" => base.SendMessage("#!flow.ExitChat THIS TEXT SHOULD NOT BE SEEN"),
      "6" => base.SendMessage("Bye! #!flow.ExitChat THIS TEXT SHOULD NOT BE SEEN"),
      "7" => base.SendMessage("That reminds me of something...#!flow.SplitMessage Actually, last week I was thinking about..."),
      _ => base.SendMessage(message),
    };
  }

  public override IAsyncEnumerable<string> GetChatStream(string message) {
    return base.GetChatStream(message);
  }
}
