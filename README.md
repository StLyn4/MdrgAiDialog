# MDRG AI Dialog

A project for working with conversational AI.

## Installation

// TODO: Add instructions for installing BepInEx
1. Перейдите в раздел [Releases](https://github.com/StLyn4/MdrgAiDialog/releases) и скачайте последнюю версию.
2. Скопируйте файл `MdrgAiDialog.dll` в папку `BepInEx\plugins` вашей игры.
3. Запустите игру.

## Installation (for development)

1. Make sure you have .NET 6.0 and BepInEx 6.0 (Bleeding Edge!) installed
2. Clone the repository:
```
git clone https://github.com/StLyn4/MdrgAiDialog.git
```
3. Copy `Directory.Build.user.props.template` to `Directory.Build.user.props` and set your game installation path
4. Open the solution in Visual Studio - now you can edit the game path in the project properties
5. You can also immediately build the project, install the DLL, and launch the game using the script if you have [.NET CLI](https://dotnet.microsoft.com/download/dotnet) installed:
```
scripts\install-and-run.bat
```
