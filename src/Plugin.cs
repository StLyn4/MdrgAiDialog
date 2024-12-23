using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace MdrgAiDialog;

[BepInPlugin("com.delta.mdrg.ai-dialog", "Mod for talking to LLM", "0.1.0")]
public class Plugin : BasePlugin {
  public override void Load() {
    AddComponent<ChatSingleton>();
    
    // Initialize Harmony
    Harmony harmony = new Harmony("com.delta.mdrg.ai-dialog");
    harmony.PatchAll();
  }
}
