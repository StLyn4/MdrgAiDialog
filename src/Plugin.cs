using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MdrgAiDialog.Chat;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog;

/// <summary>
/// Main plugin class that handles initialization and setup
/// </summary>
[BepInPlugin("com.delta.mdrg.aidialog", "Mod for talking to LLM", "0.2.0")]
public class Plugin : BasePlugin {
  /// <summary>
  /// Gets the singleton instance of the plugin
  /// </summary>
  public static Plugin Instance { get; private set; }

  /// <summary>
  /// Initializes the plugin, its configuration and required components
  /// </summary>
  public override void Load() {
    // Make this instance available to other classes
    Instance = this;

    // Load config
    PluginConfig.Load(Config);

    // Add singletons
    MonoSingletonManager.Add<MainThreadRunner>();
    MonoSingletonManager.Add<ChatManager>();
    MonoSingletonManager.Add<ChatWriter>();

    // Initialize Harmony
    Harmony harmony = new("com.delta.mdrg.aidialog");
    harmony.PatchAll();
  }
}
