using System.Collections.Generic;

namespace MdrgAiDialog.Utils;

/// <summary>
/// Generic utility class that provides locking mechanism with hit counting functionality.
/// Allows to lock objects by key and unlock them after specified number of hits
/// </summary>
/// <remarks>
/// This class only manages lock states - actual access control implementation
/// is the responsibility of the consuming code
/// </remarks>
/// <typeparam name="T">Type of the key used for locking</typeparam>
public static class Locker<T> {
  private static readonly Dictionary<T, bool> locks = [];
  private static readonly Dictionary<T, int> locksHitCounters = [];

  /// <summary>
  /// Locks an object by its key with optional hit count to unlock
  /// </summary>
  /// <param name="key">Key to lock</param>
  /// <param name="hitCountToUnlock">Number of hits required to unlock. 0 means manual unlock required</param>
  public static void Lock(T key, int hitCountToUnlock = 0) {
    locks[key] = true;
    locksHitCounters[key] = hitCountToUnlock;
  }

  /// <summary>
  /// Manually unlocks an object by its key
  /// </summary>
  /// <param name="key">Key to unlock</param>
  public static void Unlock(T key) {
    if (locks.ContainsKey(key)) {
      locks.Remove(key);
      locksHitCounters.Remove(key);
    }
  }

  /// <summary>
  /// Checks if an object is currently locked
  /// </summary>
  /// <param name="key">Key to check</param>
  /// <returns>True if the key is locked, false otherwise</returns>
  public static bool IsLocked(T key) {
    return locks.ContainsKey(key);
  }

  /// <summary>
  /// Registers a hit for the locked object. If hit count reaches specified unlock threshold, the object is unlocked
  /// </summary>
  /// <param name="key">Key to register hit for</param>
  public static void Hit(T key) {
    if (!IsLocked(key)) {
      return;
    }

    var hitCountToUnlock = locksHitCounters.GetValueOrDefault(key, 0);

    if (hitCountToUnlock == 1) {
      Unlock(key);
    } else if (hitCountToUnlock > 1) {
      locksHitCounters[key] = hitCountToUnlock - 1;
    }
  }
}
