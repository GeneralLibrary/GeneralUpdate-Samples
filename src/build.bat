@echo off

REM 定义要启动的可执行文件路径
SET EXE_PATH=".\FileService\hfs.exe"

REM 启动指定的可执行文件
ECHO Starting executable: %EXE_PATH%
START "" %EXE_PATH%

REM 定义解决方案文件路径
SET SOLUTION_PATHS=(
    ".\Server\ServerSample.sln"
    ".\StartManager\StartManager.sln"
    ".\Upgrade\UpgradeSample.sln"
    ".\Client\ClientSample.sln"
)

REM 遍历解决方案文件并编译
FOR %%S IN %SOLUTION_PATHS% DO (
    ECHO Compiling %%S...

    REM 运行dotnet build命令
    dotnet build %%S -c Release

    IF !ERRORLEVEL! NEQ 0 (
        ECHO Failed to build %%S
        EXIT /B !ERRORLEVEL!
    )
)

ECHO All solutions compiled successfully.

EXIT /B 0