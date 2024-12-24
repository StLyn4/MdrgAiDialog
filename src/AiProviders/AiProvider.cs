using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

public abstract class AiProvider {
  protected List<ChatMessage> _messages = [];

  public abstract void SetSystemMessage(string message);
  public abstract Task<string> SendMessage(string message);

  public void ResetChat(bool resetSystem = false) {
    if (resetSystem) {
      _messages.Clear();
    } else {
      var systemMessage = _messages.FirstOrDefault((m) => m.Role == "system");
      _messages.Clear();
      if (systemMessage != null) {
        _messages.Add(systemMessage);
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
