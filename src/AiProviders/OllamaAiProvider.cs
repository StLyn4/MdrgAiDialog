using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for Ollama API
/// </summary>
public class OllamaAiProvider : AiProvider {
  private readonly HttpClient client;
  private readonly string model;
  private readonly double temperature;

  /// <summary>
  /// Creates a new instance of OllamaAiProvider
  /// </summary>
  /// <param name="apiUrl">Base URL of Ollama API</param>
  /// <param name="model">Model name to use</param>
  /// <param name="temperature">Temperature for response generation</param>
  /// <param name="timeoutSeconds">Request timeout in seconds</param>
  public OllamaAiProvider(string apiUrl, string model, double temperature, int timeoutSeconds) {
    this.model = model;
    this.temperature = temperature;

    client = new HttpClient {
      BaseAddress = new Uri(apiUrl),
      Timeout = TimeSpan.FromSeconds(timeoutSeconds),
      DefaultRequestHeaders = {
        { "Connection", "keep-alive" }
      }
    };
  }

  public override Task WarmUp() {
    return MakeRequest(
      stream: true,
      overrideMessages: [
        new ChatMessage { Role = "user", Content = "DO NOT RESPOND" }
      ]
    );
  }

  public override async Task<string> SendMessage(string message) {
    try {
      messages.Add(new ChatMessage { Role = "user", Content = message });

      var response = await MakeRequest(stream: false);
      var result = await response.Content.ReadAsStringAsync();
      var chatResponse = JsonSerializer.Deserialize<ChatResponse>(result);

      var content = chatResponse.Message.Content;
      messages.Add(new ChatMessage { Role = "assistant", Content = content });
      return chatResponse.Message.Content;
    } catch (Exception ex) {
      RemoveLastMessage();
      return $"Error: {ex.Message}";
    }
  }

  public override async IAsyncEnumerable<string> GetChatStream(string message) {
    messages.Add(new ChatMessage { Role = "user", Content = message });

    HttpResponseMessage response = null;
    var fullResponseBuilder = new StringBuilder(2048);
    string error = null;

    try {
      response = await MakeRequest(stream: true);
    } catch (Exception ex) {
      error = ex.Message;
    }

    if (response != null && error == null) {
      var responseStream = await response.Content.ReadAsStreamAsync();
      using var reader = new StreamReader(responseStream);

      while (!reader.EndOfStream) {
        string line = null;
        string content = null;

        try {
          line = await reader.ReadLineAsync();
          var chunk = JsonSerializer.Deserialize<ChatResponse>(line);

          if (chunk?.Message?.Content != null) {
            fullResponseBuilder.Append(chunk.Message.Content);
            content = chunk.Message.Content;
          }
        } catch (Exception ex) {
          error = ex.Message;
          break;
        }

        if (content != null) {
          yield return content;
        }
      }
    }

    if (error != null) {
      RemoveLastMessage();
      yield return $"Error: {error}";
      yield break;
    }

    messages.Add(new ChatMessage {
      Role = "assistant",
      Content = fullResponseBuilder.ToString()
    });
  }

  private async Task<HttpResponseMessage> MakeRequest(bool stream, List<ChatMessage> overrideMessages = null) {
    var request = new ChatRequest {
      Model = model,
      Messages = overrideMessages ?? messages,
      Temperature = temperature,
      Stream = stream
    };

    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat") {
      Content = new StringContent(
        JsonSerializer.Serialize(request),
        Encoding.UTF8,
        "application/json"
      )
    };

    var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

    if (!response.IsSuccessStatusCode) {
      var errorContent = await response.Content.ReadAsStringAsync();
      var errorMessage = errorContent;

      try {
        var errorBody = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
        errorMessage = errorBody.Error;
      } catch { }

      throw new HttpRequestException(errorMessage);
    }

    return response;
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

  private class ErrorResponse {
    [JsonPropertyName("error")]
    public string Error { get; set; }
  }
}
