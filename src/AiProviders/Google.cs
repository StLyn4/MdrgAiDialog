using System.Collections.Generic;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for Google AI Studio
/// </summary>
public class Google(AiProviderConfig config) : OpenAi(config) {
  protected override Dictionary<string, object> CreateRequestPayload(bool stream, List<ChatMessage> overrideMessages) {
    var payload = base.CreateRequestPayload(stream, overrideMessages);

    if (config.ReasoningEnabled.HasValue && !config.ReasoningEnabled.Value) {
      payload["reasoning_effort"] = "none";
    }

    return payload;
  }
}
