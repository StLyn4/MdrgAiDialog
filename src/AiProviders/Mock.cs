using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// Mock AI provider for testing
/// </summary>
public class Mock(AiProviderConfig config) : AiProvider(config) {
  public override Task WarmUp() {
    return Task.CompletedTask;
  }

  public override async IAsyncEnumerable<string> SendMessage(string message) {
    string responseText = GetResponseText(message);

    // Simulate initial processing delay
    await Task.Delay(250);

    int chunkSize = 4;
    for (int i = 0; i < responseText.Length; i += chunkSize) {
      int charsLeft = responseText.Length - i;
      int charsToTake = Math.Min(charsLeft, chunkSize);
      yield return responseText.Substring(i, charsToTake);
      await Task.Delay(10); // Simulate typing delay
    }

    messages.Add(new ChatMessage { Role = "assistant", Content = responseText });
  }

  private string GetResponseText(string message) {
    messages.Add(new ChatMessage { Role = "user", Content = message });

    return message switch {
      "0" => message,
      "1" => "Hi! #!bot.Expression.VeryHappy #!bot.ArmR.UpHi #!bot.Expression.Blush I am so happy to see you! #!bot.ArmBoth.DownNormal #!bot.Expression.NoBlush",
      "2" => "New lines: \r1. \\\\r \r\n2. \\\\r\\\\n \n3. \\\\n",
      "3" => "  Too \t  many    \n\n  spaces  #!flow.SplitMessage \n\n\n\n  And \t some more\t\t\t\tspaces",
      "4" => "#!bot.Expression.BROKEN-EMOTE",
      "5" => "#!flow.ExitChat THIS TEXT SHOULD NOT BE SEEN",
      "6" => "Bye! #!flow.ExitChat THIS TEXT SHOULD NOT BE SEEN",
      "7" => "That reminds me of something...#!flow.SplitMessage Actually, last week I was thinking about...",
      _ => message
    };
  }
}
