# MDRG AI Dialog

**Download:** [Latest Release](https://github.com/StLyn4/MdrgAiDialog/releases)

MDRG AI Dialog is a MelonLoader mod for **My Dystopian Robot Girlfriend** that lets you chat with Jun using an LLM (local or cloud).

> **Important:** out of the box the mod tries to connect to a local Ollama server at `http://localhost:11434`.
> If you don't have [Ollama](https://ollama.com/download) installed/running, you'll get a connection error with default settings.

## Quick start (local & simplest): Ollama

1. Install **MelonLoader Nightly** (0.7.2+).
2. Install the mod (`MdrgAiDialog.dll`) into your game `Mods` folder.
3. Install **Ollama** and make sure it is running.
4. Launch the game. The mod can automatically download the default model when needed (you will get an in-game prompt).

## Requirements

- The game: **My Dystopian Robot Girlfriend**
- **MelonLoader Nightly 0.7.2+** (latest nightly build at the time of writing)
  - Installer: [LavaGang/MelonLoader.Installer](https://github.com/LavaGang/MelonLoader.Installer)
- This mod: `MdrgAiDialog.dll` (from the Releases page)
- **Ollama** (only required if you use the default/local provider)
  - Download: [ollama.com/download](https://ollama.com/download)

## Installation (Windows)

1. **Install MelonLoader (Nightly)**
   - Download and run the MelonLoader Installer: [LavaGang/MelonLoader.Installer](https://github.com/LavaGang/MelonLoader.Installer)
   - Select your game and install **Nightly** (0.7.2+).
2. **Run the game once** (this lets MelonLoader create its folders).
3. **Install the mod**
   - Download `MdrgAiDialog.dll` from the Releases page.
   - Copy it to:
     - `<GameFolder>\Mods\MdrgAiDialog.dll`
4. **Launch the game**.

> **Where is `<GameFolder>`?** The folder where `My Dystopian Robot Girlfriend.exe` is located.

## Configuration

The mod uses a single config file created on first launch:

- `<GameFolder>\UserData\MdrgAiDialog.cfg`

**How to edit it:**

1. Close the game.
2. Open `<GameFolder>\UserData\MdrgAiDialog.cfg` in Notepad.
3. Change `UsedProvider` and the settings for that provider.
4. Save the file and launch the game again.

### Minimal config example: Ollama

```ini
[General]
UsedProvider = "Ollama"

[Ollama]
ApiUrl = "http://localhost:11434/v1"
Model = "artifish/llama3.2-uncensored"
```

> **Tip:** The mod will check whether the model exists on your Ollama server. If it is missing, the game will ask if you want to download it and show progress.

### Minimal config example: OpenRouter

OpenRouter is a great option if you don't want to run a local server. It offers many models, including a good selection of **free** ones, behind a single OpenAI-compatible API.

```ini
[General]
UsedProvider = "OpenRouter"

[OpenRouter]
ApiUrl = "https://openrouter.ai/api/v1"
ApiKey = "PUT_YOUR_OPENROUTER_KEY_HERE"
Model = "deepseek/deepseek-r1-0528:free"
```

## AI providers

Provider selection is done via `UsedProvider` in `[General]`. Names must match exactly:

- `Ollama` (default)
- `OpenAI`
- `OpenRouter`
- `Mistral`
- `Google`
- `DeepSeek`
- `Claude`
- `Mock` (just for testing)

### Ollama (default, local)

- **Default URL:** `http://localhost:11434/v1`
- **Default model:** `artifish/llama3.2-uncensored`
- **If you don’t install Ollama:** the mod will fail to connect using default settings.
- **Pros:** local, free, no usage limits, ...
- **Cons:** model quality and speed depend on your PC. Don’t expect too much.

### OpenRouter (recommended)

If you want a cloud option and don’t want to pick a single vendor, OpenRouter is a very convenient choice. It provides an OpenAI-compatible API and a large catalog of models (including free ones).

### OpenAI / OpenAI-compatible providers

This mod uses the **OpenAI** compatible endpoints so it will just work with
many services that provide such an API.

To use OpenAI (or a compatible service):

1. Set:
   - `[General] UsedProvider = "OpenAI"`
2. Set these fields in `[OpenAI]`:
   - `ApiUrl` (the base URL, usually ends with `/v1` - do not add `/chat/completions`)
   - `ApiKey` (your key/token)
   - `Model` (provider-specific model name)

### Mistral / Google / DeepSeek / Claude

These providers are pre-configured with default API URLs. In most cases you only need to set:

- `ApiKey`
- `Model`

## Troubleshooting

- **I see a connection error right away**
  - If you are using `Ollama`: install Ollama and make sure it is running.
  - Or switch to a cloud provider by setting `[General] UsedProvider = "OpenAI"` (or something else) and adding an API key.
- **Config file is missing**
  - Run the game once with MelonLoader installed, then check `<GameFolder>\UserData\`.
- **Where are logs?**
  - `<GameFolder>\MelonLoader\Latest.log`
- **How to reset configuration?**
  - Close the game and delete `<GameFolder>\UserData\MdrgAiDialog.cfg` (it will be re-created on next launch).

## For developers

- Install **.NET 6 SDK**.
- Set your game path:
  - `Directory.Build.props` (`<GamePath>...</GamePath>`)
  - (Optional) `scripts\install.bat` (`GAME_DIR_PATH=...`)
- Build:
  - `dotnet build -c Release`
- The project copies the built DLL to the game `Mods` folder automatically (see `MdrgAiDialog.csproj`).
