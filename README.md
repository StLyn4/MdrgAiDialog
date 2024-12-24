# MDRG AI Dialog

A project for working with conversational AI.

## Installation

// TODO: Add instructions for installing BepInEx
1. Перейдите в раздел [Releases](https://github.com/StLyn4/MdrgAiDialog/releases) и скачайте последнюю версию.
2. Скопируйте файл `MdrgAiDialog.dll` в папку `BepInEx\plugins` вашей игры.
3. Запустите игру.

## Installation (for development)

1. Make sure you have .NET 6.0 and BepInEx 6.0 (Bleeding Edge!) installed
2. Get it from: [https://builds.bepinex.dev/projects/bepinex_be] IL2Cpp x64 (64 bit)
3. Extract BepInEx into the root directory of the MDRG game folder, and run once, the mod loader should install dependencies
4. Clone the repository:
```
git clone https://github.com/StLyn4/MdrgAiDialog.git
```
3. Copy `Directory.Build.user.props.template` to `Directory.Build.user.props` and set your game installation path
4. Open the solution in Visual Studio - now you can edit the game path in the project properties
5. You can also immediately build the project, install the DLL, and launch the game using the script if you have [.NET CLI](https://dotnet.microsoft.com/download/dotnet) installed:
```
scripts\install-and-run.bat
```

## Update Path in installer.bar

1. Go into the scripts folder, and right click on "install.bat"
2. On line 5 go to set "GAME_DIR_PATH="
 and set the full URL to the game directory.
3. Save and exit, this should allow the files to compile and copy to the proper directory.


## Fixing timeout

Inside of the Bepinex Config folder inside the Game folder you'll find "com.delta.mdrg.aidialog.cfg"

Set "TimeoutSeconds = 30" to "TimeoutSeconds = 300" and change "AiProviderModel" to "artifish/llama3.2-uncensored"


Next go download Ollama from [https://ollama.com/] 

Run the command  ```ollama run artifish/llama3.2-uncensored``` and wait for it to launch, then type in  ```/bye``` to end the session

The model will have a System Prompt in one of the files NOTE: Do not open the language model it may lag/freeze your PC. 

[No Input Prompt Available]
It will be one of the blob files, to set the system Prompt this will dictate how the AI Model behaves. 


