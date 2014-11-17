# Are you working on the command line?
# Do you love unix tools?
# Is your name Travis?
# Then rejoice! You can type 'make test' to build and test!

# This makefile isn't a very good make-file; it only has one
# real target, which is make test'.

all: test

test:
	bin/build
	nunit-console --exclude=FlakyNetwork build/Tests/bin/Debug/Tests.dll
	prove

clean:
	rm ckan.exe netkan.exe
	rm -r build
