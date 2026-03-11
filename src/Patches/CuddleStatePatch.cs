using System;
using HarmonyLib;
using Il2Cpp;
using MdrgAiDialog.Chat;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(CuddleState))]
public class CuddleStatePatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(CuddleState __instance) {
    var gameVariables = GameScript.Instance.GameVariables;
    if (!gameVariables.IsBotSmart) {
      return;
    }

    var chat = ChatManager.Instance;
    var buttonList = __instance._cuddleStaticGui.ButtonList;
    var cuddleText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    var buttonWrapper = buttonList.Cast<IModificationPeriodButtonList>().AddButton(0);
    buttonWrapper.SetText($"{cuddleText} (AI)");
    buttonWrapper.OnClick = new Action(chat.StartChat);

    buttonList.UpdateCurrentButtonState();
  }
}
