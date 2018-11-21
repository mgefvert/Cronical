@echo off

if ""=="%LIBPATH%" (
    echo LIBPATH missing -- this needs to run from Visual Studio Command Prompt
    goto :error
)

setlocal
set bindir=cronical\bin\release

echo.
echo = Cronical
msbuild Cronical.sln /v:minimal /p:Configuration=Release
if errorlevel 1 goto :error

echo.
echo = Copying binaries

rd /s /q Release
md Release

pushd cronical\bin\release
7z a ..\..\..\Release\Cronical.zip cronical.exe cronical.exe.config cronical.dat
popd

echo.
echo = Done
echo.

goto :eof

:error
echo *** Build failed ***
