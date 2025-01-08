using System;
using System.Collections.Generic;
using UnityEngine;

namespace MdrgAiDialog.Utils;

/// <summary>
/// Marks a MonoBehaviour class as a singleton that should be managed by <see cref="MonoSingletonManager"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MonoSingletonAttribute : Attribute { }

/// <summary>
/// Manages creation and access to MonoBehaviour singletons
/// </summary>
/// <remarks>
/// Ensures that only one instance of each singleton exists and handles their lifecycle.
/// Singletons must be created through this manager to maintain proper initialization order
/// and prevent duplicate instances
/// </remarks>
public static class MonoSingletonManager {
  private static readonly Dictionary<Type, MonoBehaviour> singletons = [];
  private static readonly HashSet<Type> pendingSingletons = [];

  /// <summary>
  /// Creates a new instance of a singleton component if it doesn't exist
  /// </summary>
  /// <typeparam name="T">Type of the singleton component</typeparam>
  /// <returns>Instance of the singleton component</returns>
  /// <exception cref="Exception">Thrown when type is not marked with MonoSingleton attribute</exception>
  public static T Add<T>() where T : MonoBehaviour {
    if (Has<T>()) {
      return Get<T>();
    }

    if (!typeof(T).IsDefined(typeof(MonoSingletonAttribute), false)) {
      throw new Exception($"Type {typeof(T)} is not marked with MonoSingleton attribute");
    }

    pendingSingletons.Add(typeof(T));
    var component = Plugin.Instance.AddComponent<T>();
    UnityEngine.Object.DontDestroyOnLoad(component);
    singletons[typeof(T)] = component;
    pendingSingletons.Remove(typeof(T));

    return component;
  }

  /// <summary>
  /// Gets an existing singleton instance or creates a new one if it doesn't exist
  /// </summary>
  /// <typeparam name="T">Type of the singleton component</typeparam>
  /// <returns>Instance of the singleton component</returns>
  public static T Get<T>() where T : MonoBehaviour {
    if (!singletons.TryGetValue(typeof(T), out var singleton)) {
      singleton = Add<T>();
    }
    return (T)singleton;
  }

  /// <summary>
  /// Checks if a singleton instance exists
  /// </summary>
  /// <typeparam name="T">Type of the singleton component</typeparam>
  /// <param name="includePending">If true, also checks components that are being initialized</param>
  /// <returns>True if singleton exists (or is pending if includePending is true)</returns>
  public static bool Has<T>(bool includePending = false) where T : MonoBehaviour {
    if (singletons.ContainsKey(typeof(T))) {
      return true;
    }

    if (includePending) {
      return IsPending<T>();
    }

    return false;
  }

  /// <summary>
  /// Checks if a singleton is currently being initialized
  /// </summary>
  /// <typeparam name="T">Type of the singleton component</typeparam>
  /// <returns>True if singleton is in initialization process</returns>
  public static bool IsPending<T>() where T : MonoBehaviour {
    return pendingSingletons.Contains(typeof(T));
  }
}

/// <summary>
/// Extension methods for MonoBehaviour singletons
/// </summary>
public static class MonoSingletonExtensions {
  /// <summary>
  /// Validates that a singleton component was created through <see cref="MonoSingletonManager"/>
  /// </summary>
  /// <typeparam name="T">Type of the singleton component</typeparam>
  /// <param name="component">Component instance to validate</param>
  /// <exception cref="Exception">Thrown when component was not created through MonoSingletonManager</exception>
  public static void ValidateSingleton<T>(this T component) where T : MonoBehaviour {
    if (!MonoSingletonManager.IsPending<T>()) {
      UnityEngine.Object.Destroy(component);
      throw new Exception($"Singleton {component.GetType()} must be created through MonoSingletonManager.Add<T>()");
    }
  }
}
