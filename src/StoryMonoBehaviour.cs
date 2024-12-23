using UnityEngine;
using System;

namespace MdrgAiDialog;

public abstract class StoryMonoBehaviour : MonoBehaviour {
  protected bool isStoryActive { get; private set; } = false;

  private Action stopStory;
  private Il2CppSystem.Action currentOnFinished;

  public StoryMonoBehaviour() {
    stopStory = new Action(StopStory);
  }

  public abstract void Story(Action stopStory);

  public void StartStory() {
    if (!isStoryActive) {
      PrepareForStory();
      Story(stopStory);
    }
  }

  private void PrepareForStory() {
    var live2DController = Live2DControllerSingleton.Instance;
    var character = live2DController.InteractCharacter;
    var brain = character.CurrentBrain;

    brain.PrepareForStory(out var onFinished);
    isStoryActive = true;
    currentOnFinished = onFinished;
  }

  private void StopStory() {
    currentOnFinished?.Invoke();
    currentOnFinished = null;
    isStoryActive = false;
  }
}
