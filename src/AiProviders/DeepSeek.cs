using System.Collections.Generic;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for DeepSeek API
/// </summary>
public class DeepSeek(AiProviderConfig config) : OpenAi(config) {
  protected override Dictionary<string, object> CreateRequestPayload(bool stream, List<ChatMessage> overrideMessages) {
    var payload = base.CreateRequestPayload(stream, overrideMessages);

    if (config.ReasoningEnabled.HasValue) {
      payload["thinking"] = new { type = config.ReasoningEnabled.Value ? "enabled" : "disabled" };
    }

    return payload;
  }
}
