# Git Extensions

## Introduction

GitExtensions is a shell extension, a Visual Studio 2008 / 2010 plugin and a standalone Git repository tool.

## Mailing list

The mailing list can be found at [http://groups.google.com/group/gitextensions](http://groups.google.com/group/gitextensions)

## How to debug GitExtensions

The installer is build using WiX. You need to install WiX when you want to build the installer. This can be downloaded here: [http://wix.sourceforge.net/](http://wix.sourceforge.net/). If you do not want to build the installer, just open the solution and ignore the warning.

* Open the solution file (GitCommands.sln or GitCommands.VS2010.sln)
* Hit F5 to compile and run GitExtensions

## How to contribute code

* Login in github (you need an account)
* Fork the main repository from [http://github.com/spdr870/gitextensions](github)
* Push your changes to your fork
* Send me a pull request

If you do not want to use github, I also accept mailed patches. Just make sure the patch is send as an attachement and not in the body of the mail.

## How to create the installer

* Download and install WiX [http://wix.sourceforge.net/](http://wix.sourceforge.net/)
* Open the solution file (GitCommands.sln or GitCommands.VS2010.sln)
* Compile SimpleShlExt in release for 64bit windows
* Compile SimpleShlExt in release for 32bit windows
* Compile entire solution in release, mixed platforms
* Run Setup\MakeInstallers.bat to build the installers

## Links

* Download page: [http://code.google.com/p/gitextensions/downloads/list](http://code.google.com/p/gitextensions/downloads/list)
* Source code: [http://github.com/spdr870/gitextensions](http://github.com/spdr870/gitextensions)
* Issue tracker: [http://github.com/spdr870/gitextensions/issues](http://github.com/spdr870/gitextensions/issues)
* Mailing list: [http://groups.google.com/group/gitextensions](http://groups.google.com/group/gitextensions)
