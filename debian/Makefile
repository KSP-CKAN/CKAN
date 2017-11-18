.PHONY: clean test

EXESRC=../_build/ckan.exe
EXEDEST=fakeroot/usr/lib/ckan/ckan.exe
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

$(DEB): $(EXEDEST) $(CONTROL) $(CHANGELOGZIP) $(DEBCHANGEZIP) $(MANZIP)
	fakeroot dpkg-deb --build fakeroot/ .

$(EXEDEST): $(EXESRC)
	mkdir -p $(shell dirname $@)
	cp $< $@

$(EXESRC):
	cd .. && ./build

$(CONTROL): $(CONTROLIN) $(CHANGELOG)
	mkdir -p $(shell dirname $@)
	umask 0022 && sed -e 's/@VERSION@/$(VERSION)/' $< > $@

$(CHANGELOGZIP): $(CHANGELOG)
	mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(DEBCHANGEZIP): $(DEBCHANGE)
	mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(MANZIP): $(MAN)
	mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

clean:
	rm -rf $(DEB) $(EXEDEST) $(CONTROL) $(CHANGELOGZIP) $(DEBCHANGEZIP) $(MANZIP)

test: $(DEB)
	lintian $<
