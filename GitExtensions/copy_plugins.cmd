@echo off
set config=%1
md Plugins\
echo Microsoft.TeamFoundation.WorkItemTracking.Client.DataStoreLoader.dll > exclude.txt
echo Microsoft.WITDataStore.dll >> exclude.txt
for /d %%I in (%~p0\..\Plugins\*, %~p0\..\Plugins\Statistics\*, %~p0\..\Plugins\BuildServerIntegration\*) do (
  if not "%%I" == "%~p0\..\Plugins\BuildServerIntegration\TfsInterop.Common" (
    xcopy /y /r %%I\bin\%config%\*.dll Plugins\ /EXCLUDE:exclude.txt
    xcopy /y /r %%I\bin\%config%\*.pdb Plugins\
  )
)