#!/bin/sh

xbuild /verbosity:minimal CKAN-netkan.sln
nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
# prove
