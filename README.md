# MDRG AI Dialog

A project for working with conversational AI.

## Version 0.2 Release
# [New Release is now out! Click here to Download](https://github.com/StLyn4/MdrgAiDialog/releases/tag/Stable-Release-Ver-2)

## Installation

* Download BepInEx 6.0 IL2CPP x64 from [BepInEx Bleeding Edge builds](https://builds.bepinex.dev/projects/bepinex_be).
* Extract the downloaded BepInEx archive into your game's root directory.
* Run the game once to let BepInEx install and configure itself.
* Go to [Releases](https://github.com/StLyn4/MdrgAiDialog/releases) and download the latest version.
* Copy `MdrgAiDialog.dll` file to the `BepInEx\plugins` folder in your game directory.
* Launch the game.

## Installation (for development)

* Make sure you have .NET 6.0 and BepInEx 6.0 (Bleeding Edge!) installed.
* Get it from: [BepInEx Bleeding Edge builds](https://builds.bepinex.dev/projects/bepinex_be).
* Extract BepInEx into the root directory of the MDRG game folder, and run once, the mod loader should install dependencies.
* Clone the repository:

``` bash
git clone https://github.com/StLyn4/MdrgAiDialog.git
```

* Open `Directory.Build.props` and set your game installation path.
* You can also immediately build the project, install the DLL, and launch the game in one go using the script if you have [.NET CLI](https://dotnet.microsoft.com/download/dotnet) installed:

``` bash
scripts\install-and-run.bat
```

## Update Path in install.bat

* Go into the scripts folder, and right click on "install.bat".
* On line 5 go to set "GAME_DIR_PATH=".
* and set the full URL to the game directory.
* Save and exit, this should allow the files to compile and copy to the proper directory.

## Fixing timeout

Inside of the Bepinex Config folder inside the Game folder you'll find "com.delta.mdrg.aidialog.cfg".

Set "TimeoutSeconds = 300" to "TimeoutSeconds = 600" (or higher) and change "AiProviderModel" to "artifish/llama3.2-uncensored".

Next go download Ollama from the [Ollama website](https://ollama.com/).

Run the command  ```ollama run artifish/llama3.2-uncensored``` and wait for it to launch, then type in  ```/bye``` to end the session.

The model will have a System Prompt in one of the files NOTE: Do not open the language model it may lag/freeze your PC.

[No Input Prompt Available]
It will be one of the blob files, to set the system Prompt this will dictate how the AI Model behaves.
