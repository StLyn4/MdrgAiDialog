using Fungus;
using HarmonyLib;
using MdrgAiDialog.Chat;
using System;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(SayDialog))]
public class FungusSayDialogPatch {
  [HarmonyPatch("DoSay")]
  [HarmonyPrefix]
  public static void BeforeDoSay(SayDialog __instance, string text, bool clearPrevious, bool waitForInput, bool fadeWhenDone, Action onComplete) {
    ChatWriter.EventBus.Fire("say-dialog-changed", __instance);
  }
}
