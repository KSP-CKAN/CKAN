# Change Log

All notable changes to this project will be documented in this file.

## Unreleased

### Bugfixes
- [Core] In certain cases a `NullReferenceException` could be produced inside error handling code when processing the registry. (#1700 by: keyspace, reviewed: dbent)
- [GUI] Fix typo in export options. (#1718 by: dandrestor, reviewed: plague006)

### Features

- [GUI] Added "filter by description" search box. (#1632 by: politas; reviewed: pjf)
- [CLI] `compare` command now checks positive and negative rather than -1/+1 (#1649 by: dbent; reviewed: Daz)

### Internal

- [Multiple] Removed various references and code for processing mods on KerbalStuff. Thank you, Sircmpwn, for providing us with such a great service for so long. (#1615 by: Olympic1; reviewed: pjf)
- [Spec] Updated Spec with the `kind` field which was introduced in v1.6. (#1597 by: plague006; reviewed: Daz)
- [spec] ckan.schema now enforces structure of install directives (#1578 by: Zane6888; reviewed: pjf, Daz)
- [NetKAN] Catch ValueErrors rather than printing the trace (#1648 by: techman83; reviewed: Daz )
- [NetKAN] Catch ksp_version from SpaceDocks newly implemented game_version (#1655 by: dbent; reviewed: -)
- [NetKAN] Allow specifying when an override is executed (#1684 by: dbent; fixes: #1674)
- [NetKAN] Redirects to the download file are now resolved when using HTTP $krefs (#1696 by: dbent, reviewed: techman83)
- [NetKAN] Remote AVC files will be used in preference to ones stored in the archive if they have the same version (#1701 by: dbent, reviewed: techman83)
- [NetKAN] Add Download Attribute Transformer (#1710 by: techman83; addresses: #1682)

## v1.16.1

### Bugfixes

- [GUI] The "Not Installed" filter now has a more correct label. (#1573 by: plague006; reviewed: Postremus)
- [GUI] Scrolling of the mod-list no longer requires clicking on the list after start-up. (#1584 by: ChucklesTheBeard; reviewed: Olympic1)
- [GUI] The GUI now displays repo information as "Source Code" rather than "Github". (#1627 by: politas; reviewed: pjf)

### Features

- [GUI] The export menu now selects "favourites" as default, as that's almost always what people want. (#1609 by: plague006; reviewed: pjf)
- [Core/NetKAN] CKAN will now also work for mods that are hosted on SpaceDock. Use the new `$kref` "spacedock". (#1593 by: Olympic1, Zane6888; reviewed: pjf)
- [Core] CKAN has now an improved version sorting. (#1554 by: distantcam; reviewed: Olympic1)

### Internal

- [General] General code tidy-up. (#1582, #1602 by: ChucklesTheBeard; reviewed: plague006, Olympic1)
- [GUI] Avoidance of a future bug involving how we query users regarding choices. (#1538 by: pjf, RichardLake; reviewed: Postremus)
- [GUI] Fixed mispellings in the word "directory". (#1624 by: tonygambone; reviewed: pjf)
- [Spec] Updated Spec with newer `netkan.exe` features. (#1581 by: dbent; reviewed: Dazpoet)
- [NetKAN] `netkan.exe` now has support for downloading GitHub sources of a release. (#1587 by: dbent; reviewed: Olympic1)
- [NetKAN] `netkan.exe` checks for malformed url's and prevents them from being added to the metadata. (#1580 by: dbent; reviewed: Olympic1)
- [NetKAN] `netkan.exe` will now add all authors listed on SpaceDock (#1600,#1620 by: dbent; reviewed: techman83)
- [Core] Spelling mistake in documentation fixed (#1623 by: Dazpoet; reviewed: pjf)
- [Reporting] Creation of an issues template to help with bug reporting. (#1596 and #1598 by plague006, Shuudoushi; reviewed: Dazpoet, Olympic1)

## v1.16.0

### Bugfixes

- [GUI] CKAN handlers added to `mimeapps.list` in a more cross-platform friendly fashion. (danielrschmidt, #1536)

### Features

- [Core] Better detection of KSP installs in non-standard Steam locations (LarsOL #1444, pjf #1481)
- [Core] `find` and `find_regexp` install directives will match files as well as directories if the `find_matches_files` field is set to `true`. (dbent #1241)
- [Core/GUI] Missing directories in `Ships` will be recreated as needed. (Wetmelon, #1525)
- [Core] Framework added to allow fuzzy version checking, including "you're on your own" comparisons where KSP version checks are disabled. Updated spec to include `ksp_version_strict`, which enforces strict versioning. (pjf #1499)
- [Core] Thumbs subdirectories in `Ships` can now be directly targeted by install stanzas. (Postremus, #1448)

### Internal

- [NetKAN] `netkan.exe` will now sort `conflicts` relationships next to other relationships. (dbent)
- [NetKAN] `netkan.exe` now has much better support for Jenkins CI servers, allowing full automation. (dbent)

## v1.14.3 (Haumea)

### Bugfixes

- [Core] CKAN is more likely to find your KSP install. (McJones, #1480)
- [Core] Uninstalled mods will no longer be reported as upgradeable when another mod provides their fuctionality. (Postremus, #1449)
- [GUI] Installing a `.ckan` virtual mod will crash the client with an NRE less often (Postremus, #1478)
- [GUI] The "Installing mods" tab is now called the "Status log" tab, as it's used for upgrading and removing mods too. (plague006, #1460)
- [GUI] Links to `ckan://` resources under Linux are more likely to be handled correctly. (Postremus, #1434)
- [GUI] Mods upgrades with additional dependencies are better handled and displayed to the user. (Postremus, #1447)

### Features

- [GUI] The CKAN Identifer for each mod is now shown in their metadata panel. (plague006, #1476)
- [GUI] Double-clicking on a filename in the 'Contents' panel now opens the directory containing that file. (Postremus, #1443)
- [GUI] The progress bar now shows the progress of downloading to the cache. (Postremus, #1445)
- [GUI] Mods can now be searched by their CKAN identifier in the name textbox (Postremus, #1475)

### Internal

- [Internal] `Module` and `CkanModule` are finally merged into the same class! (Postremus, #1440)

## v1.14.2 (Makemake)

### Bugfixes

- [GUI] Numerical columns can now be sorted numerically. (Postremus, #1420)
- [GUI] Clicking on rows in suggests, recommends, and requirement pickers now selects the whole row, not just the cell clicked. (Postremus, #1438)

### Features

- [GUI] Updating the list of available mods will no longer clear user selections. (Postremus, #1402)
- [GUI] Mods can be search by abbreviation by typing directly into the modlist, as well as the search bar. (Postremus, #1430)
- [GUI] Mods can be filtered by locally cached status (Postremus, #1426)

### Internal

- [Updater] Checking for updates takes less network resources, and is more resilient to malformed release notes. (Postremus #1410; pjf #1453)
- [Core] We now cache the results of cache look-ups (so you can cache while you cache... faster). (pjf, #1454)

## v1.14.1 (Eris)

### Bugfixes

- [GUI] Re-ordering repositories in the settings panel is more stable. (Postremus, #1431)
- [GUI] Fixed an unhandled exception that could occur when installing metapages via `Install -> From .ckan`. (Postremus, #1436)
- [Core] Less likely to remove essential directories (such as `Ships/*`) if empty. (Postremus, #1405)

### Features

- [All] When installing and uninstalling mods, the mod name and version will be used rather than the internal identifier. (Postremus, #1401)
- [GUI] The GUI changeset screen now provides explanations as to why changes are being made. (Postremus, #1412)

### Internal

- [NetKAN] `netkan.exe` will now report its version and exit if run with the `--version` switch. (pjf, #1415)

## v1.14.0 (Mimas)

### Bugfixes

- [GUI] The CKAN client is less likely to believe that *all* mods are new when auto-updating metadata is enabled. (Postremus, #1369)
- [GUI] The latest CKAN-available version of a mod is always shown in the 'latest' column, even if that mod is not compatible with the installed KSP version. (Postremus, #1396)
- [GUI] Pressing a key with an empty modlist will no longer crash the client. (Postremus, #1329)
- [GUI] The 'mark for update' button no longer highlights when the only upgrade candidates are autodetected mods we can't actually upgrade. (Postremus, #1392)
- [Core] Installing from `.ckan` files (such as exported modlists) is more likely to succeed even when dependencies are complex. (#1337, Postremus)
- [Cmdline] `ckan.exe --verbose` and `ckan.exe --debug` now start the GUI with the appropriate logging mode. (#1403, Postremus)
- [Updater] We'll no longer try to download a CKAN release that hasn't finished building its assets yet. (Postremus, #1397)

### Features

- [GUI] Updates to the text of some buttons, and change the check mark from blue to green. (plague006, #1352)
- [GUI] The main display shows the download size if known. (Postremus, #1399)
- [GUI] Suggested and recommended mods can now be (de)selected with a single click. (martinnj, #1398)
- [GUI] Mods can be searched by their abbreviation, which we generate by taking the first letter of each word in the title. For example, `KIS` will match `Kerbal Inventory System`. (Postremus, #1394)
- [GUI] The side metadata panel and its elements can now be resized. (martinnj, #1378)
- [GUI] Mod filters will no longer reset to 'compatible' each time the client is opened. (Postremus, #1384)
- [GUI] The repository list no longer has weird numbers visible that we only use internally, and may eventually remove. (Postremus, #1393)

### Internal

- [Core] Additional tests against autodetected mods in the RelationshipResolver. (Postremus and pjf, #1226 and #1355)
- [GUI] Removed a spurious warning when building. (pjf, #1343)
- [NetKAN] Reading of information from `.version` files greatly improved, especially when mixing metadata from other sources like github and kerbalstuff. (dbent, #1299)
- [Core] Files can now be installed to `saves/scenarios` using the `Scenarios` target. (pjf, #1360)
- [Spec] Grammar corrections. (Postremus, #1400)
- [NetKAN] Files produced by `netkan.exe` have more stable field ordering. (dbent, #1304)
- [NetKAN] `netkan.exe` can use regexps to manipulate version strings. (dbent, #1321)
- [GUI] Refactoring to remove duplicated code. (Postremus, #1362)

## v1.12.0 (Veil Nebula)

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
