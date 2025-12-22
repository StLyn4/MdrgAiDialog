using HarmonyLib;
using Il2Cpp;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(GameScript))]
public class GameScriptPatch {
  [HarmonyPatch("LoadGame")]
  [HarmonyPostfix]
  public static void AfterLoadGame(GameScript __instance, GameVariables save, GameScript.LoadProcedureData lpd) {
    SaveStorage.Instance.InvalidateCache();
  }

  [HarmonyPatch("StartNewGame", [typeof(GameScript.LoadProcedureData)])]
  [HarmonyPostfix]
  public static void AfterStartNewGame(GameScript __instance, GameScript.LoadProcedureData lpd) {
    SaveStorage.Instance.InvalidateCache();
  }
}
