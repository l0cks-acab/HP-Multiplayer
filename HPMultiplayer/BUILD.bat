@echo off
echo Building HP Multiplayer Mod...
echo.

REM Try to find MSBuild in common Visual Studio locations
set "MSBUILD="

if exist "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

if "%MSBUILD%"=="" (
    echo.
    echo ========================================
    echo ERROR: Could not find MSBuild.exe
    echo ========================================
    echo.
    echo You need to install Visual Studio Build Tools to compile the mod.
    echo.
    echo INSTALLATION ^(FREE^):
    echo 1. Download from visualstudio.microsoft.com/downloads
    echo 2. Get "Build Tools for Visual Studio 2022"
    echo 3. Select ".NET desktop build tools"
    echo 4. Click Install ^(~500MB download^)
    echo 5. After installation, run this script again
    echo.
    echo OR install full Visual Studio Community ^(free^)
    echo.
    pause
    exit /b 1
)

echo Found MSBuild at: %MSBUILD%
echo Building project...
echo.

REM Change to the directory where the batch file is located
pushd "%~dp0"

REM Capture build output to log file
set "LOGFILE=%~dp0build_output.log"
set "ERRORFILE=%~dp0build_errors.txt"

echo Build output will be saved to: %LOGFILE%
echo Extracting errors to: %ERRORFILE%
echo.

REM Run MSBuild and capture output to log file
"%MSBUILD%" HPMultiplayer.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Build > "%LOGFILE%" 2>&1
set BUILD_EXIT_CODE=%ERRORLEVEL%

REM Extract errors and warnings from log file (ignore findstr errors)
findstr /C:"error" /C:"warning" /C:"Error" /C:"Warning" /C:"ERROR" /C:"WARNING" /C:"Failed" /C:"FAILED" "%LOGFILE%" > "%ERRORFILE%" 2>nul

REM Display the full output
type "%LOGFILE%"

popd

if %BUILD_EXIT_CODE% EQU 0 (
    echo.
    echo ========================================
    echo BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo The DLL has been copied to the Mods folder.
    echo.
    echo Full build output saved to: %LOGFILE%
    echo.
    echo You can now launch House Party and test the mod!
) else (
    echo.
    echo ========================================
    echo BUILD FAILED
    echo ========================================
    echo.
    echo Full build output saved to: %LOGFILE%
    echo Errors and warnings extracted to: %ERRORFILE%
    echo.
    echo Please check the error messages above or review the log files.
)

echo.
pause
