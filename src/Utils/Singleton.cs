namespace MdrgAiDialog.Utils;

/// <summary>
/// Base class for implementing the singleton pattern
/// </summary>
/// <typeparam name="T">Type of the singleton class</typeparam>
/// <remarks>
/// Usage example:
/// <code>
/// public class MyService : Singleton&lt;MyService&gt; {
///   // Must have public parameterless constructor or no constructor at all
///   // private MyService() { } - will not work!
///
///   public void DoSomething() {
///     // Service implementation
///   }
/// }
///
/// // Usage:
/// MyService.Instance.DoSomething();
/// </code>
/// </remarks>
public abstract class Singleton<T> where T : Singleton<T>, new() {
  private static T instance;
  private static bool isConstructorUnlocked = false;
  private static readonly object @lock = new();

  /// <summary>
  /// Gets the singleton instance, creating it if it doesn't exist
  /// </summary>
  /// <remarks>
  /// Note that T must have a public parameterless constructor due to new() constraint
  /// </remarks>
  public static T Instance {
    get {
      if (instance == null) {
        lock (@lock) {
          if (instance == null) {
            isConstructorUnlocked = true;
            instance = new T();
            isConstructorUnlocked = false;
          }
        }
      }
      return instance;
    }
  }

  /// <summary>
  /// Protected constructor to prevent direct instantiation
  /// </summary>
  /// <exception cref="Exception">
  /// Thrown when:
  /// - An instance already exists
  /// - Constructor is called directly instead of through Instance property
  /// </exception>
  /// <remarks>
  /// Due to new() constraint, derived classes must have public parameterless constructor
  /// or no constructor at all
  /// </remarks>
  protected Singleton() {
    if (instance != null) {
      throw new System.Exception($"An instance of {typeof(T)} already exists.");
    }

    if (!isConstructorUnlocked) {
      throw new System.Exception($"Constructor of {typeof(T)} cannot be called directly. Use {typeof(T)}.Instance instead.");
    }
  }
}
