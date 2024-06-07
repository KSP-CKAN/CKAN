.PHONY: clean test repo

# Borrow files from .deb
DEBDIR:=$(shell pwd)/../debian

TOPDIR:=$(shell pwd)/../_build/rpm
SCRIPTSRC:=$(DEBDIR)/ckan
MANSRC:=$(DEBDIR)/ckan.1
DESKTOPSRC:=$(DEBDIR)/ckan.desktop
CONSOLEUIDESKTOPSRC:=$(DEBDIR)/ckan-consoleui.desktop
CMDPROMPTDESKTOPSRC:=$(DEBDIR)/ckan-cmdprompt.desktop
ICONSIZES=16 32 48 64 96 128 256
ICONSRC:=$(foreach size,$(ICONSIZES),../assets/ckan-$(size).png)
CONFIGURATION?=Release
EXESRC:=$(shell pwd)/../_build/repack/$(CONFIGURATION)/ckan.exe
CHANGELOGSRC:=../CHANGELOG.md
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' -e 's/-/_/g' ).$(shell date +'%g%j')
RPM:=$(TOPDIR)/RPMS/noarch/ckan-$(VERSION)-1.noarch.rpm

CREATEREPO:=$(if $(shell which createrepo),createrepo,createrepo_c)
REPODIR:=$(TOPDIR)/repo
CODENAME?=nightly
REPOFILE:=ckan_$(CODENAME).repo
REPORPM:=$(REPODIR)/ckan-$(VERSION)-1.noarch.rpm
REPOXML:=$(REPODIR)/repodata/repomd.xml
REPOSIG:=$(REPOXML).asc
GPGNAME:=KSP CKAN <debian@ksp-ckan.space>

# rpmbuild tries to use $HOME/rpmbuild by default
# rpmbuild can't handle relative paths for its topdir
# rpmbuild assumes we'll copy files into its SOURCES dir for it

$(RPM): $(SCRIPTSRC) $(EXESRC) $(MANSRC) $(DESKTOPSRC) $(ICONSRC) $(CONSOLEUIDESKTOPSRC) $(CMDPROMPTDESKTOPSRC)
	mkdir -p "${TOPDIR}/SOURCES"
	cp $^ "${TOPDIR}/SOURCES"
	rpmbuild --define "_topdir ${TOPDIR}" --define "_version $(VERSION)" -bb ckan.spec

repo: $(REPOSIG)

$(REPOSIG): $(REPOXML)
	gpg --detach-sign --armor $<

$(REPOXML): $(REPORPM)
	$(CREATEREPO) "$(REPODIR)"

$(REPORPM): $(RPM) $(REPOFILE)
	mkdir -p "$(REPODIR)"
	cp $^ "$(REPODIR)"
	rpm --define '%_gpg_name $(GPGNAME)' \
	    --define '%__gpg $(shell which gpg)' \
	    --addsign $@

$(EXESRC):
	cd .. && ./build --configuration=$(CONFIGURATION)

clean:
	rm -r "$(TOPDIR)"

test: $(RPM)
	rpm -qlp $<
	rpmlint $<
