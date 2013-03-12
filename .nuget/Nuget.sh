#!/bin/sh

cd packages
for x in `find ../ -name packages.config`;do mono --runtime=v4.0.30319 ../.nuget/nuget.exe install $x;done
