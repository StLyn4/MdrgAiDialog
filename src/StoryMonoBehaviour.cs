using UnityEngine;
using System;

namespace MdrgAiDialog;

public abstract class StoryMonoBehaviour : MonoBehaviour {
  protected bool IsStoryActive { get; private set; } = false;
  private Il2CppSystem.Action CurrentOnFinished;

  public abstract void Story(Action stopStory);

  public void StartStory() {
    if (!IsStoryActive) {
      PrepareForStory();
      Story(StopStory);
    }
  }

  private void PrepareForStory() {
    var live2DController = Live2DControllerSingleton.Instance;
    var character = live2DController.InteractCharacter;
    var brain = character.CurrentBrain;

    brain.PrepareForStory(out var onFinished);
    IsStoryActive = true;
    CurrentOnFinished = onFinished;
  }

  private void StopStory() {
    CurrentOnFinished?.Invoke();
    CurrentOnFinished = null;
    IsStoryActive = false;
  }
}
