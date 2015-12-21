# Contributing to Git Extensions

## How to contribute code

* Login in github (you need an account)
* Fork the main repository from [github](http://github.com/gitextensions/gitextensions)
* Read [Project workflow](https://github.com/gitextensions/gitextensions/wiki/Project-Workflow) and [Coding guide](https://github.com/gitextensions/gitextensions/wiki#coding-guide)
* Push your changes to your fork
* Send me a pull request

If you do not want to use github, I also accept mailed patches. Just make sure the patch is send as an attachement and not in the body of the mail.

## How to debug GitExtensions

The installer is build using WiX. You need to install WiX when you want to build the installer. This can be downloaded here: [http://wixtoolset.org/](http://wixtoolset.org/). If you do not want to build the installer, just open the solution and ignore the warning.

* Open the solution file (GitExtensions.VS2012.sln, or GitExtensions.VS2013.sln, or GitExtensions.VS2015.sln)
* Hit F5 to compile and run GitExtensions

## How to create the installer

* Download and install WiX [http://wixtoolset.org/](http://wixtoolset.org/)
* Run Setup\\BuildInstallers.VS2013.cmd, or Setup\\BuildInstallers.VS2013.cmd, or Setup\\BuildInstallers.VS2015.cmd to build the installers

## Installing Nuget in Monodevelop/Xamarin Studio

Following steps will add nuget package management capabilities to your monoDevelop/Xamarin Studio. Installing nuget requires at least Monodevelop version 3.0.5.

OBS: Monodevelop 4.0 is Xamarin Studio

* On monoDevelop find Add-In manager (macOS is on program main menu, other OS's under tools)
* over the gallery tab, select "Manage repositories..." from the "Repositories" drop-down menu
* For monodevelop 3.0.5, add a new URL http://mrward.github.com/monodevelop-nuget-addin-repository/3.0.5/main.mrep otherwise, if using Xamarin Studio use http://mrward.github.com/monodevelop-nuget-addin-repository/4.0/main.mrep
* A new IDE extension option will appear, select the nuget package management
* To add nuget packages or maintain it, you right click either on the solution or in the references

On monoDevelop preferences, you will find a new "Nuget" option. Check the box that says "Enable package restore".

If by any reason your xbuild is not downloading and installing the packages, manually run the following command to get all the missing packages. Make sure you run it at the top level directory of your solution, like the example below:

```
gitextensions  (master)$ .nuget/Nuget.sh 
Successfully installed 'NBug 1.1.1'.
Successfully installed 'NUnit.Mocks 2.6.2'.
Successfully installed 'NUnit 2.6.2'.
Successfully installed 'Rx-Main 2.1.30214.0'.
Successfully installed 'Rx-Core 2.1.30214.0'.
Successfully installed 'Rx-Interfaces 2.1.30214.0'.
Successfully installed 'Rx-PlatformServices 2.1.30214.0'.
Successfully installed 'Rx-Linq 2.1.30214.0'.
Successfully installed 'Newtonsoft.Json 4.5.11'.
Successfully installed 'RestSharp 104.1'.
Successfully installed 'NBug 1.1.1'.
```
