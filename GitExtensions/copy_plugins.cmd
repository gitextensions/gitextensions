@echo off
set config=%1
md Plugins\
for /d %%I in (%~p0\..\Plugins\*, %~p0\..\Plugins\Statistics\*, %~p0\..\Plugins\BuildServerIntegration\*) do (
  xcopy /y %%I\bin\%config%\*.dll Plugins\
  xcopy /y %%I\bin\%config%\*.pdb Plugins\
)