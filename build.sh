#!/bin/sh

xbuild /verbosity:minimal CKAN-netkan.sln
chmod a+x ../CKAN-core/packages/ILRepack.1.25.0/tools/ILRepack.exe

mono ../CKAN-core/packages/ILRepack.1.25.0/tools/ILRepack.exe /target:exe /out:../netkan.exe bin/Debug/NetKAN.exe bin/Debug/log4net.dll bin/Debug/Newtonsoft.Json.dll bin/Debug/ICSharpCode.SharpZipLib.dll bin/Debug/ChinhDo.Transactions.dll bin/Debug/CKAN.dll bin/Debug/CommandLine.dll bin/Debug/nunit.framework.dll

nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
# prove
