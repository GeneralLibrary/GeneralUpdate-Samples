@echo off
setlocal

set "processes=ClientSample.exe ServerSample.exe UpgradeSample.exe hfs.exe StartManager.exe"

for %%p in (%processes%) do (
    echo Checking for process %%p ...
    tasklist /FI "IMAGENAME eq %%p" 2>NUL | find /I "%%p" >NUL
    if not errorlevel 1 (
        echo Terminating process %%p ...
        taskkill /F /IM %%p
    ) else (
        echo Process %%p not found.
    )
)

echo Done.
endlocal