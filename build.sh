#!/bin/bash
set -x

xbuild /verbosity:minimal CKAN-GUI.sln
nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
# prove
