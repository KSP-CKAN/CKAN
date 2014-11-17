# Are you working on the command line?
# Do you love unix tools?
# Is your name Travis?
# Then rejoice! You can type 'make test' to build and test!

TARGET=Debug

CKAN := ckan.exe
NETKAN := netkan.exe
TESTS := build/Tests/bin/$(TARGET)/Tests.dll

CKAN_DYN := build/CmdLine/bin/$(TARGET)/CmdLine.exe build/CmdLine/bin/$(TARGET)/CKAN-GUI.exe $(shell find ./build/CmdLine/bin/Debug/ -name "*.dll")
NETKAN_DYN := build/NetKAN/bin/$(TARGET)/NetKAN.exe $(shell find ./build/NetKAN/bin/Debug/ -name "*.dll")

CKAN_SOURCE := $(shell find ./CKAN/CKAN -name "*.cs")
NETKAN_SOURCE := $(shell find ./CKAN/NetKAN -name "*.cs")
TESTS_SOURCE := $(shell find ./CKAN/Tests -name "*.cs")

STATIC := $(CKAN) $(NETKAN) $(TESTS)
DYNAMIC := $(CKAN_DYN) $(NETKAN_DYN)

all: static

build: static

static: $(STATIC)

dynamic:  $(DYNAMIC)

test: static
	nunit-console --exclude=FlakyNetwork build/Tests/bin/Debug/Tests.dll
	prove

$(CKAN): $(CKAN_DYN)
	bin/build

$(CKAN_DYN): $(CKAN_SOURCE)
	bin/build

$(NETKAN): $(NETKAN_DYN)
	bin/build

$(NETKAN_DYN): $(NETKAN_SOURCE)
	bin/build

$(TESTS): $(TESTS_SOURCE)
	bin/build

clean:
	rm $(STATIC) $(DYNAMIC)
	rm -r build
