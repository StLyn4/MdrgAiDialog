using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

public class OllamaAiProvider : IAiProvider {
  private readonly HttpClient client;
  private readonly List<ChatMessage> messages = new();
  private readonly string model;

  public OllamaAiProvider(string apiUrl, string model) {
    this.model = model;
    client = new HttpClient {
      BaseAddress = new Uri(apiUrl)
    };
  }

  public void SetSystemMessage(string message) {
    messages.RemoveAll(m => m.Role == "system");
    messages.Insert(0, new ChatMessage { Role = "system", Content = message });
  }

  public async Task<string> SendMessage(string message) {
    try {
      messages.Add(new ChatMessage { Role = "user", Content = message });

      var request = new ChatRequest {
        Model = model,
        Messages = messages,
        Stream = false
      };

      var response = await client.PostAsync(
        "api/chat",
        new StringContent(
          JsonSerializer.Serialize(request),
          Encoding.UTF8,
          "application/json"
        )
      );

      var result = await response.Content.ReadAsStringAsync();
      var chatResponse = JsonSerializer.Deserialize<ChatResponse>(result);
      var assistantMessage = chatResponse.Message;

      messages.Add(new ChatMessage { Role = "assistant", Content = assistantMessage.Content });
      return assistantMessage.Content;
    } catch (Exception ex) {
      if (messages.Count > 0) {
        messages.RemoveAt(messages.Count - 1);
      }
      return $"Error: {ex.Message}";
    }
  }

  public void ResetChat() {
    messages.Clear();
  }

  // ... классы для сериализации немного отличаются от Mistral API
  private class ChatMessage {
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
  }

  private class ChatRequest {
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
  }

  private class ChatResponse {
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
  }
}
