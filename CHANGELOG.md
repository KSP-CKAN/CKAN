# Change Log

All notable changes to this project will be documented in this file.

## Unreleased

### Bugfixes

- [GUI] The CKAN client is less likely to believe that *all* mods are new when auto-updating metadata is enabled. (Postremus, #1369)

### Features

- [GUI] Updates to the text of some buttons, and change the check mark from blue to green. (plague006, #1352)

### Internal

- [Core] Additional tests against autodetected mods in the RelationshipResolver (Postremus and pjf, #1226 and #1355)
- [GUI] Removed a spurious warning when building (pjf, #1343)
- [NetKAN] Reading of information from `.version` files greatly improved, especially when mixing metadata from other sources like github and kerbalstuff. (dbent, #1299)
- [Core] Files can now be installed to `saves/scenarios` using the `Scenarios` target (pjf, #1360)

## v1.12.0

### Bugfixes

- [GUI] Hitting `cancel` is much more likely to actually cancel an install-in-progress (Postremus, #1325)
- [GUI] Fewer crashes when double-clicking on an auto-detected mod (Postremus, #1237)
- [Cmdline] `ckan compare` fails more gracefully when called without arguments (Postremus, #1283)
- [Cmdline] `ckan show` more accurately displays the cached filename (mgsdk, #1266)
- [Core] We fail more gracefully when mod metadata can't be downloaded (Postremus, #1291)

### Features

- [GUI] Installed mods can now be exported as a "favourites list" via `File -> Export Installed Mods`. Imported favourites lists allow the user to choose which mods they get, and will install the latest versions available for the user's version of KSP. (Postremus, #972)
- [GUI] Enter and escape can be used to accept and cancel changes when editing the command-line dialog (Postremus, #1318)
- [GUI] Yes/no dialog boxes have a more user-friendly title (plague006, #1312)
- [GUI] Downloading a file to the cache shows the download in progress, and refreshes the contents viewer when complete (Postremus, #1231)
- [GUI] On first start we always refresh the modlist, with an option to do so each time the CKAN is loaded (Postremus, #1285)
- [Core] KSP instance names now default to the folder in which they're installed (Postremus, #1261)
- [Core] Processing an updated mod list is now faster, and other speed enhancements (Postremus, #1229)
- [Core] Metadata is now downloaded in `.tar.gz` rather than `.zip` format, resulting in much faster downloads (pjf, #1344)
- [Spec] `install_to` can now target `Ships/` subdirectories (dbent, #1243)

### Internal

- [NetKAN] Cached files have more descriptive file extensions (dbent, #1308)
- [NetKAN] A warning is generated if a file without a `.netkan` extension is processed (dbent, #1308)
- [NetKAN] We better handle null values in retrieved metadata (dbent, #1324)
- [NetKAN] Better handling of license strings with leading and trailing spaces (dbent, #1305)
- [Core] We no longer try to use `libcurl` on systems where the .NET web classes are sufficient (dbent, #1294)
