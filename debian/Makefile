.PHONY: clean test

EXESRC=../_build/repack/Release/ckan.exe
EXEDEST=fakeroot/usr/lib/ckan/ckan.exe
SCRIPT=fakeroot/usr/bin/ckan
CONTROLIN=control.in
CONTROL=fakeroot/DEBIAN/control
CHANGELOG=../CHANGELOG.md
CHANGELOGZIP=fakeroot/usr/share/doc/ckan/changelog.gz
DEBCHANGE=changelog
DEBCHANGEZIP=fakeroot/usr/share/doc/ckan/changelog.Debian.gz
MAN=ckan.1
MANZIP=fakeroot/usr/share/man/man1/ckan.1.gz
VERSION=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOG) | head -1 | sed -e 's/^\s*\#\#\s\+v//' )
DEB=ckan_$(VERSION)_all.deb

$(DEB): $(EXEDEST) $(SCRIPT) $(CONTROL) $(CHANGELOGZIP) $(DEBCHANGEZIP) $(MANZIP)
	fakeroot dpkg-deb --build fakeroot/ .

$(EXEDEST): $(EXESRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(EXESRC):
	cd .. && ./build --configuration=Release

$(CONTROL): $(CONTROLIN) $(CHANGELOG)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && sed -e 's/@VERSION@/$(VERSION)/' $< > $@

$(CHANGELOGZIP): $(CHANGELOG)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(DEBCHANGEZIP): $(DEBCHANGE)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(MANZIP): $(MAN)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

clean:
	rm -rf $(DEB) $(EXEDEST) $(CONTROL) $(CHANGELOGZIP) $(DEBCHANGEZIP) $(MANZIP)

test: $(DEB)
	lintian $<
