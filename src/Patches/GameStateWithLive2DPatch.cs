using HarmonyLib;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(GameStateWithLive2D<Live2DInteractionController>))]
public class GameStateWithLive2DPatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(GameStateWithLive2D<Live2DInteractionController> __instance) {
    Utils.GameState.SetCurrentState(__instance);
  }
}
