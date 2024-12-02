@echo off
echo Running npm run clear...
npm run clear && (
    echo Running npm run build...
    npm run build && (
        echo Running npm run serve...
        npm run serve
    )
)

echo All commands executed.
pause