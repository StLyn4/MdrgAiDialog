using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace MdrgAiDialog.AiProviders;

public class OllamaAiProvider : AiProvider {
  private readonly HttpClient _client;
  private readonly string _model;
  private readonly double _temperature;

  public OllamaAiProvider(string apiUrl, string model, double temperature, int timeoutSeconds) {
    _model = model;
    _temperature = temperature;
    _client = new HttpClient {
      BaseAddress = new Uri(apiUrl),
      Timeout = TimeSpan.FromSeconds(timeoutSeconds)
    };
  }

  public override void SetSystemMessage(string message) {
    Messages.RemoveAll((m) => m.Role == "system");
    Messages.Insert(0, new ChatMessage { Role = "system", Content = message });
  }

  public override async Task<string> SendMessage(string message) {
    try {
      Messages.Add(new ChatMessage { Role = "user", Content = message });

      Debug.Log("==================================================");
      Debug.Log($"Messages in total: {Messages.Count}");
      Debug.Log("Message history:");
      foreach (var msg in Messages) {
        switch (msg.Role) {
          case "system":
            Debug.LogError($"[System] {msg.Content}");
            break;
          case "user":
            Debug.Log($"[User] {msg.Content}");
            break;
          case "assistant":
            Debug.LogWarning($"[Assistant] {msg.Content}");
            break;
        }
      }
      Debug.Log("==================================================");

      var request = new ChatRequest {
        Model = _model,
        Messages = Messages,
        Temperature = _temperature,
        Stream = false,
      };

      var response = await _client.PostAsync(
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

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
  }

  private class ChatResponse {
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }
  }
}
