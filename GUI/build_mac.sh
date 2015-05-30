#!/bin/bash
set -x

#Run the CMD build.sh
cd ../CKAN-cmdline
./build.sh
cd ../CKAN-GUI

if [ -d "../CKAN.app/" ]; then
  rm -rf "../CKAN.app"
fi
macpack -m:1 -o:../ \
  -i:assets/ckan.icns \
  -n:CKAN -a:../ckan.exe