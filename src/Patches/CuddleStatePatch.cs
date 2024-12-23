using HarmonyLib;
using System;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(CuddleState))]
[HarmonyPatch("EnterState")]
public class CuddleStatePatch {
  [HarmonyPostfix]
  public static void Postfix(CuddleState __instance) {
    var chat = ChatSingleton.Instance;
    var cuddleStaticGui = __instance._cuddleStaticGui;
    var buttonList = cuddleStaticGui.ButtonList;
    var talkText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    var buttonWrapper = buttonList.AddButton(0);
    buttonWrapper.SetText($"{talkText} (AI)");
    buttonWrapper.OnClick = new Action(chat.StartChat);

    buttonList.UpdateAndSortButtons();
  }
}
