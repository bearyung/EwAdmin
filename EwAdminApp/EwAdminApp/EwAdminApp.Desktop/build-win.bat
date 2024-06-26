﻿@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo Version number is required.
    echo Usage: build.bat [version] [extra_args...]
    exit /b 1
)

set "version=%~1"

echo.
echo Compiling EwAdminApp.Desktop with dotnet...
dotnet publish -c Release --no-self-contained -r win-x64 -o %~dp0publish

echo.
echo Building EwAdminApp.Desktop Release v%version%
vpk pack -u EwAdminApp.Desktop -o %~dp0releases -p %~dp0publish -f net8-x64-desktop -v %*