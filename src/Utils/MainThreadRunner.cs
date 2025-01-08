using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Il2CppInterop.Runtime;

namespace MdrgAiDialog.Utils;

/// <summary>
/// Provides functionality to execute code on Unity's main thread
/// </summary>
/// <remarks>
/// Unity requires certain operations to be performed on the main thread.
/// This class ensures actions are executed in the correct context by using
/// various synchronization mechanisms depending on the current environment
/// </remarks>
[MonoSingleton]
public class MainThreadRunner : MonoBehaviour {
  public static MainThreadRunner Instance => MonoSingletonManager.Get<MainThreadRunner>();

  private static readonly ConcurrentQueue<(Action action, TaskCompletionSource tcs)> actions = new();
  private static SynchronizationContext mainThreadContext;
  private static Il2CppSystem.Threading.SynchronizationContext unitySynchronizationContext;

  /// <summary>
  /// Initializes synchronization contexts when the component awakens
  /// </summary>
  public void Awake() {
    this.ValidateSingleton();
    mainThreadContext = SynchronizationContext.Current;
    unitySynchronizationContext = UnitySynchronizationContext.Current;
  }

  /// <summary>
  /// Executes actions queued in <see cref="RunInNextUpdate">RunInNextUpdate</see> on each frame update
  /// </summary>
  public void Update() {
    while (actions.TryDequeue(out var work)) {
      try {
        work.action();
        work.tcs.SetResult();
      } catch (Exception e) {
        work.tcs.SetException(e);
      }
    }
  }

  /// <summary>
  /// Executes an action on the main thread
  /// </summary>
  /// <param name="action">Action to execute</param>
  /// <returns>Task that completes when the action has been executed</returns>
  /// <remarks>
  /// The method automatically chooses the best available synchronization mechanism:
  /// - Executes immediately if already on main thread
  /// - Uses managed synchronization context if available
  /// - Uses Unity's synchronization context if available
  /// - Falls back to queuing for next Update call
  /// </remarks>
  public static Task Run(Action action) {
    if (IsMainThread()) {
      return RunInPlace(action);
    }

    if (mainThreadContext != null) {
      return RunInMainThreadContext(action);
    } else if (unitySynchronizationContext != null) {
      return RunInUnitySynchronizationContext(action);
    } else {
      return RunInNextUpdate(action);
    }
  }

  /// <summary>
  /// Checks if the current code is executing on the main thread
  /// </summary>
  private static bool IsMainThread() {
    return Environment.CurrentManagedThreadId == 1;
  }

  /// <summary>
  /// Executes action immediately on the current thread
  /// </summary>
  private static Task RunInPlace(Action action) {
    try {
      action();
      return Task.CompletedTask;
    } catch (Exception e) {
      return Task.FromException(e);
    }
  }

  /// <summary>
  /// Executes action using the managed synchronization context
  /// </summary>
  private static Task RunInMainThreadContext(Action action) {
    var tcs = new TaskCompletionSource();

    mainThreadContext.Post((state) => {
      try {
        action();
        tcs.TrySetResult();
      } catch (Exception ex) {
        tcs.TrySetException(ex);
      }
    }, null);

    return tcs.Task;
  }

  /// <summary>
  /// Executes action using Unity's synchronization context
  /// </summary>
  private static Task RunInUnitySynchronizationContext(Action action) {
    var tcs = new TaskCompletionSource();

    void wrappedAction(Il2CppSystem.Object state) {
      try {
        action();
        tcs.SetResult();
      } catch (Exception e) {
        tcs.SetException(e);
      }
    }

    // It is necessary to convert .NET Action to Il2CppAction
    var callback = DelegateSupport.ConvertDelegate<Il2CppSystem.Threading.SendOrPostCallback>(wrappedAction);
    unitySynchronizationContext.Post(callback, null);

    return tcs.Task;
  }

  /// <summary>
  /// Queues action for execution in the next Update call
  /// </summary>
  private static Task RunInNextUpdate(Action action) {
    var tcs = new TaskCompletionSource();

    lock (actions) {
      actions.Enqueue((action, tcs));
    }

    return tcs.Task;
  }
}
