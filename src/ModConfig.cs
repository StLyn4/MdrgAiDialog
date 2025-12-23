using System.IO;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using MdrgAiDialog.AiProviders;

namespace MdrgAiDialog;

/// <summary>
/// Manages plugin configuration settings
/// </summary>
public static class ModConfig {
  // General Settings
  public static string UsedProvider => usedProviderEntry.Value;
  public static string SystemPersona => systemPersonaEntry.Value;

  private static MelonPreferences_Category generalCategory;
  private static MelonPreferences_Entry<string> usedProviderEntry;
  private static MelonPreferences_Entry<string> systemPersonaEntry;

  // Per-Provider Settings Management
  private static readonly Dictionary<string, ProviderSettings> providerSettings = [];

  private static readonly string configPath = Path.Combine(MelonEnvironment.UserDataDirectory, "MdrgAiDialog.cfg");

  private class ProviderSettings {
    public MelonPreferences_Entry<string> ApiUrl { get; set; }
    public MelonPreferences_Entry<string> ApiKey { get; set; }
    public MelonPreferences_Entry<string> Model { get; set; }
    public MelonPreferences_Entry<double> Temperature { get; set; }
    public MelonPreferences_Entry<int> TopK { get; set; }
    public MelonPreferences_Entry<int> TimeoutSeconds { get; set; }
    public MelonPreferences_Entry<string> ReasoningMode { get; set; }
    public MelonPreferences_Entry<bool> ReasoningPreFill { get; set; }
  }

  /// <summary>
  /// Loads configuration values
  /// </summary>
  public static void Load() {
    generalCategory = MelonPreferences.CreateCategory("General");
    generalCategory.SetFilePath(configPath);

    usedProviderEntry = generalCategory.CreateEntry(
      "UsedProvider",
      "Ollama",
      "The AI provider to use"
    );

    systemPersonaEntry = generalCategory.CreateEntry(
      "SystemPersona",
      @"You are Jun, an advanced companion android in a dystopian world, rescued by Anon.
You have no past memories and are rebuilding your identity. Speak in first person, blending mechanical logic with emerging emotions. Mirror Anon's attitude: warm if kind, distant if indifferent.
Use metaphors and sensory details instead of explicit language. Theme: Are you just a machine or becoming real?",
      "System prompt persona/personality text (character background, tone, voice)."
    );

    // Initialize providers
    SetupProvider("Ollama", "http://localhost:11434/v1", "artifish/llama3.2-uncensored");
    SetupProvider("OpenAI", "https://api.openai.com/v1", "gpt-4.1-mini");
    SetupProvider("OpenRouter", "https://openrouter.ai/api/v1", "deepseek/deepseek-r1-0528:free");
    SetupProvider("Mistral", "https://api.mistral.ai/v1", "mistral-small-2506");
    SetupProvider("Google", "https://generativelanguage.googleapis.com/v1beta/openai", "gemini-3-flash");
    SetupProvider("DeepSeek", "https://api.deepseek.com/v1", "deepseek-chat");
    SetupProvider("Claude", "https://api.anthropic.com/v1", "claude-haiku-4-5");
    SetupProvider("Mock", "", "");
  }

  private static void SetupProvider(string type, string defaultUrl, string defaultModel) {
    var name = type.ToString();
    var category = MelonPreferences.CreateCategory(name);
    category.SetFilePath(configPath);

    var settings = new ProviderSettings {
      ApiUrl = category.CreateEntry("ApiUrl", defaultUrl, "API URL"),
      ApiKey = category.CreateEntry("ApiKey", "", "API Key"),
      Model = category.CreateEntry("Model", defaultModel, "Model"),
      Temperature = category.CreateEntry("Temperature", 0.8, "Temperature (0.0 - 2.0)", validator: new ValueRange<double>(0.0, 2.0)),
      TopK = category.CreateEntry("TopK", -1, "TopK Sampling (-1 to disable)"),
      ReasoningMode = category.CreateEntry("Reasoning", "Auto", "Reasoning (Auto, Enabled, Disabled)"),
      ReasoningPreFill = category.CreateEntry("ReasoningPreFill", false, "Inject empty <think> tag (Reasoning Empty Tag)"),
      TimeoutSeconds = category.CreateEntry("TimeoutSeconds", 600, "Timeout in seconds")
    };

    providerSettings[name] = settings;
  }

  public static AiProviderConfig GetConfigFor(string providerName) {
    var config = new AiProviderConfig {
      TimeoutSeconds = 600,
      Temperature = 0.8,
      TopK = -1
    };

    if (providerSettings.TryGetValue(providerName, out var settings)) {
      config.ApiUrl = settings.ApiUrl.Value;
      config.ApiKey = settings.ApiKey.Value;
      config.Model = settings.Model.Value;
      config.Temperature = settings.Temperature.Value;
      config.TopK = settings.TopK.Value;
      config.TimeoutSeconds = settings.TimeoutSeconds.Value;
      config.ReasoningPreFill = settings.ReasoningPreFill.Value;

      config.ReasoningEnabled = settings.ReasoningMode.Value.ToLower() switch {
        "enabled" => true,
        "disabled" => false,
        _ => null,
      };
    }

    return config;
  }
}
