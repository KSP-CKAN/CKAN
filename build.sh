#!/bin/sh

xbuild /verbosity:minimal CKAN-cmdline.sln
nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
# prove
