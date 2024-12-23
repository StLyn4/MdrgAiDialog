using HarmonyLib;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(GameStateWithLive2D<Live2DInteractionController>))]
public class GameStateWithLive2DPatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(GameStateWithLive2D<Live2DInteractionController> __instance) {
    var gameState = GameStateSingleton.Instance;
    gameState.SetCurrentState(__instance);
  }

  [HarmonyPatch("ExitState")]
  [HarmonyPostfix]
  public static void AfterExitState(GameStateWithLive2D<Live2DInteractionController> __instance) {
    var gameState = GameStateSingleton.Instance;
    gameState.SetCurrentState(null);
  }
}
