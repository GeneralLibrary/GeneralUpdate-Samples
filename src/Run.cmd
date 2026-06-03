@echo off
REM GeneralUpdate 示例浏览器入口 (Windows)
REM 直接双击或在终端运行即可
powershell -ExecutionPolicy Bypass -File "%~dp0Run.ps1" %*
pause
