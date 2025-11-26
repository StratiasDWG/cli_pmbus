@echo off
REM PMBus Master CLI Publish Script for Windows
REM Creates standalone executable for distribution

echo ================================================
echo   PMBus Master CLI - Publish Script (Windows)
echo ================================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 6.0 SDK or later
    pause
    exit /b 1
)

REM Detect architecture
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set RID=win-x64
) else if "%PROCESSOR_ARCHITECTURE%"=="ARM64" (
    set RID=win-arm64
) else (
    set RID=win-x64
)

echo Detected platform: %RID%
echo.

REM Publish self-contained executable
echo Publishing self-contained executable...
dotnet publish -c Release -r %RID% --self-contained true -p:PublishSingleFile=true -o .\publish
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)

echo.
echo ================================================
echo   Publish Complete!
echo ================================================
echo.
echo Standalone executable location:
echo   .\publish\pmbus-cli.exe
echo.
echo Run with:
echo   .\publish\pmbus-cli.exe ^<command^>
echo.
echo To test:
echo   .\publish\pmbus-cli.exe list
echo.

pause
