#!/bin/bash
set -x

xbuild /verbosity:minimal CKAN-GUI.sln

if [ -d "./bin/Debug/CKAN.app/" ]; then
  rm -rf "./bin/Debug/CKAN.app"
fi
macpack -m:1 -o:bin/Debug/ \
  -i:assets/ckan.icns \
  -r:/Library/Frameworks/Mono.framework/Versions/Current/lib/ \
  -r:bin/Debug/CKAN.dll \
  -r:bin/Debug/ChinhDo.Transactions.dll \
  -r:bin/Debug/ICSharpCode.SharpZipLib.dll \
  -r:bin/Debug/INIFileParser.dll \
  -r:bin/Debug/Newtonsoft.Json.dll \
  -r:bin/Debug/log4net.dll \
  -n:CKAN -a:bin/Debug/CKAN-GUI.exe
