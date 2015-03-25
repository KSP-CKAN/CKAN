#!/bin/bash
set -x

xbuild /verbosity:minimal /property:win32icon=assets/ckan.ico CKAN-cmdline.sln

#Run the GUI build.sh
cd ../CKAN-GUI
sh build.sh
cd ../CKAN-cmdline
#prove

chmod a+x ../CKAN-core/packages/ILRepack.1.25.0/tools/ILRepack.exe

mono ../CKAN-core/packages/ILRepack.1.25.0/tools/ILRepack.exe \
	/target:exe \
	/out:../ckan.exe \
	bin/Debug/CmdLine.exe \
	bin/Debug/CKAN-GUI.exe \
	bin/Debug/ChinhDo.Transactions.dll \
	bin/Debug/CKAN.dll \
	bin/Debug/CommandLine.dll \
	bin/Debug/ICSharpCode.SharpZipLib.dll \
	bin/Debug/log4net.dll \
	bin/Debug/Newtonsoft.Json.dll \
	bin/Debug/INIFileParser.dll \
        ../CKAN-core/packages/curlsharp-v0.5.1-2-gd2d5699/CurlSharp.dll

