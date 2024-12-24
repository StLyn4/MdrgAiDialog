using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

public abstract class AiProvider {
  protected List<ChatMessage> Messages = [];

  protected string ExtractTextContent(string rawMessage) {
    var messages = JsonSerializer.Deserialize<string[]>(rawMessage);
    return string.Join(" ", messages.Where((m) => !m.StartsWith("#")));
  }

  public abstract void SetSystemMessage(string message);
  public abstract Task<string> SendMessage(string message);

  public void ResetChat(bool resetSystem = false) {
    if (resetSystem) {
      Messages.Clear();
    } else {
      var systemMessage = Messages.FirstOrDefault((m) => m.Role == "system");
      Messages.Clear();
      if (systemMessage != null) {
        Messages.Add(systemMessage);
      }
    }
  }

  protected class ChatMessage {
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
  }
}
