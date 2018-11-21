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
if exist Binaries rd /s /q Binaries
md Binaries

for %%f in (cronical.exe cronical.exe.config cronical.dat) do (
    copy cronical\bin\release\%%f binaries
)

echo.
echo = Done
echo.

goto :eof

:error
echo *** Build failed ***
