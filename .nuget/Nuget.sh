#!/bin/sh

if [ ! -d 'packages' ]; then
	mkdir packages
fi

for x in `find ./ -name packages.config`;do mono --runtime=v4.0.30319 .nuget/NuGet.exe install $x -o packages;done
