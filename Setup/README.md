Building installers for Git Extensions
======================================

1. Download the Windows Installer XML (WiX) 3.11.0 toolset from
   [here](https://www.nuget.org/packages/WiX) and install it.
    * Latest stable version at the time of writing was 3.11.0

2. If you don't have Visual Studio installed, 
   download Visual Studio Community from [here](https://www.visualstudio.com/vs/community/)
   and install it.

3. Synchronize submodules:
    ```
    git submodule init
    git submodule sync
    ```

4. Run set_version_to.py (Python 2) to change current version. For example -
    ```
    set_version_to.py -t 2.50 -v 2.50
    ```

5. Run BuildInstallers.VS2015.cmd or BuildInstallers.VS2017.cmd. This will generate two msi installers:
    * GitExtensions250.msi - the standard install with only Git Extensions
    * GitExtensions250Complete.msi - also includes Git for Windows and KDiff3

6. Run BuildInstallers.Mono.cmd. This will generate a ZIP archive
