@setlocal
@for /F "delims=" %%I in ("%~dp0") do @set gitex_folder=%%~fI
@set PATH=%gitex_folder%;%PATH%
@REM If no arguments, try open current working directory
@set arg=%*
@if not defined arg SET arg=browse .
@start /B GitExtensions.exe %arg%