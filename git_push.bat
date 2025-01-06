@echo off
echo Pushing to default remote repository...
git push
if %errorlevel% neq 0 (
    echo Failed to push to default remote repository.
    exit /b %errorlevel%
)

echo Pushing to upstream remote on 'main' branch...
git push upstream main
if %errorlevel% neq 0 (
    echo Failed to push to upstream remote on 'main' branch.
    exit /b %errorlevel%
)

echo Pushing to 'upstream_gitcode' remote on 'main' branch...
git push upstream_gitcode main
if %errorlevel% neq 0 (
    echo Failed to push to 'upstream_gitcode' remote on 'main' branch.
    exit /b %errorlevel%
)

echo All pushes completed successfully.
pause