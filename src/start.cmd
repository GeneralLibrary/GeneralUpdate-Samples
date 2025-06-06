@echo off

call "%~dp0process.bat"

REM 保存初始目录
set InitialDir=%CD%

cd /d %InitialDir%
echo Running Server.bat
call .\Server\build.bat
echo .\Server\build.bat completed

cd /d %InitialDir%
echo Running Upgrade.bat
call .\Upgrade\build.bat
echo .\Upgrade\build.bat completed

cd /d %InitialDir%
echo Running Client.bat
call .\Client\build.bat
echo .\Client\build.bat completed

echo All scripts completed.

REM Define the base directory
set BASE_DIR=%~dp0

REM Check if run directory exists and delete it if it does
if exist "%BASE_DIR%run" (
    echo Deleting existing run directory...
    rmdir /s /q "%BASE_DIR%run"
)

REM Create run directory and its subdirectories
mkdir "%BASE_DIR%run"
mkdir "%BASE_DIR%run\"

REM Copy files from Client, Server, Upgrade to app directory
xcopy "%BASE_DIR%Client\bin\Release\net8.0\*" "%BASE_DIR%run\" /s /e /y
xcopy "%BASE_DIR%Server\bin\Release\net8.0\*" "%BASE_DIR%run\" /s /e /y
xcopy "%BASE_DIR%Upgrade\bin\Release\net8.0\*" "%BASE_DIR%run\" /s /e /y

echo Operation completed successfully.

cd /d "%BASE_DIR%run\"
start "" .\ServerSample.exe
start "" .\ClientSample.exe

exit