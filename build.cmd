@echo off

if ""=="%LIBPATH%" (
    echo LIBPATH missing -- this needs to run from Visual Studio Command Prompt
    goto :error
)

setlocal
set BINDIR=cronical\bin\release
set LINK=%HOMEDRIVE%%HOMEPATH%\.nuget\packages\ilrepack\2.0.16\tools\ilrepack


echo.
echo === Building

msbuild Cronical.sln /v:minimal /p:Configuration=Release
if errorlevel 1 goto :error


echo.
echo === Copying and linking to binaries

if exist Binaries rd /s /q Binaries
md Binaries
%LINK% /out:Binaries\Cronical.exe /v4 %BINDIR%\Cronical.exe %BINDIR%\DotNetCommons.dll
copy %BINDIR%\cronical.dat binaries
del binaries\*.pdb

echo.
echo = Done
echo.

goto :eof

:error
echo *** Build failed ***
