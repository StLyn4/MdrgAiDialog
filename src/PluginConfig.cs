using BepInEx.Configuration;

namespace MdrgAiDialog;

/// <summary>
/// Manages plugin configuration settings
/// </summary>
public static class PluginConfig {
  /// <summary>
  /// Gets the name of the AI provider to use
  /// </summary>
  public static string UsedAiProvider { get; private set; }

  /// <summary>
  /// Gets the API URL for the AI provider
  /// </summary>
  public static string AiProviderApiUrl { get; private set; }

  /// <summary>
  /// Gets the API key for authentication with the AI provider
  /// </summary>
  public static string AiProviderApiKey { get; private set; }

  /// <summary>
  /// Gets the model name to use with the AI provider
  /// </summary>
  public static string AiProviderModel { get; private set; }

  /// <summary>
  /// Gets the temperature setting for AI response generation
  /// </summary>
  public static double AiProviderTemperature { get; private set; }

  /// <summary>
  /// Gets the timeout in seconds for AI provider requests
  /// </summary>
  public static int AiProviderTimeoutSeconds { get; private set; }

  /// <summary>
  /// Loads configuration values from the config file
  /// </summary>
  /// <param name="configFile">BepInEx configuration file</param>
  public static void Load(ConfigFile configFile) {
    // Load config values with defaults
    UsedAiProvider = configFile.Bind(
      "General",
      "UsedAiProvider",
      "Ollama",
      "The AI provider to use"
    ).Value;

    AiProviderApiUrl = configFile.Bind(
      "General",
      "AiProviderApiUrl",
      "http://localhost:11434",
      "The API URL for the AI provider"
    ).Value;

    AiProviderApiKey = configFile.Bind(
      "General",
      "AiProviderApiKey",
      "CHANGE_ME",
      "The API key for the AI provider"
    ).Value;

    AiProviderModel = configFile.Bind(
      "General",
      "AiProviderModel",
      "artifish/llama3.2-uncensored",
      "The model to use"
    ).Value;

    AiProviderTemperature = configFile.Bind(
      "General",
      "AiProviderTemperature",
      0.8,
      "The temperature to use"
    ).Value;

    AiProviderTimeoutSeconds = configFile.Bind(
      "AI Provider",
      "TimeoutSeconds",
      300,
      "Timeout in seconds for AI provider requests"
    ).Value;
  }
}
