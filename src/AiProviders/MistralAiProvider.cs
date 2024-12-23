using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MdrgAiDialog.AiProviders;

public class MistralAiProvider : IAiProvider {
  private readonly HttpClient client;
  private readonly List<ChatMessage> messages = new();
  private readonly string model;

  public MistralAiProvider(string apiUrl, string apiKey, string model) {
    this.model = model;
    client = new HttpClient {
      BaseAddress = new Uri(apiUrl)
    };
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
  }

  public void SetSystemMessage(string message) {
    messages.RemoveAll(m => m.Role == "system");

    messages.Insert(0, new ChatMessage {
      Role = "system",
      Content = message
    });
  }

  public async Task<string> SendMessage(string message) {
    try {
      messages.Add(new ChatMessage { Role = "user", Content = message });

      var request = new ChatRequest {
        Model = model,
        Messages = messages,
        Temperature = 0.7,
        MaxTokens = 1000,
        TopP = 1,
        SafeMode = false,
        RandomSeed = null
      };

      var response = await client.PostAsync(
        "v1/chat/completions",
        new StringContent(
          JsonSerializer.Serialize(request, new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
          }),
          Encoding.UTF8,
          "application/json"
        )
      );

      var result = await response.Content.ReadAsStringAsync();
      var chatResponse = JsonSerializer.Deserialize<ChatResponse>(result);
      var assistantMessage = chatResponse.Choices[0].Message;

      messages.Add(assistantMessage);
      return assistantMessage.Content;
    } catch (Exception ex) {
      // Delete user message if error
      if (messages.Count > 0) {
        messages.RemoveAt(messages.Count - 1);
      }
      return $"Error: {ex.Message}";
    }
  }

  public void ResetChat() {
    messages.Clear();
  }

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

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }

    [JsonPropertyName("top_p")]
    public double TopP { get; set; }

    [JsonPropertyName("safe_mode")]
    public bool SafeMode { get; set; }

    [JsonPropertyName("random_seed")]
    public int? RandomSeed { get; set; }
  }

  private class ChatResponse {
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; }
  }

  private class ChatChoice {
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
  }
}
