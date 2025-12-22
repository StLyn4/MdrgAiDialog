using System;
using System.Threading.Tasks;
using UnityEngine;
using Il2Cpp;

namespace MdrgAiDialog.Utils;

public interface IProgressHandle {
  void Report(string message, float value);
}

public static class ProgressPopupHelper {
  private static readonly Logger logger = new("ProgressPopupHelper");
  private static PrefabPopup<ProgressPopup> cachedPrefabSource;

  public static async Task Show(string title, string initialMessage, Func<IProgressHandle, Task> work) {
    ProgressPopup popup = null;

    // 1. Create and Open Popup
    await MainThreadRunner.Run(() => {
      try {
        if (cachedPrefabSource == null) {
          var popups = Resources.FindObjectsOfTypeAll<ProgressPopup>();
          if (popups != null && popups.Count > 0) {
            cachedPrefabSource = popups[0].Cast<PrefabPopup<ProgressPopup>>();
          }
        }

        if (cachedPrefabSource != null) {
          popup = cachedPrefabSource.InstantiatePrefab();
          popup.Open();

          if (popup.titleTmp != null) popup.titleTmp.text = title;
          if (popup.textTmp != null) popup.textTmp.text = initialMessage;
          if (popup.slider != null) popup.slider.value = 0;
        } else {
          logger.LogError("ProgressPopup prefab not found via Resources.FindObjectsOfTypeAll");
        }
      } catch (Exception ex) {
        logger.LogError($"Failed to instantiate ProgressPopup: {ex}");
        cachedPrefabSource = null;
      }
    });

    if (popup == null) return;

    // 2. Perform Work
    try {
      var handle = new ProgressHandleImpl(popup);
      await work(handle);
    } finally {
      // 3. Close Popup
      await MainThreadRunner.Run(() => {
        popup?.CloseFromUIOverlay();
      });
    }
  }

  private class ProgressHandleImpl(ProgressPopup popup) : IProgressHandle {
    public void Report(string message, float value) {
      MainThreadRunner.Run(() => {
        if (popup == null) return;
        if (popup.textTmp != null) popup.textTmp.text = message;
        if (popup.slider != null) popup.slider.value = value;
      });
    }
  }
}
