#!/bin/sh

if [ ! -d 'packages' ]; then
	mkdir packages
fi

find ./ -name packages.config -exec mono --runtime=v4.0.30319 .nuget/NuGet.exe install {} -o packages \;
