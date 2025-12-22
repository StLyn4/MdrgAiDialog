using MelonLoader;

namespace MdrgAiDialog.Utils;

/// <summary>
/// A simple logging utility that provides scoped logging capabilities
/// </summary>
public class Logger {
  private readonly string scope;

  public Logger(string scope) {
    this.scope = scope;
  }

  /// <summary>
  /// Logs an informational message to the current scope
  /// </summary>
  /// <param name="message">The message to log</param>
  public void Log(string message) {
    MelonLogger.Msg($"[{scope}] {message}");
  }

  /// <summary>
  /// Logs a warning message to the current scope
  /// </summary>
  /// <param name="message">The warning message to log</param>
  public void LogWarning(string message) {
    MelonLogger.Warning($"[{scope}] {message}");
  }

  /// <summary>
  /// Logs an error message to the current scope
  /// </summary>
  /// <param name="message">The error message to log</param>
  public void LogError(string message) {
    MelonLogger.Error($"[{scope}] {message}");
  }
}
