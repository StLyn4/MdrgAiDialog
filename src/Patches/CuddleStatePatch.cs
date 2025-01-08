using HarmonyLib;
using MdrgAiDialog.Chat;
using System;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(CuddleState))]
public class CuddleStatePatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(CuddleState __instance) {
    var chat = ChatManager.Instance;
    var gameVariables = GameScript.Instance.GameVariables;

    var cuddleStaticGui = __instance._cuddleStaticGui;
    var buttonList = cuddleStaticGui.ButtonList;
    var talkText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    if (gameVariables.IsBotSmart) {
      var buttonWrapper = buttonList.AddButton(0);
      buttonWrapper.SetText($"{talkText} (AI)");
      buttonWrapper.OnClick = new Action(chat.StartChat);

      buttonList.UpdateAndSortButtons();
    }
  }
}
