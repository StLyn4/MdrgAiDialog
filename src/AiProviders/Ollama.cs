using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Il2Cpp;
using Il2CppCysharp.Threading.Tasks;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for Ollama
/// </summary>
public class Ollama : OpenAi {
  private bool modelAvailable = false;
  private readonly HttpClient managementClient;

  private static readonly Logger logger = new("Ollama");

  public Ollama(AiProviderConfig config) : base(config) {
    managementClient = new HttpClient {
      BaseAddress = new Uri(GetBaseOllamaUrl()),
      Timeout = System.Threading.Timeout.InfiniteTimeSpan,
      DefaultRequestHeaders = {
        { "Connection", "keep-alive" },
        { "Authorization", $"Bearer {config.ApiKey}" }
      }
    };

    // Check if model exists in background
    CheckModelExists().ContinueWith(task => {
      if (task.Status == TaskStatus.RanToCompletion && task.Result) {
        modelAvailable = true;
      }
    });
  }

  public override async Task<bool> EnsureReadyForChat() {
    // Best case scenario: model is already available
    if (modelAvailable) {
      return true;
    }

    // Fast path: model was downloaded/created after initial check in the constructor
    if (await CheckModelExists()) {
      modelAvailable = true;
      return true;
    }

    // If model is still missing, ask user to download it
    var downloadAttempted = await EnsureModelAvailable();
    if (!downloadAttempted) {
      return false;
    }

    // Verify model presence after download attempt
    if (await CheckModelExists()) {
      modelAvailable = true;
      return true;
    }

    logger.LogWarning($"Model '{config.Model}' is still missing after download attempt.");
    return false;
  }

  private async Task<bool> EnsureModelAvailable() {
    if (modelAvailable) {
      return true;
    }

    // Ask user to download
    var tcs = new TaskCompletionSource<bool>();

    await MainThreadRunner.Run(() => {
      UiOverlay.Instance.SimplePopup(
        "Model Missing",
        $"The model '{config.Model}' is not present on your Ollama server.\nDo you want to download it now?",
        new Action(() => tcs.TrySetResult(true)), // Yes
        new Action(() => tcs.TrySetResult(false)) // No
      );
    });

    var shouldDownload = await tcs.Task;
    if (!shouldDownload) {
      logger.LogWarning($"Model '{config.Model}' is missing and download was rejected.");
      return false;
    }

    try {
      await ProgressPopupHelper.Show(
        "Downloading Model",
        $"Pulling {config.Model}...",
        DownloadModel
      );
    } catch (Exception ex) {
      logger.LogError($"Download failed: {ex}");
      return false;
    }

    return true;
  }

  private async Task<bool> CheckModelExists() {
    try {
      var request = new HttpRequestMessage(HttpMethod.Post, "/api/show") {
        Content = new StringContent(
          JsonSerializer.Serialize(new { name = config.Model }),
          Encoding.UTF8,
          "application/json"
        )
      };

      var response = await managementClient.SendAsync(request);
      return response.IsSuccessStatusCode;
    } catch (Exception ex) {
      logger.LogError($"Failed to check model existence: {ex.Message}");
      return false;
    }
  }

  private async Task DownloadModel(IProgressHandle progress) {
    try {
      var request = new HttpRequestMessage(HttpMethod.Post, "/api/pull") {
        Content = new StringContent(
          JsonSerializer.Serialize(new { name = config.Model }),
          Encoding.UTF8,
          "application/json"
        )
      };

      var response = await managementClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
      response.EnsureSuccessStatusCode();

      using var stream = await response.Content.ReadAsStreamAsync();
      using var reader = new StreamReader(stream);

      float lastProgress = 0f;

      while (!reader.EndOfStream) {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line)) continue;

        try {
          var status = JsonSerializer.Deserialize<OllamaStatus>(line);
          if (status != null) {
            if (!string.IsNullOrEmpty(status.Error)) {
              throw new Exception($"Ollama API Error: {status.Error}");
            }

            if (status.Total > 0 && status.Completed.HasValue) {
              lastProgress = (float)status.Completed.Value / status.Total.Value;
              progress.Report($"Downloading {config.Model}...\n{status.Status} ({lastProgress:P0})", lastProgress);
            } else if (!string.IsNullOrEmpty(status.Status)) {
              progress.Report($"Downloading {config.Model}...\n{status.Status}", lastProgress);
            }
          }
        } catch (Exception ex) {
          if (ex.Message.StartsWith("Ollama API Error")) throw;
          logger.LogError($"Failed to parse progress update: {line}");
        }
      }
    } catch (Exception ex) {
      logger.LogError($"Failed to download model: {ex.Message}");
      throw;
    }
  }

  private string GetBaseOllamaUrl() {
    var uri = new Uri(config.ApiUrl);
    return $"{uri.Scheme}://{uri.Authority}";
  }

  private class OllamaStatus {
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("total")]
    public long? Total { get; set; }

    [JsonPropertyName("completed")]
    public long? Completed { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
  }
}
