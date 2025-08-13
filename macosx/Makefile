.PHONY: clean app

CONFIGURATION?=Debug
NAME:=CKAN
APPNAME:=$(NAME).app
OUTDIR:=../_build/osx
DMGROOT:=$(OUTDIR)/dmg
APPROOT=$(DMGROOT)/$(APPNAME)
DMG:=$(OUTDIR)/$(NAME).dmg
SCRIPTSRC:=CKAN
SCRIPTDEST:=$(APPROOT)/Contents/MacOS/CKAN
BUILDDIR_x86_64_SRC:=../_build/out/CKAN-CmdLine/$(CONFIGURATION)/bin/net8.0/osx-x64/publish
BUILDDIR_x86_64_DEST:=$(APPROOT)/Contents/MacOS/x86_64
BUILDDIR_arm64_SRC:=../_build/out/CKAN-CmdLine/$(CONFIGURATION)/bin/net8.0/osx-arm64/publish
BUILDDIR_arm64_DEST:=$(APPROOT)/Contents/MacOS/arm64
ICNSSRC:=../assets/ckan.icns
ICNSDEST:=$(APPROOT)/Contents/Resources/CKAN.icns
PLISTSRC:=Info.plist.in
PLISTTMP:=$(OUTDIR)/Info.plist
PLISTDEST:=$(APPROOT)/Contents/Info.plist
CHANGELOGSRC:=../CHANGELOG.md
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' )

$(DMG): app
	mkdir -p $(OUTDIR)
	genisoimage -V $(NAME) -D -R -apple -no-pad -o $@ $(DMGROOT)

app: $(BUILDDIR_x86_64_DEST) $(BUILDDIR_arm64_DEST) $(SCRIPTDEST) $(ICNSDEST) $(PLISTDEST)
	rdfind -minsize 10240 -makeresultsfile false -makesymlinks true $(BUILDDIR_x86_64_DEST) $(BUILDDIR_arm64_DEST)
	symlinks -rc $(BUILDDIR_arm64_DEST)

$(BUILDDIR_x86_64_DEST): $(BUILDDIR_x86_64_SRC)
	mkdir -p $(shell dirname $@)
	cp -rT $< $@

$(BUILDDIR_arm64_DEST): $(BUILDDIR_arm64_SRC)
	mkdir -p $(shell dirname $@)
	cp -rT $< $@

$(SCRIPTDEST): $(SCRIPTSRC)
	mkdir -p $(shell dirname $@)
	ln -f $< $@

$(ICNSDEST): $(ICNSSRC)
	mkdir -p $(shell dirname $@)
	ln -f $< $@

$(PLISTDEST): $(PLISTSRC) $(CHANGELOGSRC)
	mkdir -p $(shell dirname $@)
	sed -e 's/@VERSION@/$(VERSION)/' $< > $(PLISTTMP)
	plistutil -i $(PLISTTMP) -o $@

clean:
	rm -rf $(OUTDIR)
