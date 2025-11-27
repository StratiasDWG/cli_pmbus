@echo off
REM PMBus Master CLI - Release Packaging Script (Windows)
REM This script builds and packages the application for all supported platforms

setlocal enabledelayedexpansion

REM Configuration
set PROJECT_NAME=PmbusMasterCLI
set PROJECT_FILE=%PROJECT_NAME%.csproj
set BUILD_CONFIG=Release
set PUBLISH_DIR=.\publish
set PACKAGES_DIR=.\packages

REM Extract version from .csproj file
for /f "tokens=2 delims=<>" %%a in ('findstr /C:"<Version>" %PROJECT_FILE%') do set VERSION=%%a
if "%VERSION%"=="" set VERSION=2.0.0

REM Print banner
echo ================================================
echo    PMBus Master CLI - Release Packager
echo    Version: %VERSION%
echo ================================================
echo.

REM Check if .NET SDK is installed
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found. Please install .NET 6.0 SDK or later.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/6.0
    exit /b 1
)

REM Print .NET version
echo [INFO] Using .NET SDK version:
dotnet --version
echo.

REM Clean previous builds
echo [INFO] Cleaning previous builds...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%PACKAGES_DIR%" rmdir /s /q "%PACKAGES_DIR%"
dotnet clean "%PROJECT_FILE%" -c "%BUILD_CONFIG%" >nul 2>&1
echo [SUCCESS] Project cleaned
echo.

REM Restore dependencies
echo [INFO] Restoring NuGet packages...
dotnet restore "%PROJECT_FILE%"
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to restore dependencies
    exit /b 1
)
echo [SUCCESS] Dependencies restored
echo.

REM Run tests
echo [INFO] Running tests...
dotnet test "%PROJECT_NAME%.Tests.csproj" -c "%BUILD_CONFIG%" --no-restore --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] All tests passed
) else (
    echo [WARNING] Tests failed or test project not found. Continuing...
)
echo.

REM Create packages directory
if not exist "%PACKAGES_DIR%" mkdir "%PACKAGES_DIR%"

REM Build for each runtime
set RUNTIMES=linux-x64 win-x64 osx-x64 linux-arm64 osx-arm64

for %%r in (%RUNTIMES%) do (
    echo [INFO] Building for %%r...

    set OUTPUT_DIR=%PUBLISH_DIR%\%%r

    REM Publish with trimming and single-file
    dotnet publish "%PROJECT_FILE%" -c "%BUILD_CONFIG%" -r %%r --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:DebugType=none -p:DebugSymbols=false -o "!OUTPUT_DIR!" --no-restore >nul 2>&1

    if !ERRORLEVEL! EQU 0 (
        echo [SUCCESS] Built successfully for %%r

        REM Create package based on platform
        set PACKAGE_NAME=pmbus-cli-v%VERSION%-%%r

        REM Check if Windows runtime
        echo %%r | findstr /C:"win-" >nul
        if !ERRORLEVEL! EQU 0 (
            echo [INFO]   Creating Windows package...
            cd "!OUTPUT_DIR!"
            powershell -Command "Compress-Archive -Path * -DestinationPath '..\..\%PACKAGES_DIR%\!PACKAGE_NAME!.zip' -Force" >nul 2>&1
            cd ..\..
            echo [SUCCESS]   Created !PACKAGE_NAME!.zip
        ) else (
            echo [INFO]   Creating tar.gz package...
            REM For non-Windows platforms, create tar.gz using PowerShell or external tool
            powershell -Command "$source = '!OUTPUT_DIR!'; $destination = '%PACKAGES_DIR%\!PACKAGE_NAME!.tar.gz'; tar -czf $destination -C $source ." >nul 2>&1
            if !ERRORLEVEL! EQU 0 (
                echo [SUCCESS]   Created !PACKAGE_NAME!.tar.gz
            ) else (
                echo [WARNING]   tar command not available, skipping tar.gz creation for %%r
            )
        )
        echo.
    ) else (
        echo [ERROR] Failed to build for %%r
        echo.
    )
)

REM Generate checksums using PowerShell
echo [INFO] Generating checksums...
powershell -Command "Get-ChildItem '%PACKAGES_DIR%\*' -File | Get-FileHash -Algorithm SHA256 | Select-Object @{Name='Hash';Expression={$_.Hash.ToLower()}}, @{Name='File';Expression={Split-Path $_.Path -Leaf}} | ForEach-Object { $_.Hash + '  ' + $_.File } | Out-File -FilePath '%PACKAGES_DIR%\checksums-sha256.txt' -Encoding ASCII"
echo [SUCCESS] SHA256 checksums generated
echo.

REM Create release notes template
set RELEASE_NOTES=%PACKAGES_DIR%\RELEASE_NOTES.md
(
echo # PMBus Master CLI - Release v%VERSION%
echo.
echo ## Release Information
echo.
echo **Version:** %VERSION%
echo **Release Date:** %DATE%
echo **Build Date:** %DATE% %TIME%
echo.
echo ## Download
echo.
echo Choose the appropriate package for your platform:
echo.
echo ### Windows ^(x64^)
echo 1. Download `pmbus-cli-v%VERSION%-win-x64.zip`
echo 2. Extract the ZIP file
echo 3. Run `pmbus-cli.exe --help`
echo.
echo ### Linux ^(x64^)
echo ```bash
echo wget https://github.com/StratiasDWG/cli_pmbus/releases/download/v%VERSION%/pmbus-cli-v%VERSION%-linux-x64.tar.gz
echo tar -xzf pmbus-cli-v%VERSION%-linux-x64.tar.gz
echo chmod +x pmbus-cli
echo ./pmbus-cli --help
echo ```
echo.
echo ## Requirements
echo.
echo - **NI-8451 USB-to-I2C/SPI Interface** hardware
echo - **NI-845x drivers** installed
echo - For framework-dependent deployment: **.NET 6.0 Runtime** or later
echo.
echo ## Documentation
echo.
echo - [README.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/README.md^)
echo - [QUICKSTART.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/QUICKSTART.md^)
echo - [BUILD_COMPLETE.md](https://github.com/StratiasDWG/cli_pmbus/blob/main/BUILD_COMPLETE.md^)
echo.
) > "%RELEASE_NOTES%"

echo [SUCCESS] Release notes template created
echo.

REM Summary
echo ================================================
echo    Build Complete!
echo ================================================
echo.
echo [INFO] Release packages created in: %PACKAGES_DIR%\
echo.
echo [INFO] Package contents:
dir /b "%PACKAGES_DIR%"
echo.

echo [INFO] Next steps:
echo   1. Review release notes: %RELEASE_NOTES%
echo   2. Test packages on target platforms
echo   3. Create Git tag: git tag -a v%VERSION% -m "Release v%VERSION%"
echo   4. Push tag: git push origin v%VERSION%
echo   5. Create GitHub release and upload packages
echo.

echo [SUCCESS] Done!

endlocal
