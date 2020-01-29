.PHONY: clean

NAME:=CKAN
APPNAME:=$(NAME).app
OUTDIR:=../_build/osx
DMGROOT:=$(OUTDIR)/dmg
APPROOT=$(DMGROOT)/$(APPNAME)
DMG:=$(OUTDIR)/$(NAME).dmg
SCRIPTSRC:=CKAN
SCRIPTDEST:=$(APPROOT)/Contents/MacOS/CKAN
EXESRC:=../_build/repack/Release/ckan.exe
EXEDEST:=$(APPROOT)/Contents/MacOS/ckan.exe
ICNSSRC:=../assets/ckan.icns
ICNSDEST:=$(APPROOT)/Contents/Resources/CKAN.icns
PLISTSRC:=Info.plist.in
PLISTTMP:=$(OUTDIR)/Info.plist
PLISTDEST:=$(APPROOT)/Contents/Info.plist
CHANGELOGSRC:=../CHANGELOG.md
VERSION:=$(shell egrep '^\s*\#\#\s+v.*$$' $(CHANGELOGSRC) | head -1 | sed -e 's/^\s*\#\#\s\+v//' )

$(DMG): $(EXEDEST) $(SCRIPTDEST) $(ICNSDEST) $(PLISTDEST)
	mkdir -p $(OUTDIR)
	xorrisofs -V $(NAME) -D -R -no-pad -o $@ $(DMGROOT)

$(EXEDEST): $(EXESRC)
	mkdir -p $(shell dirname $@)
	ln $< $@

$(EXESRC):
	cd .. && ./build --configuration=Release

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
