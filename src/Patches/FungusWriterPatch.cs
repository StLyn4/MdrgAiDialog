using Fungus;
using HarmonyLib;
using MdrgAiDialog.Chat;
using MdrgAiDialog.Utils;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(Writer))]
public class FungusWriterPatch {
  [HarmonyPatch("OnNextLineEvent")]
  [HarmonyPrefix]
  public static bool BeforeOnNextLineEvent(Writer __instance) {
    ChatWriter.EventBus.Fire("user-input", __instance);
    return !Locker<Writer>.IsLocked(__instance);
  }
}
