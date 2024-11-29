@echo off
setlocal

REM Define the base directory
set BASE_DIR=%~dp0

REM Check if run directory exists and delete it if it does
if exist "%BASE_DIR%run" (
    echo Deleting existing run directory...
    rmdir /s /q "%BASE_DIR%run"
)

REM Create run directory and its subdirectories
mkdir "%BASE_DIR%run"
mkdir "%BASE_DIR%run\app"

REM Copy files from Client, Server, Upgrade to app directory
xcopy "%BASE_DIR%Client\bin\Release\net8.0\*" "%BASE_DIR%run\app\" /s /e /y
xcopy "%BASE_DIR%Server\bin\Release\net8.0\*" "%BASE_DIR%run\app\" /s /e /y
xcopy "%BASE_DIR%Upgrade\bin\Release\net8.0\*" "%BASE_DIR%run\app\" /s /e /y
xcopy "%BASE_DIR%StartManager\bin\Release\net8.0\*" "%BASE_DIR%run\app\" /s /e /y

echo Operation completed successfully.

endlocal