Building installers for Git Extensions
======================================

1. Download the Windows Installer XML (WiX) 3.7 toolset from
   [here](http://wix.codeplex.com/releases/) and install it.
    * Latest stable version at the time of writing was 3.7

2. Edit CommonAssemblyInfo.cs and MakeInstallers.bat so that the line which 
   specifies the version is correct for the current release.
   * CommonAssemblyInfo.cs

      [assembly: AssemblyVersion("2.33")]  
      [assembly: AssemblyFileVersion("2.33")]
   * GitExtensionsShellEx\GitExtensionsShellEx.rc

      FILEVERSION 2,33  
      PRODUCTVERSION 2,33  
      VALUE "FileVersion", "2, 33\0"  
      VALUE "ProductVersion", "2, 33\0"
   * MakeInstallers.bat

      set version=2.33

3. Run BuildInstallers.bat. This will generate two msi installers:   
    * GitExtensions233.msi - the standard install with only Git Extensions
    * GitExtensions233Complete.msi - also includes msysgit and KDiff3

4. Build the zip file by hand :(
