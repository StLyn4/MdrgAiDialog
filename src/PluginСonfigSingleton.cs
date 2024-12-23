using BepInEx.Configuration;

namespace MdrgAiDialog;

public class PluginConfigSingleton {
  private static PluginConfigSingleton _instance;
  public static PluginConfigSingleton Instance {
    get {
      if (_instance == null) {
        _instance = new PluginConfigSingleton();
      }

      return _instance;
    }
  }

  // Add configuration entries
  public string usedAiProvider { get; private set; }
  public string aiProviderApiUrl { get; private set; }
  public string aiProviderApiKey { get; private set; }
  public string aiProviderModel { get; private set; }

  public void Load(ConfigFile configFile) {
    // Load config values with defaults
    usedAiProvider = configFile.Bind(
      "General",
      "UsedAiProvider",
      "Ollama",
      "The AI provider to use"
    ).Value;

    aiProviderApiUrl = configFile.Bind(
      "General",
      "AiProviderApiUrl",
      "http://localhost:11434",
      "The API URL for the AI provider"
    ).Value;

    aiProviderApiKey = configFile.Bind(
      "General",
      "AiProviderApiKey",
      "CHANGE_ME",
      "The API key for the AI provider"
    ).Value;

    aiProviderModel = configFile.Bind(
      "General",
      "AiProviderModel",
      "mistral-nemo",
      "The model to use"
    ).Value;
  }
}
