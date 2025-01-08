namespace MdrgAiDialog.Utils;

/// <summary>
/// Static class that manages and tracks game state transitions.
/// Provides access to current and previous game states
/// </summary>
public static class GameState {
  /// <summary>
  /// Gets the current game state
  /// </summary>
  public static GameStateWithLive2D<Live2DInteractionController> CurrentState { get; private set; }

  /// <summary>
  /// Gets the previous game state before the last transition
  /// </summary>
  public static GameStateWithLive2D<Live2DInteractionController> PreviousState { get; private set; }

  /// <summary>
  /// Updates the current game state and stores the previous one
  /// </summary>
  /// <param name="state">New game state to set as current</param>
  public static void SetCurrentState(GameStateWithLive2D<Live2DInteractionController> state) {
    if (CurrentState != state) {
      PreviousState = CurrentState;
      CurrentState = state;
    }
  }

  /// <summary>
  /// Checks if the current state is of specified type
  /// </summary>
  /// <typeparam name="T">Type of state to check</typeparam>
  /// <returns>True if current state matches the specified type, false otherwise</returns>
  public static bool IsStateType<T>() where T : GameStateWithLive2D<Live2DInteractionController> {
    return CurrentState is T;
  }
}
