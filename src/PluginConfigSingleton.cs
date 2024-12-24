using BepInEx.Configuration;

namespace MdrgAiDialog;

public class PluginConfigSingleton {
  private static PluginConfigSingleton s_instance;
  public static PluginConfigSingleton Instance {
    get {
      if (s_instance == null) {
        s_instance = new PluginConfigSingleton();
      }

      return s_instance;
    }
  }

  // Add configuration entries
  public string UsedAiProvider { get; private set; }
  public string AiProviderApiUrl { get; private set; }
  public string AiProviderApiKey { get; private set; }
  public string AiProviderModel { get; private set; }
  public double AiProviderTemperature { get; private set; }
  public int AiProviderTimeoutSeconds { get; private set; }

  public void Load(ConfigFile configFile) {
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
      "mistral-nemo",
      "The model to use"
    ).Value;

    AiProviderTemperature = configFile.Bind(
      "General",
      "AiProviderTemperature",
      1.05,
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
