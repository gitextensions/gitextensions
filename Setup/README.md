Building installers for Git Extensions
======================================

1. Download the Windows Installer XML (WiX) 3.0 toolset from
   [here](http://wix.sourceforge.net/releases/) and install it.

    * Latest stable version at the time of writing was 3.0.5419
    * Version 3.5.x is not stable yet

2. Edit MakeInstallers.bat so that the line which specifies the version
   is correct for the current release.

        set version=1.92

3. Build all of the C# projects and build the shell extensions in 32-bit
   and 64-bit mode.

4. Run MakeInstallers.bat. This will generate two msi installers:
   
    * GitExtensions192.msi - the standard install with only Git Extensions
    * GitExtensions192Complete.msi - also includes msysgit and KDiff3

5. Build the zip file by hand :(
