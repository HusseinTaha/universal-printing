@ECHO OFF
REM The following directory is for .NET 2.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
echo Installing WindowsService...
echo ---------------------------------------------------
InstallUtil /u "C:\WorkSpace\PrintFileToPrinterSLN\FilePrintService\bin\Debug\FilePrintService.exe"
echo ---------------------------------------------------
echo Done.
pause