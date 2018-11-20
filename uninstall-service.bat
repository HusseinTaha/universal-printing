cd %~dp0
@ECHO OFF
REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
set mypath=%cd%
echo Installing WindowsService...
echo ---------------------------------------------------
InstallUtil /u "%mypath%\FilePrintService.exe"
echo ---------------------------------------------------
echo Done. 