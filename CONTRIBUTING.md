# Contributing to Git Extensions

## How to contribute code

* Login in github (you need an account)
* Fork the main repository from [github](http://github.com/gitextensions/gitextensions)
* Read [Project workflow](https://github.com/gitextensions/gitextensions/wiki/Project-Workflow) and [Coding guide](https://github.com/gitextensions/gitextensions/wiki#coding-guide)
* Push your changes to your fork
* Create a pull request

## How to debug GitExtensions

The installer is build using WiX. You need to install WiX when you want to build the installer. This can be downloaded here: [http://wixtoolset.org/](http://wixtoolset.org/). If you do not want to build the installer, just open the solution and ignore the warning.

* Open the solution file (GitExtensions.sln)
* Hit F5 to compile and run GitExtensions

## How to create the installer

* Download and install WiX [http://wixtoolset.org/](http://wixtoolset.org/)
* Run Setup\\BuildInstallers.cmd to build the installers

