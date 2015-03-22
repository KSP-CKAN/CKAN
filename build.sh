#!/bin/bash
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

xbuild /verbosity:minimal CKAN-GUI.sln

# Find a suitable version of nunit.
declare -a VERSIONS=("nunit-console" "nunit-console4")

for i in "${VERSIONS[@]}"
do
    echo "Checking if $i is available..."
    command -v "$i" >/dev/null 2>&1 && check_nunit $i
done

# If we found a suitable nunit binary, continue with the testing.
if [ "$NUNIT_BINARY" != "" ]
then
    command $NUNIT_BINARY --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
else
    echo "Could not find a suitable version of nunit-console to run the tests. Skipping test execution."
fi
