using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils;
using MdrgAiDialog.Utils;
using UnityEngine;

namespace MdrgAiDialog.Chat;

/// <summary>
/// Handles text display and animation in the game's dialog system
/// </summary>
/// <remarks>
/// Manages text rendering, character-by-character animation, and callback execution.
/// Integrates with Fungus dialog system for visual novel style text display
/// </remarks>
[MonoSingleton]
public class ChatWriter : MonoBehaviour {
  public static ChatWriter Instance => MonoSingletonManager.Get<ChatWriter>();
  public static EventBus EventBus { get; private set; } = new();

  private bool isReady = false;
  private bool isFlushing = false;
  private bool isPaused = false;
  private bool isStopped = false;
  private bool isWaitingForCallback = false;

  private Fungus.SayDialog sayDialog;
  private readonly StringBuilder textBuffer = new(1024);
  private float nextCharacterTime = 0f;
  private int visibleCharactersCount = 0;
  private int textStart = 0; // Index of the first character to be printed
  private int textEnd = 0; // Position (not index) of the last character to be printed

  private bool needUpdateText = false;
  private bool needUpdateVisibleCharacters = false;

  private readonly Action<Fungus.SayDialog> onDialogFinished;
  private readonly Action<Fungus.Writer, Fungus.WriterState> onWriterState;

  private readonly Queue<(int Position, Func<Task> Callback)> pendingCallbacks = new();
  private (int Position, Func<Task> Callback)? nextCallback;

  /// <summary>
  /// Initializes event handlers
  /// </summary>
  public ChatWriter() : base() {
    onDialogFinished = new(OnDialogFinished);
    onWriterState = new(OnWriterState);
  }

  /// <summary>
  /// Validates singleton initialization
  /// </summary>
  public void Awake() {
    this.ValidateSingleton();
  }

  /// <summary>
  /// Subscribes to dialog events
  /// </summary>
  public void Start() {
    Fungus.SayDialogSignals.OnDialogFinished += onDialogFinished;
    Fungus.WriterSignals.OnWriterState += onWriterState;
  }

  /// <summary>
  /// Unsubscribes from dialog events
  /// </summary>
  public void OnDestroy() {
    Fungus.SayDialogSignals.OnDialogFinished -= onDialogFinished;
    Fungus.WriterSignals.OnWriterState -= onWriterState;
  }

  /// <summary>
  /// Updates text animation and executes callbacks
  /// </summary>
  public void Update() {
    if (needUpdateText) {
      needUpdateText = false;

      var lengthAfterStart = textBuffer.Length - textStart;
      var newText = textBuffer.ToString(textStart, lengthAfterStart).Trim();
      sayDialog.writer.targetTextTMP.text = newText;
    }

    if (needUpdateVisibleCharacters) {
      needUpdateVisibleCharacters = false;

      sayDialog.writer.targetTextTMP.maxVisibleCharacters = visibleCharactersCount;
      sayDialog.writer.visibleCharacterCount = visibleCharactersCount;

      if (visibleCharactersCount > 0) {
        var lastCharacter = textBuffer[textEnd - 1];
        sayDialog.writer.NotifyGlyph(lastCharacter);
      }
    }

    if (isFlushing && IsAllTextPrinted()) {
      isFlushing = false;
      EventBus.Fire("text-flushed");
      return;
    }

    if (isReady && !isPaused && !isStopped && !isWaitingForCallback) {
      var writingSpeed = sayDialog.writer.writingSpeed;
      var invWritingSpeed = 1f / writingSpeed;

      while (Time.time >= nextCharacterTime && !IsAllTextPrinted() && !isWaitingForCallback) {
        this.StartCoroutine(RevealNextCharacter());
        nextCharacterTime = Time.time + invWritingSpeed;
      }
    }
  }

  /// <summary>
  /// Prepares the writer for a new message
  /// </summary>
  public async Task Prepare() {
    ResetState();

    // Set traps for those events so we don't miss any
    EventBus.Capture("say-dialog-changed");
    EventBus.Capture("ready-to-start");

    await MainThreadRunner.Run(() => {
      StartCoroutine(BetterConversationManager.DoConversation("Bot: {s=1000}...{/s}"));
    });

    sayDialog = await EventBus.WaitFor<Fungus.SayDialog>("say-dialog-changed");
    Locker<Fungus.Writer>.Lock(sayDialog.writer);

    // Wait for {wi} tag to be reached
    await EventBus.WaitFor("ready-to-start");
    isReady = true;
  }

  /// <summary>
  /// Adds text to the buffer
  /// </summary>
  /// <param name="symbols">Text to add</param>
  public void Type(string symbols) {
    if (!isReady) {
      // The writer is not ready. This could mean either that
      // the user did not call Prepare(), or that
      // some command aborted execution before all Type() were called.
      return;
    }

    if (textBuffer.Length == 0) {
      // During the first call, we need to synchronize visible characters,
      // to hide all characters and avoid flickering
      SyncVisibleCharacters();
    }

    textBuffer.Append(symbols);
    SyncTmpText();
  }

  /// <summary>
  /// Adds a callback to be executed at current text position
  /// </summary>
  /// <param name="callback">Callback to execute</param>
  public async Task AddCallback(Func<Task> callback) {
    var position = textBuffer.Length;

    if (textEnd >= position) {
      // If text is already printed beyond the callback position, execute it immediately
      Pause();
      await callback();
      Resume();
      return;
    }

    pendingCallbacks.Enqueue((position, callback));
  }

  /// <summary>
  /// Flushes remaining text and executes pending callbacks
  /// </summary>
  public async Task Flush() {
    try {
      isFlushing = true;

      if (!IsAllTextPrinted()) {
        // TODO: Listeners pollute the bus if the writer is interrupted via #!flow.ExitChat
        await EventBus.WaitFor("text-flushed");
      }

      // Execute remaining callbacks if any
      await ExecuteCallbacks();

      if (sayDialog?.writer != null) {
        Locker<Fungus.Writer>.Unlock(sayDialog.writer);
      }

      await EventBus.WaitFor("dialog-finished");
    } finally {
      ResetState();
    }
  }

  /// <summary>
  /// Pauses text animation
  /// </summary>
  public void Pause() {
    isPaused = true;
  }

  /// <summary>
  /// Resumes text animation
  /// </summary>
  public void Resume() {
    isPaused = false;
  }

  /// <summary>
  /// Waits for user input on current dialog
  /// </summary>
  public async Task WaitForInput() {
    while (true) {
      var writer = await EventBus.WaitFor<Fungus.Writer>("user-input");

      if (writer == sayDialog?.writer) {
        // Break the loop only if the writer is the one we are waiting for
        break;
      }
    }
  }

  /// <summary>
  /// Stops text animation and optionally waits for input
  /// </summary>
  /// <param name="waitForInput">If true, waits for user input before stopping</param>
  public async Task Stop(bool waitForInput = false) {
    isStopped = true;

    if (waitForInput) {
      await WaitForInput();
    }

    sayDialog?.Stop();
    ResetState();
  }

  /// <summary>
  /// Clears current text and adds it to narrative log
  /// </summary>
  public void Clear() {
    var currentText = textBuffer.ToString(textStart, visibleCharactersCount);
    ChatManager.Instance.AddToNarrativeLog("Bot", currentText);

    textStart = textEnd;
    visibleCharactersCount = 0;

    SyncTmpText();
    SyncVisibleCharacters();
  }

  private IEnumerator RevealNextCharacter() {
    if (!IsAllTextPrinted()) {
      IncrementPositionCounters();
      SyncVisibleCharacters();
      yield return ExecuteCallbacksCoroutine();
    }
  }

  private void IncrementPositionCounters() {
    visibleCharactersCount++;
    textEnd++;
  }

  private void SyncTmpText() {
    needUpdateText = true;
  }

  private void SyncVisibleCharacters() {
    needUpdateVisibleCharacters = true;
  }

  private async Task ExecuteCallbacks() {
    // Prepare next callback if needed
    if (nextCallback == null && pendingCallbacks.Count > 0) {
      nextCallback = pendingCallbacks.Dequeue();
    }

    while (nextCallback != null && nextCallback?.Position <= textEnd) {
      await nextCallback?.Callback();
      nextCallback = pendingCallbacks.Count > 0 ? pendingCallbacks.Dequeue() : null;
    }
  }

  private IEnumerator ExecuteCallbacksCoroutine() {
    isWaitingForCallback = true;

    try {
      var task = ExecuteCallbacks();
      while (!task.IsCompleted) {
        yield return null;
      }
    } finally {
      isWaitingForCallback = false;
    }
  }

  private bool IsAllTextPrinted() {
    return textEnd >= textBuffer.Length;
  }

  private void OnDialogFinished(Fungus.SayDialog dialog) {
    if (dialog != null && dialog == sayDialog) {
      EventBus.Fire("dialog-finished");
    }
  }

  private void OnWriterState(Fungus.Writer writer, Fungus.WriterState state) {
    if (writer == null || sayDialog == null || writer != sayDialog.writer) {
      return;
    }

    if (state == Fungus.WriterState.PauseWaitForInput) {
      EventBus.Fire("ready-to-start");
    }
  }

  private void ResetState() {
    isReady = false;
    isFlushing = false;
    isPaused = false;
    isStopped = false;
    isWaitingForCallback = false;

    if (sayDialog?.writer != null) {
      Locker<Fungus.Writer>.Unlock(sayDialog.writer);
    }

    sayDialog = null;
    textBuffer.Clear();
    nextCharacterTime = 0f;
    visibleCharactersCount = 0;
    textStart = 0;
    textEnd = 0;

    pendingCallbacks.Clear();
    nextCallback = null;

    EventBus.ReleaseCapture("say-dialog-changed");
    EventBus.ReleaseCapture("ready-to-start");
  }
}
