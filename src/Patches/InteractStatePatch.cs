using HarmonyLib;
using MdrgAiDialog.Chat;
using System;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(InteractState))]
public class InteractStatePatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(InteractState __instance) {
    var gameVariables = GameScript.Instance.GameVariables;
    var chat = ChatManager.Instance;

    var buttonList = __instance._buttonList;
    var talkText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    if (gameVariables.IsBotSmart) {
      var buttonWrapper = buttonList.AddButton(0);
      buttonWrapper.SetText($"{talkText} (AI)");
      buttonWrapper.OnClick = new Action(chat.StartChat);

      buttonList.UpdateAndSortButtons();
    }
  }
}
