@setlocal
@for /F "delims=" %%I in ("%~dp0") do @set gitex_folder=%%~fI
@set PATH=%gitex_folder%;%PATH%
@start /B GitExtensions.exe %*