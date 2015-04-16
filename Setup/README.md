Building installers for Git Extensions
======================================

1. Download the Windows Installer XML (WiX) 3.9 toolset from
   [here](http://wix.codeplex.com/releases/) and install it.
    * Latest stable version at the time of writing was 3.9

2. If Visual Studio 2012 is NOT installed, 
   download Team Explorer for Microsoft Visual Studio 2012 
   from [here](http://www.microsoft.com/en-us/download/details.aspx?id=30656)
   and install it.

3. Synchronize submodules:
    ```
    git submodule init
    git submodule sync
    ```

4. Run set_version_to.py (Python 2) to change current version. For example -
    ```
    set_version_to.py --version 2.48
    ```
  
5. Run BuildInstallers.VS2012.cmd or BuildInstallers.VS2013.cmd. This will generate two msi installers:   
    * GitExtensions248.msi - the standard install with only Git Extensions
    * GitExtensions248Complete.msi - also includes msysgit and KDiff3
   (build requires Visual Studio 2012 or Visual Studio 2013. Visual Studio Community
   Edition can be used - https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx).

6. Run BuildInstallers.Mono.cmd. This will generate zip archive
