#!/bin/sh

if [ ! -d 'packages' ]; then
	mkdir packages
fi
if [ $1 = NoMono ] ; then 

	find ./ -name packages.config -exec .nuget/NuGet.exe install {} -o packages \;
	
else 
	find ./ -name packages.config -exec mono --runtime=v4.0.30319 .nuget/NuGet.exe install {} -o packages \;
fi

