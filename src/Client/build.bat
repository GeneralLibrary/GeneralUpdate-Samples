@echo off
REM 设置解决方案文件路径
set SOLUTION_PATH=ClientSample.sln

REM 导航到解决方案目录
cd /d %~dp0

REM 运行dotnet build命令
dotnet build %SOLUTION_PATH% -c Release

REM 检查构建是否成功
if %errorlevel% neq 0 (
    echo Build failed
    exit /b %errorlevel%
) else (
    echo Build succeeded
)