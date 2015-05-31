#!/bin/bash
set -x

xbuild /verbosity:minimal CKAN.sln

#prove
NUNIT_BINARY=""

check_nunit () {
    # Extract the CLR version of nunit-console.
    NUNIT_TEXT=$($1 -help)
    NUNIT_VERSION=$(echo "$NUNIT_TEXT" | awk '$1 ~ /CLR/ {print substr($3,1,1)}')
    
    if [ $NUNIT_VERSION -eq 4 ]
    then
        NUNIT_BINARY=$1
    fi
    
    echo "Found $1 with CLR version $NUNIT_VERSION."
}


# Find a suitable version of nunit.
echo "Checking if nunit-console is available..."
command -v "nunit-console" >/dev/null 2>&1 && check_nunit "nunit-console"

echo "Checking if nunit-console4 is available..."
command -v "nunit-console4" >/dev/null 2>&1 && check_nunit "nunit-console4"

# If we found a suitable nunit binary, continue with the testing.
if [ "$NUNIT_BINARY" != "" ]
then
    command $NUNIT_BINARY --exclude=FlakyNetwork Tests/bin/Debug/CKAN.Tests.dll
else
    echo "Could not find a suitable version of nunit-console to run the tests. Skipping test execution."
fi


chmod a+x Core/packages/ILRepack.1.25.0/tools/ILRepack.exe

mono Core/packages/ILRepack.1.25.0/tools/ILRepack.exe \
	/target:exe \
	/out:ckan.exe \
	Cmdline/bin/Debug/CmdLine.exe \
	Cmdline/bin/Debug/CKAN-GUI.exe \
	Cmdline/bin/Debug/ChinhDo.Transactions.dll \
	Cmdline/bin/Debug/CKAN.dll \
	Cmdline/bin/Debug/CommandLine.dll \
	Cmdline/bin/Debug/ICSharpCode.SharpZipLib.dll \
	Cmdline/bin/Debug/log4net.dll \
	Cmdline/bin/Debug/Newtonsoft.Json.dll \
	Cmdline/bin/Debug/INIFileParser.dll \
    Cmdline/bin/Debug/CurlSharp.dll

