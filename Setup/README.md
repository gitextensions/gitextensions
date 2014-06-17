Building installers for Git Extensions
======================================

1. Download the Windows Installer XML (WiX) 3.8 toolset from
   [here](http://wix.codeplex.com/releases/) and install it.
    * Latest stable version at the time of writing was 3.8

2. Run set_version_to.py (Python 2) to change current version.

    set_version_to.py --version 2.48
 

3. Run BuildInstallers.VS2012.cmd or BuildInstallers.VS2013.cmd. This will generate two msi installers:   
    * GitExtensions248.msi - the standard install with only Git Extensions
    * GitExtensions248Complete.msi - also includes msysgit and KDiff3

4. Run BuildInstallers.Mono.cmd. This will generate zip archive
