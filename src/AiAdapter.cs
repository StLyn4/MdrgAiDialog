using System;
using System.Threading.Tasks;
using MdrgAiDialog.AiProviders;

namespace MdrgAiDialog;

public class AiAdapter {
  private readonly IAiProvider provider;

  public AiAdapter(string provider, string apiUrl, string apiKey, string model) {
    this.provider = CreateProvider(provider, apiUrl, apiKey, model);
  }

  public async Task<string> SendMessage(string message) {
    return await provider.SendMessage(message);
  }

  public async Task<string[]> GetChatMessages(string userInput) {
    // TODO: Implement
    var response = await SendMessage(userInput);
    return ["#bot.Expression.VeryHappy", "#bot.ArmBoth.UpHi", response];
  }

  private IAiProvider CreateProvider(string provider, string apiUrl, string apiKey, string model) {
    return provider.ToLower() switch {
      "mistral" => new MistralAiProvider(apiUrl, apiKey, model),
      _ => throw new ArgumentException($"Unknown AI provider: {provider}")
    };
  }
}
