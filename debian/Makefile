.PHONY: clean test repo sign

DESTDIR:=../_build/deb/fakeroot
CONFIGURATION?=Release
EXESRC:=../_build/repack/$(CONFIGURATION)/ckan.exe
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
ICONSIZES=16 32 48 64 96 128 256
ICONDEST:=$(foreach size,$(ICONSIZES),$(DESTDIR)/usr/share/icons/hicolor/$(size)x$(size)/apps/ckan.png)
COPYRIGHTSRC:=copyright
COPYRIGHTDEST:=$(DESTDIR)/usr/share/doc/ckan/copyright
DESKTOPSRC:=ckan.desktop
DESKTOPDEST:=$(DESTDIR)/usr/share/applications/ckan.desktop
CONSOLEUIDESKTOPSRC:=ckan-consoleui.desktop
CONSOLEUIDESKTOPDEST:=$(DESTDIR)/usr/share/applications/ckan-consoleui.desktop
CMDPROMPTDESKTOPSRC:=ckan-cmdprompt.desktop
CMDPROMPTDESKTOPDEST:=$(DESTDIR)/usr/share/applications/ckan-cmdprompt.desktop
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' ).$(shell date +'%g%j')
DEB:=../_build/deb/ckan_$(VERSION)_all.deb

# Files for /deb/ (the .deb file)
APT_REPO_ROOT:=../_build/deb/apt-repo-root
REPODEB:=$(APT_REPO_ROOT)/pool/ckan_$(VERSION)_all.deb

# Files for /deb/dists/{stable,nightly}/
# (Release, main/binary-amd64/Release, main/binary-amd64/Packages.gz,
#  actual path to be determined by yml files)
APT_REPO_DIST:=../_build/deb/apt-repo-dist
RELEASESRC:=Release.in
DISTRELEASEDEST:=$(APT_REPO_DIST)/Release
SIGNEDDISTRELEASEDEST:=$(DISTRELEASEDEST).gpg
ARCHRELEASEDEST:=$(APT_REPO_DIST)/main/binary-amd64/Release
INRELEASE:=$(APT_REPO_DIST)/InRelease
PACKAGES:=$(APT_REPO_DIST)/main/binary-amd64/Packages.gz

repo: $(REPODEB) $(PACKAGES) $(DISTRELEASEDEST) $(ARCHRELEASEDEST)

sign: $(INRELEASE) $(SIGNEDDISTRELEASEDEST)

$(REPODEB): $(DEB)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(DISTRELEASEDEST): $(RELEASESRC) $(PACKAGES)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && sed -e 's/@VERSION@/$(VERSION)/' \
	-e "s/@CODENAME@/$${CODENAME-nightly}/" \
	-e 's/@PACKAGESPATH@/main\/binary-amd64\//' \
	-e "s/@DATE@/$$(date -R -u)/" \
	-e 's/@RAWHASH@/'$$(zcat $(PACKAGES) | sha256sum -t | cut -d' ' -f1)/ \
	-e 's/@RAWSIZE@/'$$(zcat $(PACKAGES) | wc -c)/ \
	-e 's/@GZHASH@/'$$(sha256sum -t < $(PACKAGES) | cut -d' ' -f1)/ \
	-e 's/@GZSIZE@/'$$(stat -c %s $(PACKAGES))/ $< > $@

$(ARCHRELEASEDEST): $(RELEASESRC) $(PACKAGES)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && sed -e 's/@VERSION@/$(VERSION)/' \
	-e "s/@CODENAME@/$${CODENAME-nightly}/" \
	-e 's/@PACKAGESPATH@//' \
	-e "s/@DATE@/$$(date -R -u)/" \
	-e 's/@RAWHASH@/'$$(zcat $(PACKAGES) | sha256sum -t | cut -d' ' -f1)/ \
	-e 's/@RAWSIZE@/'$$(zcat $(PACKAGES) | wc -c)/ \
	-e 's/@GZHASH@/'$$(sha256sum -t < $(PACKAGES) | cut -d' ' -f1)/ \
	-e 's/@GZSIZE@/'$$(stat -c %s $(PACKAGES))/ $< > $@

$(PACKAGES): $(REPODEB)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && (cd $(APT_REPO_ROOT) && dpkg-scanpackages -m pool) | gzip -c > $@

$(INRELEASE): $(DISTRELEASEDEST)
	gpg --clearsign -o $@ $<

$(SIGNEDDISTRELEASEDEST): $(DISTRELEASEDEST)
	gpg -abs -o $@ $<

$(DEB): $(EXEDEST) $(SCRIPTDEST) $(CONTROLDEST) $(CHANGELOGDEST) $(DEBCHANGEDEST) $(MANDEST) $(ICONDEST) $(COPYRIGHTDEST) $(DESKTOPDEST) $(CONSOLEUIDESKTOPDEST) $(CMDPROMPTDESKTOPDEST)
	umask 0022 && mkdir -p $(shell dirname $@)
	fakeroot dpkg-deb --build $(DESTDIR)/ $@

$(SCRIPTDEST): $(SCRIPTSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(EXEDEST): $(EXESRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(EXESRC):
	cd .. && ./build --configuration=$(CONFIGURATION)

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

define icon_rule_template =
$$(DESTDIR)/usr/share/icons/hicolor/$(1)x$(1)/apps/ckan.png: ../assets/ckan-$(1).png
	umask 0022 && mkdir -p $$(shell dirname $$@)
	umask 0022 && cp $$< $$@
endef
$(foreach size,$(ICONSIZES),$(eval $(call icon_rule_template,$(size))))

$(COPYRIGHTDEST): $(COPYRIGHTSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(DESKTOPDEST): $(DESKTOPSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(CONSOLEUIDESKTOPDEST): $(CONSOLEUIDESKTOPSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

$(CMDPROMPTDESKTOPDEST): $(CMDPROMPTDESKTOPSRC)
	umask 0022 && mkdir -p $(shell dirname $@)
	umask 0022 && cp $< $@

clean:
	rm -rf ../_build/deb

test: $(DEB)
	lintian $<
