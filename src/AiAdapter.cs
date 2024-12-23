using System.Threading.Tasks;

namespace MdrgAiDialog;

public class AiAdapter {
  public async Task<string> SendMessage(string message) {
    // TODO: Implement
    return message;
  }

  public async Task<string[]> GetChatMessages(string userInput) {
    // TODO: Implement
    var response = await SendMessage(userInput);
    return ["#bot.Expression.VeryHappy", "#bot.ArmBoth.UpHi", response];
  }
}
