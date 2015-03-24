#!/bin/sh
set -x

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

xbuild /verbosity:minimal CKAN-core.sln

# Find a suitable version of nunit.
echo "Checking if nunit-console is available..."
command -v "nunit-console" >/dev/null 2>&1 && check_nunit "nunit-console"

echo "Checking if nunit-console4 is available..."
command -v "nunit-console4" >/dev/null 2>&1 && check_nunit "nunit-console4"

# If we found a suitable nunit binary, continue with the testing.
if [ "$NUNIT_BINARY" != "" ]
then
    command $NUNIT_BINARY --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
else
    echo "Could not find a suitable version of nunit-console to run the tests. Skipping test execution."
fi
