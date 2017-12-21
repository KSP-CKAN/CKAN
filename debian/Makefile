.PHONY: clean test

DESTDIR:=../_build/deb/fakeroot
EXESRC:=../_build/repack/Release/ckan.exe
EXEDEST:=$(DESTDIR)/usr/lib/ckan/ckan.exe
SCRIPTSRC:=ckan
SCRIPTDEST:=$(DESTDIR)/usr/bin/ckan
CONTROLSRC:=control.in
CONTROLDEST:=$(DESTDIR)/DEBIAN/control
CHANGELOGSRC:=../CHANGELOG.md
CHANGELOGDEST:=$(DESTDIR)/usr/share/doc/ckan/changelog.gz
DEBCHANGESRC:=changelog
DEBCHANGEDEST:=$(DESTDIR)/usr/share/doc/ckan/changelog.Debian.gz
MANSRC:=ckan.1
MANDEST:=$(DESTDIR)/usr/share/man/man1/ckan.1.gz
ICONSRC:=ckan.ico
ICONDEST:=$(DESTDIR)/usr/share/icons/ckan.ico
COPYRIGHTSRC:=copyright
COPYRIGHTDEST:=$(DESTDIR)/usr/share/doc/ckan/copyright
DESKTOPSRC:=ckan.desktop
DESKTOPDEST:=$(DESTDIR)/usr/share/applications/ckan.desktop
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' )
DEB:=../_build/deb/ckan_$(VERSION)_all.deb

$(DEB): $(EXEDEST) $(SCRIPTDEST) $(CONTROLDEST) $(CHANGELOGDEST) $(DEBCHANGEDEST) $(MANDEST) $(ICONDEST) $(COPYRIGHTDEST) $(DESKTOPDEST)
	umask 0022 && mkdir -p $(shell dirname $@)
	fakeroot dpkg-deb --build $(DESTDIR)/ $@

$(SCRIPTDEST): $(SCRIPTSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(EXEDEST): $(EXESRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(EXESRC):
	cd .. && ./build --configuration=Release

$(CONTROLDEST): $(CONTROLSRC) $(CHANGELOGSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && sed -e 's/@VERSION@/$(VERSION)/' $< > $@

$(CHANGELOGDEST): $(CHANGELOGSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(DEBCHANGEDEST): $(DEBCHANGESRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(MANDEST): $(MANSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && gzip --best -n -c $< > $@

$(ICONDEST): $(ICONSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(COPYRIGHTDEST): $(COPYRIGHTSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(DESKTOPDEST): $(DESKTOPSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

clean:
	rm -rf ../_build/deb

test: $(DEB)
	lintian $<
