using HarmonyLib;
using System;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(InteractState))]
[HarmonyPatch("EnterState")]
public class InteractStatePatch {
  [HarmonyPostfix]
  public static void Postfix(InteractState __instance) {
    var chat = ChatSingleton.Instance;
    var buttonList = __instance._buttonList;
    var talkText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    var buttonWrapper = buttonList.AddButton(0);
    buttonWrapper.SetText($"{talkText} (AI)");
    buttonWrapper.OnClick = new Action(chat.StartChat);

    buttonList.UpdateAndSortButtons();
  }
}
