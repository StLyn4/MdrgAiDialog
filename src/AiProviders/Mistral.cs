namespace MdrgAiDialog.AiProviders;

/// <summary>
/// AI provider for Mistral API
/// </summary>
public class Mistral(AiProviderConfig config) : OpenAi(config) { }
