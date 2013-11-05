#!/bin/sh
export config=$2
mkdir -p Plugins/
for i in  $1/Plugins/* $1/Plugins/Statistics/*; do
  cp $i/bin/$config/*.dll Plugins/
  cp $i/bin/$config/*.dll.mdb Plugins/
done
