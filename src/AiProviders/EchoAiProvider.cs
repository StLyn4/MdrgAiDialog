using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// Simple AI provider that echoes messages back with configurable delays
/// </summary>
public class EchoAiProvider : AiProvider {
  private readonly int initialDelay;
  private readonly int interval;
  private readonly int symbolsPerInterval;

  /// <summary>
  /// Creates a new instance of EchoAiProvider
  /// </summary>
  /// <param name="initialDelay">Initial delay before starting response in milliseconds</param>
  /// <param name="interval">Delay between chunks in milliseconds</param>
  /// <param name="symbolsPerInterval">Number of symbols in each chunk</param>
  public EchoAiProvider(int initialDelay = 250, int interval = 10, int symbolsPerInterval = 4) {
    this.initialDelay = initialDelay;
    this.interval = interval;
    this.symbolsPerInterval = symbolsPerInterval;
  }

  /// <summary>
  /// Stub. No warmup needed for echo provider
  /// </summary>
  public override Task WarmUp() {
    return Task.CompletedTask;
  }

  /// <summary>
  /// Returns the input message unchanged with a delay
  /// </summary>
  public override async Task<string> SendMessage(string message) {
    await Task.Delay(initialDelay);
    return message;
  }

  /// <summary>
  /// Returns the input message in chunks with configured delays
  /// </summary>
  public override async IAsyncEnumerable<string> GetChatStream(string message) {
    await Task.Delay(initialDelay);
    var response = await SendMessage(message);

    for (int i = 0; i < response.Length; i += symbolsPerInterval) {
      int charsLeft = response.Length - i;
      int charsToTake = Math.Min(charsLeft, symbolsPerInterval);
      yield return response.Substring(i, charsToTake);
      await Task.Delay(interval);
    }
  }
}
