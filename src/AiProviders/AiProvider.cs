using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// Base class for AI providers that handle chat interactions
/// </summary>
public abstract class AiProvider {
  /// <summary>
  /// Chat history containing system, user and assistant messages
  /// </summary>
  protected List<ChatMessage> messages = [];

  /// <summary>
  /// Performs warmup of the AI model so it's ready to use as soon as possible
  /// </summary>
  /// <remarks>
  /// May perform additional billable requests depending on the provider
  /// </remarks>
  public abstract Task WarmUp();

  /// <summary>
  /// Sends a message to the AI and gets a complete response
  /// </summary>
  /// <param name="message">Message to send</param>
  /// <returns>AI's response</returns>
  public abstract Task<string> SendMessage(string message);

  /// <summary>
  /// Sends a message to the AI and gets a streaming response
  /// </summary>
  /// <param name="message">Message to send</param>
  /// <returns>Stream of response chunks</returns>
  public abstract IAsyncEnumerable<string> GetChatStream(string message);

  /// <summary>
  /// Sets or updates the system message that guides AI behavior
  /// </summary>
  /// <param name="message">System message to set</param>
  public void SetSystemMessage(string message) {
    messages.RemoveAll(m => m.Role == "system");
    messages.Insert(0, new ChatMessage { Role = "system", Content = message });
  }

  /// <summary>
  /// Clears chat history, optionally preserving system message
  /// </summary>
  /// <param name="resetSystem">If true, system message is also cleared</param>
  public void ResetChat(bool resetSystem = false) {
    if (resetSystem) {
      messages.Clear();
    } else {
      var systemMessage = messages.FirstOrDefault(m => m.Role == "system");
      messages.Clear();
      if (systemMessage != null) {
        messages.Add(systemMessage);
      }
    }
  }

  /// <summary>
  /// Removes the last message from chat history
  /// </summary>
  protected void RemoveLastMessage() {
    if (messages.Count > 0) {
      messages.RemoveAt(messages.Count - 1);
    }
  }

  /// <summary>
  /// Represents a message in the chat history
  /// </summary>
  protected class ChatMessage {
    /// <summary>
    /// Role of the message sender (system/user/assistant)
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    /// Content of the message
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }
  }
}
