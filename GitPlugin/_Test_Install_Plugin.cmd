set targetdir="%userprofile%\Documents\Visual Studio 2010\Addins"
copy bin\GitPlugin.AddIn %targetdir%
copy bin\GitPlugin.dll %targetdir%
mkdir %targetdir%\en-US
copy bin\en-US\GitPlugin.resources.dll %targetdir%\en-US
