@echo off
REM PMBus Master CLI Build Script for Windows
REM Requires .NET 6.0 SDK or later

echo ================================================
echo   PMBus Master CLI - Build Script (Windows)
echo ================================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK not found!
    echo.
    echo Please install .NET 6.0 SDK or later:
    echo   https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK found: %DOTNET_VERSION%
echo.

REM Restore dependencies
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo.

REM Build in Release mode
echo Building in Release mode...
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo.

echo ================================================
echo   Build Complete!
echo ================================================
echo.
echo Binary location:
echo   .\bin\Release\net6.0\pmbus-cli.dll
echo.
echo Run with:
echo   dotnet run -- ^<command^>
echo   or
echo   dotnet .\bin\Release\net6.0\pmbus-cli.dll ^<command^>
echo.
echo For standalone executable, run:
echo   publish.bat
echo.

pause
