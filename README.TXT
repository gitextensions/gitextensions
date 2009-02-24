How to compile GitExtensions
============================

-Open the solution file (\GitCommands\GitCommands.sln)
-First remove signing from the projects
-Compile SimpleShlExt in release for 64bit windows
-Compile SimpleShlExt in release for 32bit windows
-Compile entire solution in release, mixed platforms
-After everything is compiled in release, you can compile in debug

You could also directly compile in debug, but then you need to exclude the setup projects.