set targetdir="%userprofile%\Documents\Visual Studio 2010\Addins"
set config=Debug
copy bin\%config%\GitPlugin.AddIn %targetdir%
copy bin\%config%\GitPlugin.dll %targetdir%
mkdir %targetdir%\en-US
copy bin\%config%\en-US\GitPlugin.resources.dll %targetdir%\en-US
