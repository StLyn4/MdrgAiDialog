namespace MdrgAiDialog;

public class GameStateSingleton {
  private GameStateSingleton() { }

  private static GameStateSingleton s_instance;
  public static GameStateSingleton Instance {
    get {
      if (s_instance == null) {
        s_instance = new GameStateSingleton();
      }

      return s_instance;
    }
  }

  public GameStateWithLive2D<Live2DInteractionController> CurrentState { get; private set; }
  public GameStateWithLive2D<Live2DInteractionController> PreviousState { get; private set; }

  public void SetCurrentState(GameStateWithLive2D<Live2DInteractionController> state) {
    if (CurrentState != state) {
      PreviousState = CurrentState;
      CurrentState = state;
    }
  }

  public bool IsStateType<T>() where T : GameStateWithLive2D<Live2DInteractionController> {
    return CurrentState is T;
  }
}
