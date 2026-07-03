@echo off

for %%F in (*) do (
    if /I not "%%~nxF"=="clear.bat" del /F /Q "%%F"
)

echo All files have been deleted.
pause