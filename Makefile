# Are you working on the command line?
# Do you love unix tools?
# Then rejoice! You can type 'make test' to build and test!

TARGET=Debug

all: build

build:
	xbuild /verbosity:minimal CKAN-core.sln

test: build
	nunit-console --exclude=FlakyNetwork Tests/bin/Debug/Tests.dll
