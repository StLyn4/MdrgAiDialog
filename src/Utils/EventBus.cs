using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MdrgAiDialog.Utils;

/// <summary>
/// An event bus that provides a centralized event handling system.
/// Supports event capturing, one-time listeners, and async event waiting
/// </summary>
public class EventBus {
  // Dictionary to store event handlers. Key is event name, value is a set of handlers
  private readonly ConcurrentDictionary<string, ConcurrentDictionary<Delegate, byte>> eventHandlers = new();
  // Set of handlers that should be removed after first execution
  private readonly ConcurrentDictionary<Delegate, byte> oneTimeHandlers = new();

  // Set of events that should be captured when fired
  private readonly ConcurrentDictionary<string, byte> eventsToCapture = new();
  // Dictionary to store captured events and their data
  private readonly ConcurrentDictionary<string, object> capturedEvents = new();

  private readonly Logger logger = new("EventBus");

  /// <summary>
  /// Fires an event without parameters
  /// </summary>
  /// <param name="eventName">Name of the event to fire</param>
  public void Fire(string eventName) {
    // Save event if it was marked for capture
    if (eventsToCapture.ContainsKey(eventName)) {
      capturedEvents[eventName] = null;
    }

    // Execute handlers if any
    if (eventHandlers.TryGetValue(eventName, out var handlers)) {
      foreach (var (handler, _) in handlers) {
        try {
          if (oneTimeHandlers.TryRemove(handler, out _)) {
            RemoveListener(eventName, (EventHandler)handler);
          }

          ((EventHandler)handler)();
        } catch (Exception e) {
          OnError(eventName, e);
        }
      }
    }
  }

  /// <summary>
  /// Fires an event with parameters
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  /// <param name="eventName">Name of the event to fire</param>
  /// <param name="args">Event data to pass to handlers</param>
  public void Fire<T>(string eventName, T args) {
    // Save event if it was marked for capture
    if (eventsToCapture.ContainsKey(eventName)) {
      capturedEvents[eventName] = args;
    }

    // Execute handlers if any
    if (eventHandlers.TryGetValue(eventName, out var handlers)) {
      foreach (var (handler, _) in handlers) {
        try {
          if (oneTimeHandlers.TryRemove(handler, out _)) {
            RemoveListener(eventName, (EventHandler<T>)handler);
          }

          ((EventHandler<T>)handler)(args);
        } catch (Exception e) {
          OnError(eventName, e);
        }
      }
    }
  }

  /// <summary>
  /// Adds an event listener that will be called every time the event is fired
  /// </summary>
  /// <param name="eventName">Name of the event to listen for</param>
  /// <param name="handler">Handler to be called when event is fired</param>
  public void AddListener(string eventName, EventHandler handler) {
    var handlers = eventHandlers.GetOrAdd(eventName, _ => new());
    handlers.TryAdd(handler, 0);
    logger.LogWarning($"[Total listeners] {eventName} -> {handlers.Count}");
  }

  /// <summary>
  /// Adds an event listener for events with data that will be called every time the event is fired
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  /// <param name="eventName">Name of the event to listen for</param>
  /// <param name="handler">Handler to be called when event is fired</param>
  public void AddListener<T>(string eventName, EventHandler<T> handler) {
    var handlers = eventHandlers.GetOrAdd(eventName, _ => new());
    handlers.TryAdd(handler, 0);
    logger.LogWarning($"[Total listeners] {eventName} -> {handlers.Count}");
  }

  /// <summary>
  /// Adds an event listener that will be automatically removed after first execution
  /// </summary>
  /// <param name="eventName">Name of the event to listen for</param>
  /// <param name="handler">Handler to be called when event is fired</param>
  public void AddOneTimeListener(string eventName, EventHandler handler) {
    oneTimeHandlers.TryAdd(handler, 0);
    AddListener(eventName, handler);
  }

  /// <summary>
  /// Adds an event listener for events with data that will be automatically removed after first execution
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  /// <param name="eventName">Name of the event to listen for</param>
  /// <param name="handler">Handler to be called when event is fired</param>
  public void AddOneTimeListener<T>(string eventName, EventHandler<T> handler) {
    oneTimeHandlers.TryAdd(handler, 0);
    AddListener(eventName, handler);
  }

  /// <summary>
  /// Removes an event listener
  /// </summary>
  /// <param name="eventName">Name of the event</param>
  /// <param name="handler">Handler to remove</param>
  public void RemoveListener(string eventName, EventHandler handler) {
    if (eventHandlers.TryGetValue(eventName, out var handlers)) {
      handlers.TryRemove(handler, out _);
      if (handlers.IsEmpty) {
        eventHandlers.TryRemove(eventName, out _);
      }
    }
  }

  /// <summary>
  /// Removes an event listener for events with data
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  /// <param name="eventName">Name of the event</param>
  /// <param name="handler">Handler to remove</param>
  public void RemoveListener<T>(string eventName, EventHandler<T> handler) {
    if (eventHandlers.TryGetValue(eventName, out var handlers)) {
      handlers.TryRemove(handler, out _);
      if (handlers.IsEmpty) {
        eventHandlers.TryRemove(eventName, out _);
      }
    }
  }

  /// <summary>
  /// Marks an event for capture. The next time this event is fired, its data will be saved
  /// and can be retrieved by WaitFor instead of waiting for a new event
  /// </summary>
  /// <param name="eventName">Name of the event to capture</param>
  public void Capture(string eventName) {
    eventsToCapture.TryAdd(eventName, 0);
  }

  /// <summary>
  /// Releases event capture, stopping the event from being captured
  /// </summary>
  /// <param name="eventName">Name of the event to release</param>
  public void ReleaseCapture(string eventName) {
    eventsToCapture.TryRemove(eventName, out _);
  }

  /// <summary>
  /// Waits for an event to be fired. If the event was captured, returns immediately
  /// </summary>
  /// <param name="eventName">Name of the event to wait for</param>
  public async Task WaitFor(string eventName) {
    ReleaseCapture(eventName);
    if (capturedEvents.TryRemove(eventName, out _)) {
      return;
    }

    var tcs = new TaskCompletionSource<object>();
    AddOneTimeListener(eventName, () => tcs.TrySetResult(null));
    await tcs.Task;
  }

  /// <summary>
  /// Waits for an event with data to be fired. If the event was captured, returns its data immediately
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  /// <param name="eventName">Name of the event to wait for</param>
  /// <returns>Event data of type T</returns>
  public async Task<T> WaitFor<T>(string eventName) {
    ReleaseCapture(eventName);
    if (capturedEvents.TryRemove(eventName, out var capturedEvent)) {
      return capturedEvent is T typedEvent ? typedEvent : default;
    }

    var tcs = new TaskCompletionSource<object>();
    AddOneTimeListener<T>(eventName, args => tcs.TrySetResult(args));
    var result = await tcs.Task;
    return result is T typedResult ? typedResult : default;
  }

  private void OnError(string eventName, Exception exception) {
    throw new EventException($"Error handling event '{eventName}'", exception);
  }

  /// <summary>
  /// Delegate for event handlers without parameters
  /// </summary>
  public delegate void EventHandler();

  /// <summary>
  /// Delegate for event handlers with parameters
  /// </summary>
  /// <typeparam name="T">Type of the event data</typeparam>
  public delegate void EventHandler<T>(T args);

  /// <summary>
  /// Exception thrown when an error occurs during event handling
  /// </summary>
  public class EventException(string message, Exception innerException) : Exception(message, innerException) { }
}
