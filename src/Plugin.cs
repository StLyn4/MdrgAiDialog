using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace MdrgAiDialog;

[BepInPlugin("com.delta.mdrg.aidialog", "Mod for talking to LLM", "0.1.0")]
public class Plugin : BasePlugin {
  public override void Load() {
    var pluginConfig = PluginConfigSingleton.Instance;
    pluginConfig.Load(Config);

    AddComponent<ChatSingleton>();

    // Initialize Harmony
    Harmony harmony = new Harmony("com.delta.mdrg.aidialog");
    harmony.PatchAll();
  }
}
