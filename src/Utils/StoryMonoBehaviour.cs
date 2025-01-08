using UnityEngine;
using System;

namespace MdrgAiDialog.Utils;

/// <summary>
/// Base class for MonoBehaviours that implement story-based interactions
/// </summary>
/// <remarks>
/// Provides a framework for managing story state and character interactions.
/// Derived classes must implement the Story method to define their specific story logic
/// </remarks>
public abstract class StoryMonoBehaviour : MonoBehaviour {
  /// <summary>
  /// Indicates whether a story is currently active
  /// </summary>
  protected bool IsStoryActive { get; private set; } = false;
  private Il2CppSystem.Action currentOnFinished;

  /// <summary>
  /// Defines the story logic to be executed
  /// </summary>
  /// <param name="stopStory">Action to call when the story should end</param>
  /// <remarks>
  /// Implement this method in derived classes to define the story sequence.
  /// Call stopStory when the story is complete to properly cleanup resources
  /// </remarks>
  protected abstract void Story(Action stopStory);

  /// <summary>
  /// Begins the story sequence if one is not already active
  /// </summary>
  protected void StartStory() {
    if (!IsStoryActive) {
      PrepareForStory();
      Story(StopStory);
    }
  }

  /// <summary>
  /// Prepares the character for story interaction
  /// </summary>
  private void PrepareForStory() {
    var live2DController = Live2DControllerSingleton.Instance;
    var character = live2DController.InteractCharacter;
    var brain = character.CurrentBrain;

    brain.PrepareForStory(out var onFinished);
    IsStoryActive = true;
    currentOnFinished = onFinished;
  }

  /// <summary>
  /// Ends the story sequence and performs cleanup
  /// </summary>
  private void StopStory() {
    currentOnFinished?.Invoke();
    currentOnFinished = null;
    IsStoryActive = false;
  }
}
