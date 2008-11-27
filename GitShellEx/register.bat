"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe" GitShellExtension.exe
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\gacutil.exe" /f /i GitShellExtension.exe

"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe" GitCommands.dll
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\gacutil.exe" /f /i GitCommands.dll

"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\regasm.exe" GitUI.dll
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\gacutil.exe" /f /i GitUI.dll
pause