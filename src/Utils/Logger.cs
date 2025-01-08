using BepInEx.Logging;

namespace MdrgAiDialog.Utils;

/// <summary>
/// A simple logging utility that provides scoped logging capabilities
/// </summary>
public class Logger(string scope) {
  /// <summary>
  /// The underlying log source scoped to the specified name
  /// </summary>
  private readonly ManualLogSource logSource = BepInEx.Logging.Logger.CreateLogSource(scope);

  /// <summary>
  /// Logs an informational message to the current scope
  /// </summary>
  /// <param name="message">The message to log</param>
  public void Log(string message) {
    logSource.LogMessage(message);
  }

  /// <summary>
  /// Logs a warning message to the current scope
  /// </summary>
  /// <param name="message">The warning message to log</param>
  public void LogWarning(string message) {
    logSource.LogWarning(message);
  }

  /// <summary>
  /// Logs an error message to the current scope
  /// </summary>
  /// <param name="message">The error message to log</param>
  public void LogError(string message) {
    logSource.LogError(message);
  }
}
