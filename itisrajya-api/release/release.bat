@echo off
echo CLEAN RELEASE 
for %%F in (*) do (
    if /I not "%%~nxF"=="release.bat" del /F /Q "%%F"
)
cd /d ..
dotnet publish -c Release -o ./release
echo RELEASE SUCCESSFUL
pause