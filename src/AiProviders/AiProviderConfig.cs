namespace MdrgAiDialog.AiProviders;

/// <summary>
/// Configuration for an AI provider
/// </summary>
public class AiProviderConfig {
  /// <summary>
  /// API URL (e.g. https://api.openai.com/v1)
  /// </summary>
  public string ApiUrl { get; set; }

  /// <summary>
  /// API Key for authentication
  /// </summary>
  public string ApiKey { get; set; }

  /// <summary>
  /// Model name to use
  /// </summary>
  public string Model { get; set; }

  /// <summary>
  /// Temperature for response generation (0.0 to 2.0)
  /// </summary>
  public double Temperature { get; set; }

  /// <summary>
  /// Top-K sampling (optional, -1 if unused)
  /// </summary>
  public int TopK { get; set; }

  /// <summary>
  /// Whether reasoning/thought process is enabled (if supported)
  /// </summary>
  public bool? ReasoningEnabled { get; set; }

  /// <summary>
  /// Whether to pre-fill the reasoning tag to suppress/guide thinking
  /// </summary>
  public bool ReasoningPreFill { get; set; }

  /// <summary>
  /// Request timeout in seconds
  /// </summary>
  public int TimeoutSeconds { get; set; }
}
