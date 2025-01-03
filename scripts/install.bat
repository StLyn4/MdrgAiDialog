@echo off
setlocal enabledelayedexpansion

@REM Path to the game executable
set "GAME_DIR_PATH=F:\Games\Other\PGs\My Dystopian Robot Girlfriend"
set "GAME_EXE_PATH=%GAME_DIR_PATH%\My Dystopian Robot Girlfriend.exe"

@REM Store current directory
pushd "%~dp0"

@REM Change to project root directory using script path
cd "%~dp0.."

@REM Build the project in Release mode
dotnet build -c Release

@REM Check if build failed
if errorlevel 1 (
    echo Build failed
    exit /b 1
)

@REM If run parameter is provided, launch the game
if "%1"=="run" (
    start "" "%GAME_EXE_PATH%"
)

@REM Return to original directory
popd
