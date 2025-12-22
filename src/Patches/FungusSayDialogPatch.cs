using System;
using HarmonyLib;
using Il2CppFungus;
using MdrgAiDialog.Chat;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(SayDialog))]
public class FungusSayDialogPatch {
  [HarmonyPatch("DoSay")]
  [HarmonyPrefix]
  public static void BeforeDoSay(SayDialog __instance, string text, bool clearPrevious, bool waitForInput, bool fadeWhenDone, Action onComplete) {
    ChatWriter.EventBus.Fire("say-dialog-changed", __instance);
  }
}
