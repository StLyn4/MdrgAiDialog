using System;
using HarmonyLib;
using Il2Cpp;
using MdrgAiDialog.Chat;

namespace MdrgAiDialog.Patches;

[HarmonyPatch(typeof(InteractState))]
public class InteractStatePatch {
  [HarmonyPatch("EnterState")]
  [HarmonyPostfix]
  public static void AfterEnterState(InteractState __instance) {
    var gameVariables = GameScript.Instance.GameVariables;
    if (!gameVariables.IsBotSmart) {
      return;
    }

    var chat = ChatManager.Instance;
    var buttonList = __instance._interactStaticGui.ButtonList;
    var talkText = LOS.GetLocalizedString(LOC.UI.TupleReferences.Interact_Talk);

    var buttonWrapper = buttonList.Cast<IModificationPeriodButtonList>().AddButton(0);
    buttonWrapper.SetText($"{talkText} (AI)");
    buttonWrapper.OnClick = new Action(chat.StartChat);

    buttonList.UpdateCurrentButtonState();
  }
}
