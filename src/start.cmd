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
echo Running StartManager.bat
call .\StartManager\build.bat
echo .\StartManager\build.bat completed

cd /d %InitialDir%
echo Running Client.bat
call .\Client\build.bat
echo .\Client\build.bat completed

echo All scripts completed.

call "%~dp0resource.bat"

REM 使用 timeout 命令进行3秒倒计时
timeout /t 3 /nobreak >nul

cd /d %InitialDir%
echo Running StartManager
cd /d .\run\app
start "" StartManager.exe

exit