using System.Text;
using System.Threading.Tasks;

namespace MdrgAiDialog.Chat;

/// <summary>
/// Parses AI responses and handles command extraction
/// </summary>
/// <remarks>
/// Processes text stream character by character, detecting and extracting embedded commands.
/// Handles text formatting, whitespace normalization, and command execution timing
/// </remarks>
public class ChatParser(ChatWriter writer, ChatExecutor executor) {
  private readonly ChatWriter writer = writer;
  private readonly ChatExecutor executor = executor;
  private readonly StringBuilder classificationBuffer = new(64);
  private readonly StringBuilder currentCommand = new(128);

  private bool isPotentialCommand = false;
  private bool isParsingCommand = false;

  private bool lastWasWhiteSpace = false;
  private bool lastWasNewLine = false;
  private bool lastWasCarriageReturn = false;

  /// <summary>
  /// Parses a chunk of text from the AI response
  /// </summary>
  /// <param name="chunk">Text chunk to parse</param>
  /// <remarks>
  /// - Normalizes whitespace and newlines
  /// - Detects and extracts commands starting with #!
  /// - Passes clean text to writer and commands to executor
  /// </remarks>
  public async Task Parse(string chunk) {
    for (int i = 0; i < chunk.Length; i++) {
      var currentChar = chunk[i];
      var isWhiteSpace = IsWhiteSpace(currentChar);
      var isNewLine = IsNewLine(currentChar);
      var isLastChar = IsLastChar(chunk, i);

      // Replace broken characters
      currentChar = currentChar switch {
        '\t' => ' ',
        // '\r' => '\n', // Don't do it here, it's handled below
        _ => currentChar,
      };

      if (currentChar == '\r') {
        // Skip carriage return
        lastWasCarriageReturn = true;
        continue;
      }

      if (lastWasCarriageReturn && !isNewLine) {
        // Replace carriage return with newline
        writer.Type("\n");
        lastWasNewLine = true;
      }

      // Reset carriage return flag
      lastWasCarriageReturn = false;

      if (isPotentialCommand) {
        // "#" was found in the previous chunk so we need to check if it's a command
        isPotentialCommand = false;

        if (currentChar == '!') {
          isParsingCommand = true;
          classificationBuffer.Clear();
          continue;
        } else {
          // It's not a command, so we need to type previous "#"
          FlushClassificationBuffer();
        }
      }

      if (currentChar == '#' && !isParsingCommand) {
        if (isLastChar) {
          isPotentialCommand = true;
          classificationBuffer.Append(currentChar);
          continue;
        }

        if (chunk[i + 1] == '!') {
          isParsingCommand = true;
          i++; // Skip the "!"
          continue;
        }
      }

      if (isParsingCommand) {
        if (isWhiteSpace || isNewLine) {
          await EnqueueCurrentCommand();
        } else if (currentChar == '#') {
          await EnqueueCurrentCommand();
          i--; // Re-parse the "#" using code above
          continue;
        } else {
          currentCommand.Append(currentChar);
        }
      } else {
        if (isWhiteSpace && (lastWasWhiteSpace || lastWasNewLine)) {
          // Skip consecutive white spaces
          continue;
        }

        writer.Type(currentChar.ToString());

        lastWasWhiteSpace = isWhiteSpace;
        lastWasNewLine = isNewLine;
      }
    }
  }

  /// <summary>
  /// Prepares the parser for a new message
  /// </summary>
  public async Task Prepare() {
    ResetState();
    ChatExecutor.ResetBotArms();
    await writer.Prepare();
  }

  /// <summary>
  /// Flushes any remaining content and commands
  /// </summary>
  public async Task Flush() {
    if (isParsingCommand) {
      await EnqueueCurrentCommand();
    }

    FlushClassificationBuffer();
    await writer.Flush();

    ResetState();
  }

  private void FlushClassificationBuffer() {
    if (classificationBuffer.Length > 0) {
      writer.Type(classificationBuffer.ToString());
      classificationBuffer.Clear();
    }
  }

  private async Task EnqueueCurrentCommand() {
    if (currentCommand.Length > 0) {
      string command = currentCommand.ToString().Trim();
      currentCommand.Clear();

      var commandWithPrefix = $"#!{command}";
      var isExpression = ChatExecutor.validExpressions.Contains(commandWithPrefix);

      // Clear facial expression before setting a new one
      if (isExpression) {
        await EnqueueCommand("bot.Expression.Clear");
      }

      await EnqueueCommand(command);
    }

    isParsingCommand = false;
  }

  private async Task EnqueueCommand(string command) {
    await writer.AddCallback(() => executor.Run(command));
  }

  private void ResetState() {
    classificationBuffer.Clear();
    currentCommand.Clear();

    isPotentialCommand = false;
    isParsingCommand = false;

    lastWasWhiteSpace = false;
    lastWasNewLine = false;
    lastWasCarriageReturn = false;
  }

  private static bool IsLastChar(string chunk, int i) {
    return i == chunk.Length - 1;
  }

  private static bool IsWhiteSpace(char c, bool includeNewLine = false) {
    if (IsNewLine(c)) {
      return includeNewLine;
    }

    return char.IsWhiteSpace(c);
  }

  private static bool IsNewLine(char c) {
    return c == '\r' || c == '\n';
  }
}
