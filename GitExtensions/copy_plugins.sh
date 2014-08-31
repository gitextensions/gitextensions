#!/bin/sh
export config=$2
mkdir -p Plugins/
for i in  $1/Plugins/* $1/Plugins/Statistics/*; do
  if [ -d $i/bin/$config ]; then
    cp -r $i/bin/$config/*.dll Plugins/
    cp -r $i/bin/$config/*.dll.mdb Plugins/
  fi
done
