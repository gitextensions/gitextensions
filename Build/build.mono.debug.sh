#!/bin/bash
MONOSOLUTION=../GitExtensionsMono.sln

xbuild /t:clean $MONOSOLUTION
xbuild /p:TargetFrameworkProfile="" $MONOSOLUTION
