Building installers for Git Extensions
=======================================

1) Download and install the Windows Installer XML (WiX) Toolset 3.0
   from: http://wix.sourceforge.net/releases/

   - Latest stable version at the time of writing was 3.0.5419
   - Version 3.5.x is not stable yet

2) Edit MakeInstallers.bat so that the line which specifies the version
   is correct for this release.

   For Example:
      ..snip..
      set version=1.92
      ..snip..

3) Run MakeInstallers.bat. This will generate two msi installers:
   
   - GitExtensions192.msi - the standard install with only Git Extensions
   - GitExtensions192Complete.msi - also includes msysgit and KDiff3

4) Build the zip file by hand :(
