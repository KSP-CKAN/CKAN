.PHONY: clean test

# Borrow files from .deb
DEBDIR:=$(shell pwd)/../debian

TOPDIR:=$(shell pwd)/../_build/rpm
SCRIPTSRC:=$(DEBDIR)/ckan
MANSRC:=$(DEBDIR)/ckan.1
DESKTOPSRC:=$(DEBDIR)/ckan.desktop
CONSOLEUIDESKTOPSRC:=$(DEBDIR)/ckan-consoleui.desktop
ICONSRC:=$(DEBDIR)/ckan.ico
CONFIGURATION?=Release
EXESRC:=$(shell pwd)/../_build/repack/$(CONFIGURATION)/ckan.exe
CHANGELOGSRC:=../CHANGELOG.md
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' -e 's/-/_/g' )
RPM:=$(shell pwd)/../_build/rpm/RPMS/noarch/ckan-$(VERSION)-1.noarch.rpm

# rpmbuild tries to use $HOME/rpmbuild by default
# rpmbuild can't handle relative paths for its topdir
# rpmbuild assumes we'll copy files into its SOURCES dir for it

$(RPM): $(SCRIPTSRC) $(EXESRC) $(MANSRC) $(DESKTOPSRC) $(ICONSRC) $(CONSOLEUIDESKTOPSRC)
	mkdir -p "${TOPDIR}/SOURCES"
	cp $^ "${TOPDIR}/SOURCES"
	rpmbuild --define "_topdir ${TOPDIR}" --define "_version $(VERSION)" -bb ckan.spec

$(EXESRC):
	cd .. && ./build --configuration=$(CONFIGURATION)

clean:
	rm -r "$(TOPDIR)"

test: $(RPM)
	rpm -qlp $<
	rpmlint $<
