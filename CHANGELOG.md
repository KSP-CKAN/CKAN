# Change Log

All notable changes to this project will be documented in this file.

## v1.24.1

### Bugfixes

- [Multiple] Fix crash when trying to view empty auth token list (#2301 by: HebaruSan; reviewed: politas)
- [GUI] Handle mod not found in versions tab (#2303 by: HebaruSan; reviewed: politas)
- [GUI] Better resizing for Select KSP Install window (#2306 by: HebaruSan; reviewed: politas)
- [GUI] Fix GUI sort by size (#2311 by: HebaruSan; reviewed: politas)
- [Core] Don't crash if download_hash isn't set (#2313 by: HebaruSan; reviewed: politas)
- [GUI] Fix GUI instance name checking (#2316 by: HebaruSan; reviewed: politas)
- [Core] Fix ArgumentOutOfRangeException when removing files from game root (#2332 by: HebaruSan; reviewed: politas)
- [Core] Obey version properties of conflicts and depends relationships in sanity checks (#2339 by: HebaruSan; reviewed: politas)
- [Netkan] Invalidate stale cached files from GitHub in Netkan (#2337 by: HebaruSan; reviewed: politas)

### Internal

- [Build] Improve CKAN.app launch script (#2329 by: HebaruSan; reviewed: politas)
- [Build] Fix building on macOS (#2341 by: phardy; reviewed: HebaruSan, politas)
- [Build] Fix autoupdater tests on TLS-fragile platforms (#2344 by: HebaruSan; reviewed: politas)

## v1.24.0 (Bruce)

### Features

- [Multiple] Save timestamped .ckan files after we save the registry (#2239 by: HebaruSan; reviewed: politas)
- [GUI] Add status and progress bar at the bottom of the window (#2245 by: HebaruSan; reviewed: Olympic1)
- [GUI] Add import downloads menu item to GUI (#2246 by: HebaruSan; reviewed: politas)
- [Core] Accept header and infrastructure for auth tokens (#2263 by: HebaruSan; reviewed: dbent)
- [CLI] Add Cmdline import command (#2264 by: HebaruSan; reviewed: politas)
- [Multiple] User interfaces for auth tokens (#2266 by: HebaruSan; reviewed: politas)
- [CLI] Add a read-execute-print-loop prompt for Cmdline (#2273 by: HebaruSan; reviewed: politas)
- [Core] Fallback to archive.org URLs for failed downloads of FOSS packages (#2284 by: HebaruSan; reviewed: techman83, politas)
- [CLI] Show abstracts in available command (#2286 by: HebaruSan; reviewed: politas)

### Bugfixes

- [GUI] Check provides for optional dependencies in GUI (#2240 by: HebaruSan; reviewed: politas)
- [GUI] Update registry at start of GUI if available_modules is empty (#2241 by: HebaruSan; reviewed: politas)
- [GUI] Allow uninstallation of mods while Incompatible filter is selected (#2242 by: HebaruSan; reviewed: politas)
- [Core] Validate downloaded files against metadata before adding to cache (#2243 by: HebaruSan; reviewed: politas)
- [Core] Fix missing filename in install -c log message (No PR, by: HebaruSan)
- [GUI] Leave out children already shown in ancestor node (#2258 by: HebaruSan; reviewed: politas)
- [GUI] Resolve provides for install-from-ckan-file (#2259 by: HebaruSan; reviewed: politas)
- [Build] Use arch=32 for OSX (#2270 by: HebaruSan; reviewed: techman83)
- [Multiple] Retry of failed downloads (#2277 by: HebaruSan; reviewed: politas)
- [CLI] Print fewer download updates in headless mode (#2256 by: HebaruSan; reviewed: politas)
- [Core] Point to wiki page about certs on cert errors (#2279 by: HebaruSan; reviewed: politas)
- [Multiple] Handle invalid installs better (#2283 by: HebaruSan; reviewed: politas)
- [Core] Capture error details from SharpZipLib for invalid ZIPs (#2287 by: HebaruSan; reviewed: politas)
- [Netkan] Check zip validity in netkan (#2288 by: HebaruSan; reviewed: politas)
- [Core] Replace colons with hyphens in archive URLs (#2290 by: HebaruSan; reviewed: techman83)
- [Core] Force-allow TLS 1.2 on .NET 4.5 (#2297 by: HebaruSan; reviewed: politas)


## v1.24.0-PRE1 (McCandless)

### Features

- [Core] Add spec/schema to implement mod tags (#2034 by: smattiso; reviewed: ayan4m1, dbent, politas)
- [Spec] Add CC-BY-ND licence options (#2160 by: MoreRobustThanYou; reviewed: Olympic1, politas)
- [GUI] Change icon on filter button to something filter-y (#2156 by: politas; reviewed: HebaruSan)
- [Core] Add include_only fields (#1577, #2170 by: Zane6888, Olympic1; reviewed: politas)
- [ConsoleUI] Create text UI inspired by Turbo Vision (#2177 by: HebaruSan; reviewed: dbent, Maxzhao, pjf, ProfFan, politas)
- [Build] Create and release CKAN.app for Mac OS X (#2225 by: HebaruSan; reviewed: politas)
- [Build] Debian package build system (#2187 by: HebaruSan; reviewed: politas)
- [Core] Prompt to reinstall on change to include_only (No PR by: HebaruSan; reviewed: politas)
- [GUI] Remember splitter positions and whether the window was maximized (#2234 by: politas; reviewed: HebaruSan)
- [GUI] Add right-click context menu (#2202 by: Olympic1; reviewed: HebaruSan, politas)

### Bugfixes

- [Build] Add skip_deploy to GitHub release deploy provider (#2151 by: dbent; reviewed: politas)
- [Build] Fix build errors for UpdateCol (#2153 by: politas; reviewed: Olympic1)
- [Auto-updater] Move AskForAutoUpdates dialog to center of screen (#2165 by: politas; reviewed: Olympic1)
- [Core] Clean up registry lock file after parse failure (#2175 by: HebaruSan; reviewed: politas)
- [Core] Purge 6 MB of bloat from `registry.json` (#2179 by: HebaruSan; reviewed: politas)
- [Build] Only check first three segments of version in ci (#2192, #2195 by: HebaruSan; reviewed: techman83, Olympic1)
- [GUI] Initialize checkboxes to desired value at creation (#2184 by: HebaruSan; reviewed: mwerle, politas)
- [GUI] Avoid crash with unavailable installed mod, improve error messages (#2188 by: HebaruSan; reviewed: politas)
- [CLI] Fix cmdline help text problems (#2197 by: HebaruSan; reviewed: politas)
- [CLI] Dispose registry managers before exit to prevent exceptions (#2203 by: HebaruSan; reviewed: politas)
- [GUI/CLI] Avoid NRE in install-from-ckan (#2205 by: HebaruSan; reviewed: politas)
- [GUI] Avoid NRE in TooManyModsProvide (#2209 by: HebaruSan; reviewed: politas)
- [Core] Install version from file when installing from file (#2211 by: HebaruSan; reviewed: politas, techman83)
- [GUI] Show mods with incompatible dependencies (#2216 by: HebaruSan; reviewed: politas)
- [GUI] Fix missing entries in dependency graphs (#2226 by: HebaruSan; reviewed: politas)
- [Multiple] Add depending mod to missing dependency exception (#2215 by: HebaruSan; reviewed: politas)
- [CLI] Check game version compatibility when installing specific version (#2228 by: HebaruSan; reviewed: techman83, politas)
- [CLI] Make Cmdline modules case insensitive (#2223 by: HebaruSan; reviewed: politas)
- [Build] Provide fresh auto updater in releases (#2212 by: HebaruSan; reviewed: politas)
- [CLI] Don't try to remove autodetected DLLs (#2232 by: HebaruSan; reviewed: politas)
- [Core] Use shared installer code in GUI (#2233 by: HebaruSan; reviewed: Olympic1)
- [Multiple] Include invalid instances in KSPManager (#2230 by: HebaruSan; reviewed: politas)
- [Build] Check version of PowerShell in build script (#2235 by: HebaruSan; reviewed: Olympic1)
- [Multiple] Add and change logging to make INFO readable (#2236 by: HebaruSan; reviewed: politas)
- [Multiple] Use shared installer code in GUI and fix reinstall problems (#2233 by: HebaruSan; reviewed: Olympic1, politas)
- [Multiple] Don't clear available modules till after the new list is ready (#2238 by: HebaruSan; reviewed: politas)

### Internal

- [Build] Build Update (#2158 by: dbent; reviewed: Olympic1, politas)
- [Build] Establish a .gitattributes file (#2169 by: Olympic1; reviewed: politas)
- [Build] Remove unnecessary using directives (#2181 by: HebaruSan; reviewed: politas)
- [Build] Cleanup project (#2182 by: Olympic1; reviewed: HebaruSan, politas, dbent)
- [Core] Simplify IUser (#2163 by: HebaruSan; reviewed: politas)
- [NetKAN] Adapt Curse API to new widget (#2189 by: HebaruSan; reviewed: Olympic1)
- [Reporting] Improvement of issues template to help with bug reporting (#2201 by: HebaruSan; reviewed: Olympic1)

## v1.22.6 (Guiana)

### Bugfixes

- [GUI] Fix search box tab order (#2141 by: HebaruSan; reviewed: politas)
- [Core] Check for stale lock files (#2139 by: HebaruSan; reviewed: politas)
- [NetKAN] Improve error output (#2144 by: HebaruSan; reviewed: Olympic1)
- [GUI] REVERT #1929: Allow uninstall of incompatible mods in GUI (#2150 by: politas)

## v1.22.5 (Xichang)

### Bugfixes

- [GUI] Fix crash on startup (#2138 by: HebaruSan; reviewed: Olympic1)
- [Core] Fix exception installing some mods (#2137 by: HebaruSan; reviewed: Olympic1)

## v1.22.4 (Uchinoura)

### Bugfixes

- [GUI] Update Forum Thread link to new thread (#2079 by: politas; reviewed: linuxgurugamer)
- [Core] Move downloads outside of gui transaction (#2073 by: archer884; reviewed: politas)
- [CLI] Fix crash in "ckan available" with curly braces in mod name (#2111 by: HebaruSan; reviewed: politas)
- [Core] Check grandparent+ directories for find and find_regexp (#2120 by: HebaruSan; reviewed: politas)
- [Core] Add test to cover missing directory entries (#2125 by: HebaruSan; reviewed: politas)
- [Build] Fix build.ps1 script failing when spaces exist in source path (#2121 by: ayan4m1; reviewed: dbent)
- [Core] Perform directory root comparison in case-insensitive way (#2122 by: ayan4m1; reviewed: politas)
- [Build] Expand .gitignore to handle packages in subdirectories and build output (#2116 by: ayan4m1; reviewed: politas)
- [GUI] Allow uninstall of incompatible mods in GUI (#1929 by: ayan4m1; reviewed: politas)
- [Core] Loop to find max KSP version instead of assuming ordering (#2131 by: HebaruSan; reviewed: politas)

### Features

- [GUI] Quit on ctrl-q and alt-f,x (#2132 by: HebaruSan; reviewed: politas)

## v1.22.3 (Mahia (bugfix release))

### Bugfixes

- [Core] Fix broken Chicken bits from #2023 (#2058 by: politas; reviewed: -)

## v1.22.2 (Mahia)

### Bugfixes

- [CLI] Removed non-functioning code on available command (#1966 by: politas; reviewed: dbent)
- [Core] Switch Linux and MacOS to native C# downloads (#2023 by: politas; reviewed: pjf)
- [NetKAN] Convert spaces to %20 in Curse URLs (#2041 by: politas; reviewed: -)

### Features

- [Build] Use Cake for build and overhaul/cleanup build (#1589 by: dbent; reviewed: techman83, politas)
- [Build] Docker updates to support cake! (#1988 by: mathuin; reviewed: dbent)
- [Build] Update Build packages (#2028 by: dbent; reviewed: Olympic1)
- [Build] Update Build for Mono 5.0.0 (#2049 by: dbent; reviewed: politas)
- [Build] Update Update build (#2050 by: dbent; reviewed: politas)
- [Core] Update KSP builds (#2056 by: Olympic1; reviewed: linuxgurugamer)
- [NetKAN] Canonicalize non-raw GitHub URIs (#2054 by: dbent; reviewed: politas)

## v1.22.1 (Georgy)

### Bugfixes

- [GUI] Fix for black rows in GUI on conflicting mods. (#1968 by: Rohaq; reviewed: politas)

## v1.22.0 (Valentina)

### Bugfixes

- [Build] Update SharpZip dependency and remove old Newtonsoft.Json (#1879 by: ayan4m1; reviewed: politas)
- [Core] Make sure we're updating the build mappings on repository update (#1906 by: dbent; reviewed: Olympic1)
- [Core/GUI] Fix TargetInvocationException and improve mod conflict GUI test (#1371, #1373 by: Postremus, #1908 by: ayan4m1; reviewed: politas)
- [Multiple] Fix default logging on fallback when no XML file (#1920 by: politas; reviewed: ayan4m1, mathuin)
- [GUI] Update UI State on cache events (#1930 by: ayan4m1; reviewed: politas)
- [GUI] Use SystemColors to source various UI colors (#1926 by: ayan4m1; reviewed: politas)

### Features

- [Multiple] Add log4net.xml, refactor logging init and log to file by default (#1881 by: ayan4m1; reviewed: politas)
- [NetKAN] Add regexp second test for filespecs (#1919 by: politas; reviewed: ayan4m1)
- [Core] Changed name of registry lock file to registry.lock (#1944 by: politas; reviewed: ayan4m1)
- [GUI] Modlist hides epochs by default (#1942 by: politas; reviewed: ayan4m1)
- [Core/GUI] Let users select compatible KSP versions (#1957 by: grzegrzk; reviewed: dbent, politas)
- [Core] Add IntersectWith method to KspVersionRange (#1958 by: dbent; reviewed: grzegrzk, politas)
- [GUI] Display all mod versions in ModInfo panel (#1961 by: grzegrzk; reviewed: politas)
- [GUI] Use OpenFileDialog instead of FolderBrowserDialog in instance selector (#1939 by: ayan4m1; reviewed: politas)

## v1.20.1 (Alexey)

### Bugfixes

- [GUI] Reduce MinimumSize for main window to 1280x700 (#1893 by: politas; reviewed: Postremus, techman83, ayan4m1, Olympic1)

### Features

- [Multiple] Add Curse to resources (#1897 by: Olympic1; reviewed: ayan4m1, politas)
- [Core] Use maximum of buildID and buildID64 if both are available (#1900 by: dbent; reviewed: politas)

## v1.20.0 (Yuri)

### Bugfixes

- [GUI/CLI] Replace /n with /r/n in text messages, replaced CKAN-meta/issues links (#1846 by: DinCahill; reviewed: politas)
- [GUI] Fix FIPS-mode exceptions on Domain-connected Windows PCs (#1845 by: politas, #1850 by: ayan4m1; reviewed: politas)
- [All] Refactoring variables per project guidelines, Add Report Issue link in Help Menu, make SafeAdd safer, cleanup some Doco wording (#1849 by: ayan4m1; reviewed: politas)
- [GUI] Resize KSP Version Label to keep in window (#1837 by: Telanor, #1854 by: politas; reviewed: ayan4m1)
- [GUI] Fix GUI exceptions when installing/uninstalling after sorting by KSP max version (#1882, #1887 by: ayan4m1; reviewed: politas)
- [Core] Fix Windows-only NullReferenceException and add more ModuleInstaller tests (#1880 by: ayan4m1; reviewed: politas)

### Features

- [GUI] Add Back and Forward buttons to the GUI to jump to selected mods (#1841 by: b-w; reviewed: politas)
- [NetKAN] GitHub Transformer now extracts the repository name and transforms it to a usable ckan name (#1613 by: Olympic1; reviewed: pjf)
- [GUI] Don't show suggested/recommended for mods that can't be installed (#1427 by: Postremus; reviewed: politas)
- [Core] Remove empty directories when uninstalling mods (#1873 by: politas; reviewed: ayan4m1)
- [Core] Users are less able to run two copies of CKAN at the same time. (#1265 by: mgsdk, #1357 by: pjf, #1828 by: politas; reviewed: ayan4m1)
- [NetKAN] Add Curse as a $kref source (#1608 by: airminer, Olympic1; reviewed: dbent, pjf, techman83, ayan4m1)
- [All] Relationship changes now prompt reinstalling (#1730 by: dbent, #1885 by: ayan4m1; reviewed: plague006, pjf)
- [GUI] Add "X" icon to filter text boxes that clears the control (#1883 by: ayan4m1; reviewed: politas)

## v1.18.1 (StarTrammier)

### Bugfixes

- [CLI] Improve legend on `ckan list` functionality. (#1664 by: politas; reviewed: pjf)
- [Core] Workaround string.Format() bug in old Mono versions (#1784 by: dbent, reviewed: postremus)

### Features

- [CLI] `ckan.exe ksp list` now prints its output as a table and includes the version of the installation and its default status. (#1656 by: dbent; reviewed: pjf)
- [GUI] Auto-updater shows download progress (#1692 by: trakos, #1359 by: Postremus; reviewed: politas)
- [GUI] About dialog pointing to new CKAN thread on forums (#1824 by: politas; reviewed: -)
- [GUI] Ignore ksp instances if we are launching the gui anyway (#1809 by: Postremus; reviewed: politas)

### Internal

- [Build] Travis/Build: Prevent mozroots from asking for human input (#1825 by: pjf; reviewed: politas)

## v1.18.0 (StarTram)

### Bugfixes

- [Core] In certain cases a `NullReferenceException` could be produced inside error handling code when processing the registry. (#1700 by: keyspace, reviewed: dbent)
- [GUI] Fix typo in export options. (#1718 by: dandrestor, reviewed: plague006)
- [GUI] Fix unit of measure for download speed. (#1732 by: plague006, reviewed: dbent)
- [Linux] Better menu integration of the CKAN launcher. (#1704 by: reavertm; reviewed: pjf)

### Features

- [Core] `install` stanzas can have an `as` property allowing directories and files to be renamed/moved on installation. (#1728 by: dbent; reviewed: techman83)
- [GUI] Added "filter by description" search box. (#1632 by: politas; reviewed: pjf)
- [CLI] `compare` command now checks positive and negative rather than -1/+1 (#1649 by: dbent; reviewed: Daz)
- [GUI] In windows launch `KSP_x64.exe` by default rather than `KSP.exe`. (#1711 by: plague006; reviewed: dbent)
- [Core] Unlicense added to CKAN as an option for mods. (#1737 by: plague006; reviewed: techman83)
- [Core] CKAN will now read BuildID.txt for more accurate KSP versions (#1645 by: dbent; reviewed: techman83)

### Internal

- [Multiple] Removed various references and code for processing mods on KerbalStuff. Thank you, Sircmpwn, for providing us with such a great service for so long. (#1615 by: Olympic1; reviewed: pjf)
- [Spec] Updated Spec with the `kind` field which was introduced in v1.6. (#1662, #1597 by: plague006; reviewed: Daz)
- [Spec] ckan.schema now enforces structure of install directives (#1578 by: Zane6888; reviewed: pjf, Daz)
- [Spec] Documented the `x_netkan_github` and `use_source_archive` options in NetKAN files. (#1774 by: dbent; reviewed: plague006)
- [Spec] Clarified the `install_to` directive. (#1771 by: politas; reviewed: plague006)
- [Spec] Clarified example of a complete metanetkan file (#1753 by: plague006; reviewed: politas)
- [Spec] Removed stray comma (#1736 by: plague006; reviewed: politas)
- [NetKAN] Catch ValueErrors rather than printing the trace (#1648 by: techman83; reviewed: Daz)
- [NetKAN] Catch `ksp_version` from SpaceDocks newly implemented `game_version` (#1655 by: dbent; reviewed: -)
- [NetKAN] Allow specifying when an override is executed (#1684 by: dbent; reviewed: techman83)
- [NetKAN] Redirects to the download file are now resolved when using HTTP $krefs (#1696 by: dbent, reviewed: techman83)
- [NetKAN] Remote AVC files will be used in preference to ones stored in the archive if they have the same version (#1701 by: dbent, reviewed: techman83)
- [NetKAN] Sensible defaults are used when fetching abstract and homepage from github. (#1726, #1723 by: dbent; reviewed: politas)
- [NetKAN] Add Download Attribute Transformer (#1710 by: techman83; reviewed: dbent)
- [NetKAN] Add ksp_version_strict to property sort order (#1722 by: dbent; reviewed: plague006)
- [Docs] Updated `CONTRIBUTING.md` and `README.md` documentation. (#1748 by: plague006; reviewed: politas)
- [Build] Support for mono 3.2.8 deprecated (#1715 by: dbent; reviewed: techman83)
- [Build] Added support for building the CKAN client into a docker container. (#1747 by: mathuin; reviewed: pjf)
- [Build] Continuous integration is less susceptible to third-party network errors. (#1782 by: pjf; reviewed: techman83)
- [Core] Defend against corrupted KSP version numbers in old registries. (#1781 by: pjf; reviewed: politas)
- [Core] Support for upcoming download hash functionality in client. (#1752 by: plague006; reviewed: pjf)
- [GUI] Fixed spurious build warning (#1776 by: politas; reviewed: pjf)

## v1.16.1 (Plasma Window)

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
- [NetKAN] `netkan.exe` will now add all authors listed on SpaceDock (#1600, #1620 by: dbent; reviewed: techman83)
- [Core] Spelling mistake in documentation fixed (#1623 by: Dazpoet; reviewed: pjf)
- [Reporting] Creation of an issues template to help with bug reporting. (#1596, #1598 by: plague006, Shuudoushi; reviewed: Dazpoet, Olympic1)

## v1.16.0 (Ringshne)

### Bugfixes

- [GUI] CKAN handlers added to `mimeapps.list` in a more cross-platform friendly fashion. (danielrschmidt, #1536)

### Features

- [Core] Better detection of KSP installs in non-standard Steam locations (LarsOL, #1444; pjf, #1481)
- [Core] `find` and `find_regexp` install directives will match files as well as directories if the `find_matches_files` field is set to `true`. (dbent, #1241)
- [Core/GUI] Missing directories in `Ships` will be recreated as needed. (Wetmelon, #1525)
- [Core] Framework added to allow fuzzy version checking, including "you're on your own" comparisons where KSP version checks are disabled. Updated spec to include `ksp_version_strict`, which enforces strict versioning. (pjf, #1499)
- [Core] Thumbs subdirectories in `Ships` can now be directly targeted by install stanzas. (Postremus, #1448)

### Internal

- [NetKAN] `netkan.exe` will now sort `conflicts` relationships next to other relationships. (dbent, #1496)
- [NetKAN] `netkan.exe` now has much better support for Jenkins CI servers, allowing full automation. (dbent, #1512)

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

- [Updater] Checking for updates takes less network resources, and is more resilient to malformed release notes. (Postremus, #1410; pjf, #1453)
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
