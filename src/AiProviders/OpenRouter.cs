using System.Collections.Generic;

namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for OpenRouter API
/// </summary>
public class OpenRouter(AiProviderConfig config) : OpenAi(config) {
  protected override Dictionary<string, object> CreateRequestPayload(bool stream, List<ChatMessage> overrideMessages) {
    var payload = base.CreateRequestPayload(stream, overrideMessages);

    if (config.ReasoningEnabled.HasValue) {
      payload["reasoning"] = new { exclude = true, enabled = config.ReasoningEnabled.Value };
    }

    return payload;
  }
}
