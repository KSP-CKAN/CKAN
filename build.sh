#!/bin/sh

xbuild /verbosity:minimal CKAN-core.sln
nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
# prove
