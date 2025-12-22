using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// Base class for AI providers that handle chat interactions
/// </summary>
public abstract class AiProvider(AiProviderConfig config) {
  /// <summary>
  /// Configuration for this provider
  /// </summary>
  protected readonly AiProviderConfig config = config;

  /// <summary>
  /// Chat history containing system, user and assistant messages
  /// </summary>
  protected List<ChatMessage> messages = [];

  /// <summary>
  /// Returns a snapshot of chat messages excluding the system message.
  /// Intended for persistence
  /// </summary>
  public List<ChatMessage> GetNonSystemMessagesSnapshot() {
    // Avoid LINQ allocations on IL2CPP; snapshot list is used for serialization.
    var result = new List<ChatMessage>(messages.Count);
    for (int i = 0; i < messages.Count; i++) {
      var m = messages[i];
      if (m == null) continue;
      if (m.Role == "system") continue;
      result.Add(m);
    }
    return result;
  }

  /// <summary>
  /// Replaces chat history excluding system message. Keeps current system message intact.
  /// Intended for restoring persisted history
  /// </summary>
  public void SetNonSystemMessages(IEnumerable<ChatMessage> nonSystemMessages) {
    var systemMessage = messages.FirstOrDefault(m => m?.Role == "system");
    messages.Clear();

    if (systemMessage != null) {
      messages.Add(systemMessage);
    }

    if (nonSystemMessages == null) {
      return;
    }

    foreach (var m in nonSystemMessages) {
      if (m == null) continue;
      if (m.Role == "system") continue;
      // Copy to avoid sharing references with persisted/cache objects.
      messages.Add(new ChatMessage { Role = m.Role, Content = m.Content, Prefix = m.Prefix });
    }
  }

  /// <summary>
  /// Performs warmup of the AI model so it's ready to use as soon as possible
  /// </summary>
  /// <remarks>
  /// May perform additional billable requests depending on the provider
  /// </remarks>
  public abstract Task WarmUp();

  /// <summary>
  /// Ensures the provider is ready to process a new user message before the UI enters a blocked "waiting" state.
  /// </summary>
  /// <remarks>
  /// This hook is intended for provider-specific prerequisites (e.g. local model availability checks, auth prompts).
  /// </remarks>
  /// <returns>True if chat flow can continue, false if the send should be cancelled.</returns>
  public virtual Task<bool> EnsureReadyForChat() {
    return Task.FromResult(true);
  }

  /// <summary>
  /// Sends a message to the AI and gets a streaming response
  /// </summary>
  /// <param name="message">Message to send</param>
  /// <returns>Stream of response chunks</returns>
  public abstract IAsyncEnumerable<string> SendMessage(string message);

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
  public class ChatMessage {
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

    /// <summary>
    /// Whether the message is a prefix
    /// </summary>
    [JsonPropertyName("prefix")]
    public bool? Prefix { get; set; }
  }
}
