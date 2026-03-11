using HarmonyLib;
using Il2CppFungus;
using MdrgAiDialog.Chat;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(Writer))]
public class FungusWriterPatch {
  [HarmonyPatch("OnNextLineEvent", [typeof(Writer.InputFlagState)])]
  [HarmonyPrefix]
  public static bool BeforeOnNextLineEvent(Writer __instance, Writer.InputFlagState inputFlagState) {
    ChatWriter.EventBus.Fire("user-input", __instance);
    return !Locker<Writer>.IsLocked(__instance);
  }
}
