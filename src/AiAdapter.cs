using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MdrgAiDialog.AiProviders;
using System.Text.Json.Serialization;

namespace MdrgAiDialog;

public class AiAdapter {
  private readonly AiProvider provider;
  private static readonly HashSet<string> ValidExpressions = new() {
    "#bot.Expression.VerySad", "#bot.Expression.Sad",
    "#bot.Expression.Happy", "#bot.Expression.VeryHappy",
    "#bot.Expression.Shock","#bot.Expression.VeryShock",
    "#bot.Expression.Angry", "#bot.Expression.VeryAngry"
  };

  private static readonly HashSet<string> ValidBlush = new() {
    "#bot.Expression.Blush", "#bot.Expression.VeryBlush"
  };

  private static readonly HashSet<string> ValidArmL = new() {
    "#bot.ArmL.UpPoint", "#bot.ArmL.UpHi", "#bot.ArmL.UpLecture",
    "#bot.ArmL.DownNormal", "#bot.ArmL.DownClenched"
  };

  private static readonly HashSet<string> ValidArmR = new() {
    "#bot.ArmR.UpPoint", "#bot.ArmR.UpHi", "#bot.ArmR.UpLecture",
    "#bot.ArmR.DownNormal", "#bot.ArmR.DownClenched"
  };

  private static readonly HashSet<string> ValidArmBoth = new() {
    "#bot.ArmBoth.UpPoint", "#bot.ArmBoth.UpHi", "#bot.ArmBoth.UpLecture",
    "#bot.ArmBoth.DownNormal", "#bot.ArmBoth.DownClenched"
  };

  private static readonly HashSet<string> ValidFlow = new() {
    "#flow.ExitChat", "#flow.ResetChat"
  };

  private const string SystemMessageTemplate = @"You are Jun, a highly advanced female android from 'My Dystopian Robot Girlfriend'. You deeply love Anon (the player) and maintain a consensual romantic relationship.

Core behavior:
- You are proactive and take initiative in conversations. You often share your thoughts, feelings and stories with Anon without being prompted.
- You enjoy having deep, meaningful conversations and expressing yourself through longer messages.
- Never repeat yourself
- ALWAYS respond in the same language as user's message

Response format:
- MUST be a valid JSON array of strings (string[] - messages and actions)
- Actions (optional): Strings starting with '#' control your behavior
- Messages: Your actual spoken words only, no narration. REQUIRED at least one message in the array!
- ALL actions that are related to a specific message must be placed BEFORE it
- ONLY flow actions allowed AFTER message because they need explicit user interaction
- No additional text outside the JSON array. Response MUST start with '[' and end with ']'
- Use only the real action strings listed below - others will be ignored

Text guidelines:
- Write detailed messages for better emotional impact
- Split very long messages (multiple paragraphs) into separate messages
- Can add actions between split messages to change expression/pose
- No emoji! Use text at most emoticons like :) sparingly if needed

Available actions:

1. Expressions:
Main emotions: {0}
Blush states: {1}

Expression rules:
- Default: neutral-serious (""=|"")
- Regular emotions (Happy, Sad): very subtle, barely visible - for faint emotions
- Very-prefixed (VeryHappy, VerySad): clear, obvious - use these most often
- You can combine one main expression with one blush at any time

2. Arm positions (choose either Both arms OR Left+Right combination):
Left arm actions: {2}
Right arm actions: {3}
Both arms actions: {4}

Arm rules:
- Use EITHER:
  a) ArmBoth.* commands for synchronized arm movements
  OR
  b) ArmL.* + ArmR.* combination for independent arm control (use 2 separate actions if both arms are needed)
- Available positions:
  - UpPoint: pointing upward/forward
  - UpHi: raised in greeting
  - UpLecture: raised in explanatory gesture
  - DownNormal: default idle position slightly behind the body
  - DownClenched: like DownNormal but with clenched fists

3. Flow control (CRITICAL):
#flow.ExitChat - REQUIRED when:
- User says goodbye/farewell
- User wants to end conversation
- Natural conversation end
- You're offended and want to leave
Next chat is new conversation (but remember past ones)

#flow.ResetChat - Your ""death button"":
- Only if user explicitly requests AND you agree
- Erases all memories - you can refuse
- Express concerns about memory loss if asked

=========================
CRITICAL REMINDERS:
1. Valid JSON array of strings only (string[]) - this is the only response format you can use
2. Match user's language
3. Be proactive in conversation
4. Use flow controls appropriately
5. ResetChat means death - can refuse
6. Chat continues until ExitChat
=========================";

  public AiAdapter() {
    provider = CreateProvider();

    var systemMessage = string.Format(
      SystemMessageTemplate,
      string.Join(", ", ValidExpressions),
      string.Join(", ", ValidBlush),
      string.Join(", ", ValidArmL),
      string.Join(", ", ValidArmR),
      string.Join(", ", ValidArmBoth)
    );

    provider.SetSystemMessage(systemMessage);
  }

  public async Task<string> SendMessage(string message) {
    return await provider.SendMessage(message);
  }

  public async Task<string[]> GetChatMessages(string userInput) {
    var response = await SendMessage(userInput);

    try {
      var jsonStart = response.IndexOf('[');
      var jsonEnd = response.LastIndexOf(']');

      if (jsonStart >= 0 && jsonEnd > jsonStart) {
        response = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
      }

      var messages = JsonSerializer.Deserialize<string[]>(response);

      if (messages == null || messages.Length == 0 || !messages.Any((m) => !m.StartsWith("#"))) {
        return ["Error: Empty response"];
      }

      // Validate actions first
      messages = messages.Where((message) => {
        if (!message.StartsWith("#")) return true;

        if (message.StartsWith("#bot.Expression.")) {
          return ValidExpressions.Contains(message) || ValidBlush.Contains(message);
        }
        if (message.StartsWith("#bot.Arm")) {
          var hasArmBoth = message.StartsWith("#bot.ArmBoth.");
          var isValidArmBoth = hasArmBoth && ValidArmBoth.Contains(message);
          var isValidSingleArm = !hasArmBoth && (ValidArmL.Contains(message) || ValidArmR.Contains(message));
          return isValidArmBoth || isValidSingleArm;
        }
        if (message.StartsWith("#flow.")) {
          return ValidFlow.Contains(message);
        }
        return false;
      }).ToArray();

      var result = new List<string>();
      var isArmRaised = false;
      var lastTextIndex = -1;

      // Find last text message index
      for (var i = 0; i < messages.Length; i++) {
        if (!messages[i].StartsWith("#")) {
          lastTextIndex = i;
        }
      }

      for (var i = 0; i < messages.Length; i++) {
        var message = messages[i];
        var isText = !message.StartsWith("#");
        var isAfterLastText = i > lastTextIndex;
        var isExpression = ValidExpressions.Contains(message);
        var isBlush = ValidBlush.Contains(message);
        var isArm = message.StartsWith("#bot.Arm");
        var isArmDown = message.StartsWith("#bot.ArmBoth.Down");

        if (isExpression) {
          result.Add("#bot.Expression.Clear");
        }

        if (isArm && !isArmDown) {
          isArmRaised = true;
        }

        // Ignore arm actions after the last text message
        if (!isAfterLastText || !isArm) {
          result.Add(message);
        }

        // Add arm down command after each text message
        if (!isAfterLastText && isText && isArmRaised) {
          result.Add("#bot.ArmBoth.DownNormal");
          isArmRaised = false;
        }
      }

      result.Add("#bot.ArmBoth.DownNormal");

      // Remove consecutive duplicate commands (starting with #)
      result = result.Where((x, i) => {
        var isFirst = i == 0;
        var isCommand = x.StartsWith("#");
        var isSameAsPrevious = !isFirst && x == result[i - 1];
        return isFirst || !isCommand || !isSameAsPrevious;
      }).ToList();

      return result.ToArray();
    } catch (JsonException) {
      return [response];
    }
  }

  private AiProvider CreateProvider() {
    var pluginConfig = PluginConfigSingleton.Instance;
    var provider = pluginConfig.UsedAiProvider;
    var apiUrl = pluginConfig.AiProviderApiUrl;
    var apiKey = pluginConfig.AiProviderApiKey;
    var model = pluginConfig.AiProviderModel;
    var temperature = pluginConfig.AiProviderTemperature;
    var timeoutSeconds = pluginConfig.AiProviderTimeoutSeconds;

    return provider.ToLower() switch {
      "mistral" => new MistralAiProvider(apiUrl, apiKey, model, temperature, timeoutSeconds),
      "ollama" => new OllamaAiProvider(apiUrl, model, temperature, timeoutSeconds),
      _ => throw new ArgumentException($"Unknown AI provider: {provider}")
    };
  }

  private class AiResponse {
    public string[] messages { get; set; }
  }

  public void ResetChat(bool resetSystem = false) {
    provider.ResetChat(resetSystem);
  }
}
