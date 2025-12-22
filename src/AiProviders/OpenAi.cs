using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for OpenAI API and compatible services
/// </summary>
public class OpenAi : AiProvider {
  private static readonly Logger logger = new("OpenAi");

  protected readonly HttpClient client;

  public OpenAi(AiProviderConfig config) : base(config) {
    client = new HttpClient {
      BaseAddress = new Uri(EnsureTrailingSlash(config.ApiUrl)),
      Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds),
      DefaultRequestHeaders = {
        { "Connection", "keep-alive" },
        { "Authorization", $"Bearer {config.ApiKey}" },
        { "HTTP-Referer", "https://incontinentcell.itch.io/factorial-omega" },
        { "X-Title", "MDRG" }
      }
    };
  }

  private static string EnsureTrailingSlash(string url) {
    return url.EndsWith("/") ? url : url + "/";
  }

  public override Task WarmUp() {
    // No warmup needed for external providers
    return Task.CompletedTask;
  }

  public override async IAsyncEnumerable<string> SendMessage(string message) {
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
        string content = null;

        try {
          string line = await reader.ReadLineAsync();
          if (string.IsNullOrWhiteSpace(line)) continue;

          // Ignore SSE comments (keep-alive pings)
          if (line.StartsWith(":")) continue;

          if (line.StartsWith("data: ")) {
            line = line[6..];
          }

          if (line == "[DONE]") break;

          var chunk = JsonSerializer.Deserialize<ChatResponse>(line);

          if (chunk?.Choices?.Count > 0 && chunk.Choices[0].Delta?.Content != null) {
            content = chunk.Choices[0].Delta.Content;
            fullResponseBuilder.Append(content);
          }
        } catch (Exception ex) {
          logger.LogError($"Parse error: {ex.Message}");
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

  protected virtual async Task<HttpResponseMessage> MakeRequest(bool stream, List<ChatMessage> overrideMessages = null) {
    var requestBody = CreateRequestPayload(stream, overrideMessages);

    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions") {
      Content = new StringContent(
        JsonSerializer.Serialize(requestBody, new JsonSerializerOptions {
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }),
        Encoding.UTF8,
        "application/json"
      )
    };

    var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

    if (!response.IsSuccessStatusCode) {
      var errorContent = await response.Content.ReadAsStringAsync();
      var errorMessage = errorContent;

      try {
        using var doc = JsonDocument.Parse(errorContent);
        var errorObj = doc.RootElement.GetProperty("error");
        var messageElement = errorObj.GetProperty("message");
        errorMessage = messageElement.GetString() ?? errorContent;
      } catch {
        // Fallback to full content on parse error
        errorMessage = errorContent;
      }

      throw new HttpRequestException($"API Error: {response.StatusCode} - {errorMessage}");
    }

    return response;
  }

  protected virtual Dictionary<string, object> CreateRequestPayload(bool stream, List<ChatMessage> overrideMessages) {
    var msgs = overrideMessages ?? messages;

    var payload = new Dictionary<string, object> {
      { "messages", msgs },
      { "model", config.Model },
      { "temperature", config.Temperature },
      { "stream", stream }
    };

    if (config.TopK > 0) {
      payload["top_k"] = config.TopK;
    }

    // Handle reasoning pre-fill
    if (config.ReasoningPreFill && overrideMessages == null) {
      var messageList = new List<ChatMessage>(msgs.Count + 1);
      messageList.AddRange(msgs);

      messageList.Add(new ChatMessage {
        Role = "assistant",
        Content = "<think></think>\n",
        Prefix = true
      });

      payload["messages"] = messageList;
    }

    return payload;
  }

  // Data structures for JSON parsing
  protected class ChatResponse {
    [JsonPropertyName("choices")]
    public List<ChatChoice> Choices { get; set; }
  }

  protected class ChatChoice {
    [JsonPropertyName("delta")]
    public ChatMessage Delta { get; set; }
  }
}
