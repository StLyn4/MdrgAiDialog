using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MdrgAiDialog.AiProviders;

public class MistralAiProvider : AiProvider {
  private readonly HttpClient _client;
  private readonly string _model;
  private readonly double _temperature;

  public MistralAiProvider(string apiUrl, string apiKey, string model, double temperature, int timeoutSeconds) {
    _model = model;
    _temperature = temperature;
    _client = new HttpClient {
      BaseAddress = new Uri(apiUrl),
      Timeout = TimeSpan.FromSeconds(timeoutSeconds)
    };
    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
  }

  public override void SetSystemMessage(string message) {
    Messages.RemoveAll((m) => m.Role == "system");
    Messages.Insert(0, new ChatMessage { Role = "system", Content = message });
  }

  public override async Task<string> SendMessage(string message) {
    try {
      Messages.Add(new ChatMessage { Role = "user", Content = message });

      var request = new ChatRequest {
        Model = _model,
        Messages = Messages,
        Temperature = _temperature,
        MaxTokens = 1000,
        TopP = 1,
        SafeMode = false,
        RandomSeed = null
      };

      var response = await _client.PostAsync(
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

      Messages.Add(new ChatMessage { Role = "assistant", Content = assistantMessage.Content });
      return assistantMessage.Content;
    } catch (Exception ex) {
      if (Messages.Count > 0) {
        Messages.RemoveAt(Messages.Count - 1);
      }
      return $"Error: {ex.Message}";
    }
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
