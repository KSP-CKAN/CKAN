# Change Log

All notable changes to this project will be documented in this file.

## v1.34.5

### Features

- [Updater] Support dev builds for auto updater (#3997, #4008, #4059 by: HebaruSan)
- [GUI] Sort mods satisfying the same recommendation by download count (#4007 by: HebaruSan)
- [Multiple] Alternate game command lines and Steam refactor (#4010, #4013, #4028 by: HebaruSan)
- [Multiple] Recommendations usability improvements (#4025 by: HebaruSan)
- [Multiple] Prompt for client upgrade when newer spec is found (#4026, #4057 by: HebaruSan)
- [GUI] Ability to clear auto-installed flag from changeset tab (#4033, #4143 by: HebaruSan)
- [Multiple] New Crowdin updates (#4019 by: Olympic1, vinix38; reviewed: HebaruSan)
- [Core] Support Windows KSP1 instances on Linux (#4044 by: HebaruSan)
- [GUI] I18n updates from Crowdin (#4050 by: HebaruSan)
- [Multiple] Better version specific relationships at install and upgrade (#4023, #4152 by: HebaruSan)
- [GUI] Proportional, granular progress updates for installing (#4055 by: HebaruSan)
- [GUI] Modpack compatibility prompt, GameComparator clean-up (#4056 by: HebaruSan)
- [ConsoleUI] Add downloads column for ConsoleUI (#4063 by: HebaruSan)
- [ConsoleUI] Play game option for ConsoleUI (#4064 by: HebaruSan)
- [ConsoleUI] ConsoleUI prompt to delete non-empty folders after uninstall (#4066 by: HebaruSan)
- [Multiple] Treat mods with missing files as upgradeable/reinstallable (#4067 by: HebaruSan)
- [ConsoleUI] Conflicting recommendations check for ConsoleUI (#4085 by: HebaruSan)
- [Build] Linux: Improve desktop entries (#4092 by: mmvanheusden; reviewed: HebaruSan)
- [ConsoleUI] Install from .ckan file option for ConsoleUI (#4103 by: HebaruSan)
- [Build] Support icons from libraries for deb and rpm (#4104 by: HebaruSan)
- [Multiple] Store GitHub Discussions links and display in UIs (#4111 by: HebaruSan)
- [GUI] Chinese translation fixes (#4115, #4116, #4131 by: zhouyiqing0304; reviewed: HebaruSan)
- [Multiple] Visually indicate to users that they should click Refresh (#4133 by: HebaruSan)
- [Multiple] Option to clone smaller instances with junction points (Windows) or symbolic links (Unix) (#4129, #4144 by: HebaruSan)
- [Multiple] Recommendation suppression for modpacks (#4147 by: HebaruSan; reviewed: JonnyOThan)
- [GUI] Search by licenses (#4148 by: HebaruSan)
- [CLI] Make `ckan compat add` take multiple versions, add `clear` and `set` (#4151 by: HebaruSan)
- [GUI] Add mod support links to Help menu (#4154 by: HebaruSan)

### Bugfixes

- [GUI] Suppress admin user check for URL handler registration (#3996 by: HebaruSan)
- [GUI] Refactor Contents tab refreshing (#4001 by: HebaruSan)
- [Core] Fix crash with DLC disabled by Steam (#4002 by: HebaruSan)
- [Multiple] Fixes for installing .ckan files and DarkKAN mods (#4006 by: HebaruSan)
- [Core] Only auto-delete manually installed DLLs if also replacing them (#4024 by: HebaruSan)
- [Multiple] Show repo ETag and parsing errors in Failed Downloads (#4030 by: HebaruSan)
- [Multiple] Properly clear AD upgrades from changeset (#4037 by: HebaruSan)
- [Multiple] De-over-parallelize Versions tab (#4049 by: HebaruSan)
- [GUI] Use better console hiding API (#4051 by: HebaruSan)
- [Core] Trigger progress updates for frozen downloads (#4052 by: HebaruSan)
- [GUI] Fix NRE on trying to update all when there's nothing to update (#4054 by: HebaruSan)
- [Core] Fix NRE in repo update with corrupted etags.json file (#4077, #4078 by: HebaruSan)
- [Core] Skip temp files for repo updates (#4102 by: HebaruSan)
- [Core] Fix default transaction timeout (#4119 by: romi2002; reviewed: HebaruSan)
- [Core] Use DriveInfo constructor to get drive from path (#4125 by: HebaruSan)
- [Core] Detect changes to `replaced_by` (#4127 by: HebaruSan)
- [Multiple] Update changeset on Replace checkbox change, other `replaced_by` fixes (#4128 by: HebaruSan)
- [Multiple] Stop checking multiple hashes (#4135 by: HebaruSan)
- [Core] Fix max version column for wildcard compat (#4142 by: HebaruSan)
- [Multiple] Refactor ZIP importing (#4153 by: HebaruSan)

### Internal

- [Policy] Fix #3518 rewrite de-indexing policy (#3993 by: JonnyOThan; reviewed: HebaruSan)
- [Netkan] Fix null reference exception in swinfo transformer (#3999 by: HebaruSan)
- [Netkan] Improve netkan relationship error message (#4020, #4021 by: HebaruSan)
- [Core] Get KSP2 version from game assembly (#4034 by: HebaruSan)
- [Multiple] Build nuget package, support netstandard2.0 build (#4039 by: HebaruSan)
- [Core] Use fully sanitized archive.org bucket names (#4043 by: HebaruSan)
- [Netkan] Omit duplicate inflation warnings in queue (#4071 by: HebaruSan)
- [Build] Refactor + Modernise Actions (#4082, #4088, #4089, #4091, #4093, #4094, #4095, #4117 by: techman83, HebaruSan; reviewed: HebaruSan)
- [Multiple] Translation updates from Crowdin (#4105, #4149, #4157 by: vinix38, frankieorabona, ambition, Francesco Ricina, S.O.2; reviewed: HebaruSan)
- [Netkan] Allow string "Harmony" in DLL parent folder names (#4123 by: HebaruSan)
- [Netkan] Allow licenses to be absent from netkans (#4137 by: HebaruSan)
- [Netkan] Autogenerate `spec_version` and make it optional in netkans (#4155 by: HebaruSan)
- [Infra] Trigger mod installer deploy after APT repo update (#4158 by: HebaruSan)
- [CLI] Ability to update repos without a game instance (#4161 by: HebaruSan)

## v1.34.4 (Niven)

### Features

- [CLI] Pause after administrator error message (#3966 by: HebaruSan; reviewed: techman83)
- [Multiple] Put auto-installed mods in ignored modpack group by default (#3978 by: HebaruSan)
- [GUI] Add links to hidden tags and labels below mod list (#3979, #3980 by: HebaruSan)
- [Core] Detect and use Windows KSP2 executable on Linux (#3984 by: JackOfHertz; reviewed: HebaruSan)

### Bugfixes

- [Core] Oops, `HttpClient` actually sucks (#3960 by: HebaruSan; reviewed: techman83)
- [Multiple] Usability improvements for adding game instance (#3964 by: HebaruSan; reviewed: JonnyOThan)
- [Core] Fix NullReferenceException in csv/tsv export (#3967 by: HebaruSan)
- [Core] Fix cache timestamp comparisons (#3974 by: HebaruSan)
- [GUI] Fix compatible popup messing with max game version column (#3976 by: HebaruSan)
- [Core] Fix Audit Recommendations option (#3988 by: HebaruSan)
- [Multiple] Installation message usability improvements (#3989 by: HebaruSan)

### Internal

- [Netkan] Re-sort module properties after merging (#3957 by: HebaruSan)
- [Netkan] Update and optimize meta tester and inflator images (#3958, #3968 by: HebaruSan; reviewed: techman83)
- [Netkan] Generate schema compliant game versions from swinfo.json (#3969 by: HebaruSan)
- [Core] Save and load game version for fake KSP2 instances (#3986 by: HebaruSan)
- [Netkan] Resolve compatibility conflicts after multi-host merge (#3991 by: HebaruSan)

## v1.34.2 (Minkowski²)

### Bugfixes

- [GUI] Protect upgradeable mods from being displayed as uninstalled (#3944 by: HebaruSan)
- [GUI] Restore conflict highlights in changeset (#3948 by: HebaruSan)
- [GUI] Conflict highlight for selected row (#3951 by: HebaruSan)
- [Core] Fix uninstallation of manually deleted files and directories (#3955 by: HebaruSan)

## v1.34.0 (Minkowski)

### Features

- [Multiple] Support multiple download URLs per module (#3877 by: HebaruSan; reviewed: techman83)
- [Multiple] French translation updates from Crowdin (#3879 by: vinix38; reviewed: HebaruSan)
- [Multiple] Improve handling of unregistered files at uninstall (#3890, #3942 by: HebaruSan; reviewed: techman83)
- [Multiple] Show recommendations of full changeset with opt-out (#3892 by: HebaruSan; reviewed: techman83)
- [Multiple] Dutch translation and icon duplication guardrails (#3897 by: HebaruSan; reviewed: techman83)
- [GUI] Shorten toolbar button labels (#3903 by: HebaruSan; reviewed: techman83)
- [Multiple] Refactor repository and available module handling (#3904, #3907, #3908, #3935, #3937 by: HebaruSan; reviewed: techman83)
- [Multiple] Parallelize for performance, relationship resolver improvements (#3917 by: HebaruSan; reviewed: techman83)
- [Multiple] Modernize administrator and Mono version checks (#3933 by: HebaruSan; reviewed: techman83)
- [Multiple] Improve file deletion error while the game is running (#3938 by: HebaruSan; reviewed: techman83)
- [GUI] New Crowdin updates (#3940 by: Olympic1; reviewed: HebaruSan)
- [GUI] Search by supports relationship and other search improvements (#3939 by: HebaruSan)

### Bugfixes

- [GUI] Updated Chinese translation to reduce misunderstandings (#3864 by: Fierce-Cat; reviewed: HebaruSan)
- [Multiple] Translation updates from Crowdin (#3866, #3868 by: Nikita, Вячеслав Бучин, vinix38, WujekFoliarz; reviewed: HebaruSan)
- [Multiple] Fix deletion of unmanaged files (#3865 by: HebaruSan; reviewed: techman83)
- [Build] Add missing dependency to .deb package (#3872 by: HebaruSan; reviewed: erkinalp)
- [Core] Add missing resource string for upgrading (#3873 by: HebaruSan; reviewed: techman83)
- [Multiple] Repository management fixes (#3876 by: HebaruSan; reviewed: techman83)
- [GUI] Restore window position without default instance (#3878 by: HebaruSan; reviewed: techman83)
- [CLI] Correctly print cmdline errors with braces (#3880 by: HebaruSan; reviewed: techman83)
- [Multiple] Caching and changeset fixes (#3881 by: HebaruSan; reviewed: techman83)
- [GUI] Mod list fixes and improvements (#3883 by: HebaruSan; reviewed: techman83)
- [Multiple] Multi-game labels (#3885 by: HebaruSan; reviewed: techman83)
- [Multiple] Alternate mod dirs for validation and manual installs (#3891 by: HebaruSan; reviewed: techman83)
- [Core] Fix archive.org fallback URLs for versions with spaces (#3899 by: HebaruSan)
- [Multiple] Fix auto-remove during upgrade (#3913 by: HebaruSan; reviewed: techman83)
- [Build] Clean up Linux .desktop files (#3927 by: irasponsible; reviewed: HebaruSan)
- [GUI] Don't change language setting with scroll wheel (#3928 by: HebaruSan; reviewed: techman83)
- [Core] Make where-CKAN-would-install logic consistent (#3931 by: HebaruSan; reviewed: techman83)
- [Multiple] Improve provides prompt usability, with bugfixes (#3934 by: HebaruSan; reviewed: JonnyOThan)
- [GUI] Fix two small typos in Resources.pt-BR.resx at line 182 (#3943 by: idrkwhattoput; reviewed: HebaruSan)
- [Core] Handle backslashes in ZIP paths (#3893 by: HebaruSan; reviewed: techman83)

### Internal

- [Netkan] Fix Netkan swinfo transformer null list error (#3869 by: HebaruSan)
- [Tooling] Deduce primary branch name in merge script (#3884 by: HebaruSan; reviewed: techman83)
- [CLI] Parse quoted strings for `ckan prompt` (#3889 by: HebaruSan; reviewed: techman83)
- [Build] Remove log4net, newtonsoft deps from deb package (#3900 by: HebaruSan; reviewed: techman83)
- [GUI] Add test to check GUI thread safety (#3914 by: HebaruSan; reviewed: techman83)
- [Multiple] VSCode clean-up and other minor fixes (#3920 by: HebaruSan)
- [Build] Modernize build system and .NET platform targets (#3929 by: HebaruSan; reviewed: techman83)
- [Spec] Add a note on policy on relationship metadata (#3930 by: JonnyOThan; reviewed: HebaruSan)
- [Build] dotnet build with multi-targeting (#3932 by: HebaruSan; reviewed: techman83)

## v1.33.2 (Laplace)

### Bugfixes

- [GUI] Fix exception at startup w/o default game inst (#3863 by: HebaruSan)

## v1.33.0 (Lagrange)

### Features

- [GUI] Allow GUI users to delete registry lockfiles (#3829, #3841 by: HebaruSan; reviewed: techman83)
- [GUI] Show unmanaged files in game folder (#3833 by: HebaruSan; reviewed: techman83)
- [GUI] Installation history tab (#3834 by: HebaruSan; reviewed: techman83)
- [GUI] Hide fake instance creation in GUI (#3839 by: HebaruSan; reviewed: techman83)
- [GUI] Tooltip for auto-installed checkboxes (#3842 by: HebaruSan; reviewed: techman83)
- [Core] Default `install_to` to GameData/Mods for KSP2 (#3861 by: HebaruSan; reviewed: techman83)

### Bugfixes

- [GUI] Fix NRE on purging cache in GUI (#3810 by: HebaruSan; reviewed: techman83)
- [GUI] Only update Versions tab when the mod changes (#3822 by: HebaruSan; reviewed: techman83)
- [Multiple] Treat reinstalling a changed module as an update (#3828 by: HebaruSan; reviewed: techman83)
- [Core] Scan for DLLs with or without primary mod dir (#3837 by: HebaruSan; reviewed: techman83)
- [GUI] Show download errors for upgrades (#3840 by: HebaruSan; reviewed: techman83)
- [Core] Stop trying to check free space on Mono (#3850 by: HebaruSan; reviewed: techman83)
- [Core] Handle missing KSP2 exe (#3854 by: HebaruSan; reviewed: techman83)
- [Core] Linux network fixes (#3859 by: HebaruSan; reviewed: techman83)
- [Core] Include repo etags in transactions (#3860 by: HebaruSan; reviewed: techman83)

### Internal

- [Netkan] Warnings for missing swinfo.json deps (#3827, #3858 by: HebaruSan)
- [Build] Stop building on Mono 6.6 and earlier (#3832 by: HebaruSan)
- [Netkan] Add download link to staging PRs (#3831 by: HebaruSan)
- [Netkan] Clarify remote swinfo network errors (#3843 by: HebaruSan; reviewed: )

## v1.32.0 (Kepler)

### Features

- [GUI] Red highlight for dependencies on missing DLC (#3698 by: HebaruSan; reviewed: techman83)
- [GUI] Clarify mod list saving options, add menu hotkeys (#3771 by: HebaruSan; reviewed: techman83)
- [Multiple] Italian translation and other localization fixes (#3780, #3781 by: frankieorabona, WujekFoliarz, vinix38, Kalessin1; reviewed: HebaruSan)
- [Core] Fix mkbundle executable crash (#3767 by: memchr; reviewed: HebaruSan)
- [GUI] Show conflicts in changeset (#3727 by: HebaruSan; reviewed: techman83)
- [Multiple] KSP2 support (#3797, #3808, #3811, #3817 by: HebaruSan; reviewed: techman83)

### Bugfixes

- [GUI] Remove duplicate Install changes for upgrades (#3706 by: HebaruSan; reviewed: techman83)
- [GUI] Fix GUI freeze with non-empty changeset at startup (#3708 by: HebaruSan; reviewed: techman83)
- [GUI] Use changeset tab for reinstall (#3726, #3728, #3739 by: HebaruSan; reviewed: techman83)
- [Core] Fix handling of empty builds.json file (#3733 by: HebaruSan; reviewed: DasSkelett)
- [Core] Fix FIPS-mode exceptions on Windows for SHA256 (#3774 by: HebaruSan; reviewed: techman83)
- [Core] Support cancellation of download checksums (#3778 by: HebaruSan; reviewed: techman83)
- [Core] Skip duplicate repo URLs during update (#3786 by: HebaruSan; reviewed: techman83)
- [Core] Mark new deps of upgrades as auto-installed (#3702 by: HebaruSan; reviewed: techman83)
- [GUI] Fix index -1 exception in Manage Instances (#3800 by: HebaruSan; reviewed: techman83)
- [Multiple] Cache path setting fixes (#3804 by: HebaruSan; reviewed: techman83)
- [GUI] Make grid ampersand workaround platform specific (#3807 by: HebaruSan; reviewed: techman83)

### Internal

- [Tooling] Switch workflows from set-output to `$GITHUB_OUTPUT` (#3696 by: HebaruSan)
- [Netkan] Fix Netkan check for Ships/Script `spec_version` (#3713 by: HebaruSan; reviewed: techman83)
- [Netkan] Netkan warning when `include_only` doesn't match (#3805 by: HebaruSan; reviewed: techman83)

## v1.31.2 (Juno)

### Features

- [GUI] Korean translation of GUI (#3606 by: Kingnoob1377; reviewed: HebaruSan)
- [CLI] Tab completion for `ckan prompt` (#3515, #3617 by: HebaruSan; reviewed: techman83)
- [GUI] Context sensitive help (#3563 by: HebaruSan; reviewed: techman83)
- [Multiple] Add install size to metadata and display in clients (#3568 by: HebaruSan; reviewed: techman83)
- [CLI] Create a system menu entry for command prompt (#3622 by: HebaruSan; reviewed: techman83)
- [Multiple] Internationalize Core, CmdLine, ConsoleUI, and AutoUpdater (#3482 by: HebaruSan; reviewed: techman83)
- [Multiple] Check free space before downloading (#3631 by: HebaruSan; reviewed: techman83)
- [Multiple] Many improvements for failed downloads (#3635, #3637, #3642 by: HebaruSan; reviewed: techman83)
- [GUI] Show reverse relationships (#3638, #3649 by: HebaruSan; reviewed: techman83)
- [GUI] Debounce search events on all platforms (#3641, #3656 by: HebaruSan; reviewed: techman83, DasSkelett)
- [Multiple] Improvements for failed repo updates (#3645 by: HebaruSan; reviewed: techman83)
- [GUI] Highlight incompatible mods recursively (#3651 by: HebaruSan; reviewed: techman83)
- [GUI] Support mouse back/forward buttons (#3655 by: HebaruSan; reviewed: techman83)
- [Core] Resume failed downloads (#3666 by: HebaruSan; reviewed: techman83)
- [Multiple] Install dependencies first (#3667, #3675 by: HebaruSan; reviewed: techman83)
- [Multiple] Polish translation (#3669 by: WujekFoliarz; reviewed: HebaruSan)
- [GUI] ModInfo usability improvements (#3670 by: HebaruSan; reviewed: techman83)
- [Multiple] Report progress in validating downloads (#3659 by: HebaruSan; reviewed: techman83)
- [GUI] Authors as clickable filter links, combine search links w/ ctrl or shift (#3672 by: HebaruSan; reviewed: techman83)
- [GUI] New Crowdin updates (#3653, #3695 by: Olympic1; reviewed: HebaruSan)
- [Multiple] Install incompatible modpack dependencies with confirmation (#3675 by: HebaruSan; reviewed: techman83)

### Bugfixes

- [GUI] Auto-size buttons in bottom panels (#3576 by: HebaruSan; reviewed: techman83)
- [GUI] Hide tray icon at exit via ctrl-C (#3639 by: HebaruSan; reviewed: techman83)
- [Multiple] Fix auto removal when installing w/ deps (#3643, #3660 by: HebaruSan; reviewed: techman83)
- [Core] Only show DLL location error if installing DLL (#3647 by: HebaruSan; reviewed: techman83)
- [Core] Fix stale lockfile detection (#3687 by: HebaruSan; reviewed: techman83)

### Internal

- [Build] Generate RPM repo for releases and under dev builds (#3605, #3609, #3610 by: HebaruSan)
- [Netkan] Internal .ckan file compatibility with bundled mods (#3615 by: HebaruSan)
- [Netkan] Filter duplicate co-authors from SpaceDock (#3599 by: HebaruSan; reviewed: techman83)
- [Netkan] Log errors instead of PRs for OOO mods on GitHub (#3625 by: HebaruSan)
- [Core] Cache remote build map, fetch in registry refresh (#3624 by: HebaruSan; reviewed: techman83)
- [Tooling] Self review option for merge script (#3650 by: HebaruSan; reviewed: DasSkelett)
- [Infra] Improve translation process (#3648 by: Olympic1; reviewed: HebaruSan)
- [Netkan] Inflation error for `version_min` and `version_max` (#3657 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] GitLab kref (#3661 by: HebaruSan; reviewed: techman83)
- [Netkan] Make Netkan nested GameData check case insensitive (#3682 by: HebaruSan)
- [Docs] Replace apt with dnf in the docs for the RPM repo (#3684 by: htmlcsjs; reviewed: HebaruSan)

## v1.31.0 (IKAROS)

### Features

- [GUI] ru-RU translation (#3383, #3443 by: nt0g; reviewed: HebaruSan)
- [GUI] Japanese Localization (#3394 by: utah239; reviewed: HebaruSan)
- [Multiple] Match underscore in DLL to dash in identifier (#3412 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Add versions table option to ckan show command (#3414 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Override provides prompt with relationship property, check first recommendation in any_of group (#3426, #3436 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Add user guide and Discord to GUI help menu (#3437 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Label ordering buttons (#3416 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Suppress incompatibility warning at game launch (#3453 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Options for ckan show to hide sections (#3461 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Show context menu on menu key press (#3446 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Theme all buttons, checkboxes, groupboxes, and listboxes (#3489 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Add Release Date column to GUI modinfo versions list (#3481 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Global install filters (#3458 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Negated search for name, desc, author, lang, relationships, tags (#3460 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Play Time (#3543 by: PizzaRules668; reviewed: HebaruSan)

### Bugfixes

- [Multiple] Clarify that downloading to cache does not install (#3400 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't include DLC in modpacks by default (#3417 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Set GNOME single window property (#3425 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Default install stanza multi-game support, catch missing install_to (#3441 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Escape ampersands in mod info abstract label (#3429 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't prompt user to choose conflicting modules (#3434 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't prompt to overwrite dirs (#3464 by: HebaruSan; reviewed: DasSkelett)
- [ConsoleUI] Keep dependencies in the box in ConsoleUI (#3459 by: HebaruSan; reviewed: DasSkelett)
- [GUI] French translation bits in label dialog (#3465 by: vinix38; reviewed: HebaruSan)
- [GUI] Suppress filter updates for unchanged semantic search meaning (#3435 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Use CRLF for resx files (#3471 by: HebaruSan; reviewed: DasSkelett)
- [Core] Case insensitive installed file lookup on Windows (#3479 by: HebaruSan; reviewed: DasSkelett)
- [Core] Properly determine the game when cloning instances (#3478 by: DasSkelett; reviewed: HebaruSan)
- [ConsoleUI] Rewrap ConsoleUI textbox for scrollbar and resize (#3514 by: HebaruSan; reviewed: DasSkelett)
- [Core] Sort exported modpack relationships by identifier (#3499 by: HebaruSan; reviewed: DasSkelett)
- [Core] Disable tx timeouts, add tx debug logging, static DLL pattern (#3512 by: HebaruSan; reviewed: DasSkelett)
- [Core] Only delete diversely capitalized directories once on Windows (#3528 by: HebaruSan; reviewed: DasSkelett)
- [Core] Get licenses from embedded schema, skip bad modules in deserialize (#3526 by: HebaruSan; reviewed: DasSkelett)
- [Core] One concurrent download per host for all hosts (#3557 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Show dependencies of upgrading mods in change set (#3560 by: HebaruSan; reviewed: DasSkelett)
- [Core] Resolve virtual module dependencies in same order as non-virtual (#3476 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix play time column if no playtime, hide game column if all instances are of the same game (#3570 by: DasSkelett; reviewed: HebaruSan)
- [Core] Fix tracking of paths with trailing spaces on Windows (#3586 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Multiple] Cache permanent redirects (#3389 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Allow YAML for human-edited metadata (YAMLKAN) (#3367 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Fill more info from GitHub for SpaceDock mods (#3390 by: HebaruSan; reviewed: DasSkelett)
- [Spec] YAMLize netkan spec (#3438 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Append resource links to staging PRs (#3454 by: HebaruSan; reviewed: techman83)
- [Infra] Remove expired Let's Encrypt root certificate from Mono containers (#3457 by: DasSkelett; reviewed: HebaruSan)
- [Infra] Install libffi-dev to fix xKAN-meta_testing Docker image build (#3463 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Fix double-absolute SpaceDock URLs (#3466 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Set bot useragent for Inflator (#3490 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Sort Netkan warning lists (#3492 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Enforce spec version requirements for more install properties (#3494 by: HebaruSan; reviewed: techman83)
- [Build] Rename GH1866 test, fix invalid char test, fix equality assertion order (#3509 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Enforce a few more spec version requirements (#3505 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Allow overriding resources.remote-avc (#3451 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Sort GitHub releases (#3571 by: HebaruSan; reviewed: DasSkelett)

## v1.30.4 (Hubble)

### Features

- [ConsoleUI] Make current instance settings easier to find in ConsoleUI (#3385 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Show game instance selection dialog if default is locked (#3382 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [GUI] Invoke GUI update calls in SetupDefaultSearch() descendants (#3380 by: DasSkelett; reviewed: HebaruSan)

### Internal

- [Netkan] Set bugtracker resource for SpaceDock mods with GitHub repos (#3384 by: HebaruSan; reviewed: DasSkelett)
- [Core] Pass token for moved files on GitHub (#3387 by: HebaruSan; reviewed: DasSkelett)

## v1.30.2 (Hawking)

### Features

- [GUI] Add pt-BR localization (#3340 by: gsantos9489; reviewed: DasSkelett, HebaruSan)
- [GUI] Multiple search boxes with OR logic in GUI (#3323, #3374 by: HebaruSan; reviewed: Olympic1, DasSkelett)

### Bugfixes

- [GUI] Update dialog was too small for French localization (#3313 by: vinix38; reviewed: HebaruSan)
- [Build] Suppress nightly debs with even builds, push release debs to s3 first (#3317 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Handle renamed modlist columns in sort list gracefully (#3319 by: DasSkelett; reviewed: HebaruSan)
- [Core] Fix AD mod upgrading, add tests, and fix all warnings (#3315 by: HebaruSan; reviewed: DasSkelett)
- [Core] Reset cache dir to default if creation fails (#3334 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Tell user if instance addition fails (#3332 by: HebaruSan; reviewed: DasSkelett)
- [Core] Tell SharpZipLib to use Unicode when opening zips (#3345 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Make incompatible mods warning dialog use newlines instead of commas (#3346 by: DeltaDizzy; reviewed: DasSkelett)
- [Core] Fix crash when overwriting manually installed files (#3349 by: HebaruSan; reviewed: DasSkelett)
- [Core] Skip modules with parse errors in deserialization (#3347 by: HebaruSan; reviewed: DasSkelett)
- [ConsoleUI] Update or refresh ConsoleUI mod list after repo or compat changes (#3353 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Replace repo-reinst with kraken, handle in UIs (#3344 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Make ModuleInstaller a non-singleton (#3356 by: HebaruSan; reviewed: DasSkelett)
- [Core] Better recovery when registry.json is corrupted (#3351 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Fix installation of metapackages on cmdline (#3362 by: DasSkelett; reviewed: HebaruSan)
- [Multiple] Fix installation of AVP while removing EVE default configs (#3366 by: DasSkelett; reviewed: HebaruSan)

### Internal

- [Netkan] Time out after 10 seconds on text downloads in NetKAN (#3325 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Netkan errors and warnings for Harmony bundlers (#3326 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Print path of unused version file (#3327, #3328 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Switch from outdated SharpZipLib.Patched to newer SharpZipLib (#3329 by: DasSkelett; reviewed: HebaruSan)
- [Tooling] Use git cmd to add CHANGELOG.md in merge script (#3350 by: DasSkelett; reviewed: HebaruSan)
- [Tooling] Merge script fix for Windows (#3354 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Warn if a module with plugins can't be auto-detected (#3370 by: HebaruSan; reviewed: DasSkelett)

## v1.30.0 (Glenn)

### Features

- [Multiple] Store remote version file URL in metadata resources (#3259 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Abstract out game-specific logic (#3223, #3308 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Allow label to pin installed mod version (#3220 by: HebaruSan; reviewed: DasSkelett)
- [GUI] language: fr-FR (#3272, #3285 by: vinix38; reviewed: HebaruSan)
- [ConsoleUI] Dark theme for ConsoleUI (#3226 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Visual cues for incompatibility reasons (#3271 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Greeting for ckan prompt (#3300 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [GUI] Fix screen clamping logic (#3255 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Repo update usability fixes (#3249 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Added null checks in ManageMods.cs to prevent crash on an empty filter (#3266 by: Hydroxa; reviewed: HebaruSan)
- [Updater] Report AutoUpdater errors to user, fix rare failure (#3250 by: HebaruSan; reviewed: DasSkelett)
- [Core] Upgrade AD mods with mismatched version in filename (#3287 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Modpack usability fixes (#3243 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Fix crashes in audit recommendations (#3292 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix checkbox sorting (#3297 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Handle incompatible force-installed dependencies of recommendations (#3305 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Don't warn about incompatible DLCs, fix conflict highlighting with DLC installed (#3304 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Fixes for GUI and .ckan-installed modules (#3307 by: DasSkelett; reviewed: HebaruSan)

### Internal

- [GUI] Make Mono 6 builds work on Windows (#3218, #3219 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Format installation errors for GitHub Actions in headless mode (#3239 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Fix Netkan timezones again (#3246 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Better version overrides in Netkan (#3265 by: HebaruSan; reviewed: DasSkelett)
- [Tooling] Pull request merge script (#3263, #3276 by: HebaruSan; reviewed: DasSkelett)
- [Build] Upload ckan.exe artifact on pull requests (#3273 by: DasSkelett; reviewed: HebaruSan)
- [Build] Bump log4net from 2.0.8 to 2.0.10 (#3281 by: dependabot[bot]; reviewed: HebaruSan)
- [Netkan] Support indexing mulitple release assets on GitHub  (#3279 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Netkan warning for multiple assets (#3286 by: HebaruSan; reviewed: DasSkelett)

## v1.29.2 (Freedman)

### Features

- [Multiple] Allow upgrading manually installed modules (#3190 by: HebaruSan; reviewed: DasSkelett)
- [Build] Generate APT repo for releases and under dev builds (#3197, #3201, #3202, #3203, #3208, #3215 by: HebaruSan, techman83; reviewed: DasSkelett, HebaruSan)
- [CLI] Confirmation prompt for Cmdline upgrades (#3204 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Allow sort by multiple columns (#3205 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [GUI] Search UI fixes (#3198 by: HebaruSan; reviewed: DasSkelett)
- [Core] Fix error when removing file from GameRoot (#3196 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Streamline Mac onboarding (#3199 by: HebaruSan; reviewed: DasSkelett)
- [Core] Fix dependency resolution in mod upgrades (#3200 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Suppress confirmation prompt for GUI upgrades (#3206 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Manage repos with separate name and URL fields (#3214 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Build] Upgrade DLL build to .NET 5 (#3194 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Netkan warning for craft files installed outside Ships folder (#3207, #3213 by: HebaruSan; reviewed: DasSkelett)

## v1.29.0 (Eddington)

### Features

- [GUI] Search dropdown (#3175 by: HebaruSan; reviewed: DasSkelett)
- [Core] Allow installation into Ships/Script (#3180 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [Multiple] Purge CurlSharp (#3118 by: HebaruSan; reviewed: DasSkelett, techman83)
- [Core] Delete Authtoken when setting to null (#3119 by: Olympic1; reviewed: HebaruSan, DasSkelett)
- [Core] Better message when ZipFile throws NotSupportedException (#3128 by: HebaruSan; reviewed: DasSkelett)
- [Core] Fix missing DLC registration gap (#3136 by: HebaruSan; reviewed: DasSkelett)
- [Core] Satisfy dependencies with installed incompatible modules (#3137 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Extend -single-instance fix to 1.10 (#3153 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Escape '&' in mod list, fix path in GUIConfig.xml exception (#3149 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Print message for unknown install exception (#3164 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Search timer for Linux (#3167 by: HebaruSan; reviewed: DasSkelett)
- [ConsoleUI] ConsoleUI fixes for DLC (#3165 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Don't fail modpack export when having unindexed mods installed (#3139 by: DasSkelett, HebaruSan; reviewed: HebaruSan, DasSkelett)

### Internal

- [Build] Move to PackageReferences (#3125 by: Olympic1; reviewed: HebaruSan)
- [Build] Cache build tools (#3127 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Remove invalid zip files in NetKAN (#3156 by DasSkelett; reviewed: HebaruSan)
- [Build] Tests should cleanup its environment (#3132 by Olympic1; reviewed: HebaruSan)
- [Netkan] Retry failed web requests, fix install stanza error (#3166 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Delete netkan tmp files if cache fills up (#3169 by: HebaruSan; reviewed: techman83)

## v1.28.0 (Dyson)

### Bugfixes

- [Core] Fix reinstallation of updated incompatible modules (#3102 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't prompt to delete SCANsat's settings at upgrade (#3103 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Mark conflicts in initial recommendations (#3097 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Sort GUI KSP version column correctly (#3106 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Dependency/compatibility fixes (#3104 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Netkan] Add author to list of required properties in CKAN.schema (#3111 by: DasSkelett; reviewed: HebaruSan)
- [Build] Cleanup project, update builds, fixes (#3108 by: Olympic1; reviewed: HebaruSan, DasSkelett)

## v1.28.0-PRE1 (Drake)

### Features

- [Multiple] Show DLC in recommendations list (#3038 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Prompt user to overwrite manually installed files (#3043 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Master search bar and misc GUI clean-up (#3041 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Added Chinese translation of multiple search category fields. (#3053 by: hyx5020; reviewed: HebaruSan)
- [Build] Create a system menu entry for ConsoleUI (#3052 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Non-parallel GitHub downloads, one progress bar per download (#3054 by: HebaruSan; reviewed: DasSkelett)
- [Core] Multi-match 'find', allow 'as' for Ships and GameData (#3064 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Release date column (#3096 by: HebaruSan; reviewed: techman83)

### Bugfixes

- [CLI] Restore cmdline update message (#3042 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't try to install multiple versions of the same mod (#3051 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Unbreak reinstall right click (#3055 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Don't save registry when opening settings (#3058 by: DasSkelett; reviewed: HebaruSan)
- [Core] Move WebException stack trace from User error to verbose log (#3062 by: HebaruSan; reviewed: DasSkelett)
- [Core] Multi-find fixes (#3074 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix crash on marking all updates (#3079 by: HebaruSan; reviewed: DasSkelett)
- [Core] Use authToken for Curl (#3086 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Be more specific in GUI config parse error (#3090 by: HebaruSan, DasSkelett; reviewed: DasSkelett)

### Internal

- [Netkan] Purge stale cache entries for SpaceDock (#2859 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] NetKAN warnings (#3045 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Support fallback URLs for netkan validate-ckan (#3060 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Netkan warning for archived repos, set bugtracker for GitHub (#3061 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Check mixed case version files (#3065 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Netkan warning for tags (#3072 by: HebaruSan; reviewed: DasSkelett)
- [Core] Cache hashes on disk (#3080 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Don't fallback to Curl on 404, allow smaller GUI (#3084 by: HebaruSan; reviewed: DasSkelett)
- [Build] Move Travis CI to GitHub Workflows (#3085 by: DasSkelett; reviewed: techman83)
- [Build] Don't run workflow steps that require secrets on forks (#3089 by: DasSkelett; reviewed: HebaruSan)
- [Build] Run workflows in Mono Docker containers (#3091 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Generate release_date property in netkan (#3059 by: HebaruSan; reviewed: DasSkelett)

## v1.27.2 (Chandrasekhar)

### Features

- [Netkan] Create skip-releases option for NetKAN (#2996 by: DasSkelett; reviewed: HebaruSan)
- [Multiple] Check current dir for portable install (#3005 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Adding Simplified Chinese Localization (#3014 by: 050644zf; reviewed: HebaruSan)

### Bugfixes

- [GUI] Don't report an unknown error if it is known (#2995 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Extend -single-instance fix to 1.9 (#3001 by: DasSkelett; reviewed: HebaruSan)
- [Core] Check compatibility of providing modules (#3003 by: HebaruSan; reviewed: DasSkelett)
- [Core] Respect installing modules during dependency resolution (#3002 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Fix reinstall prompt crash (#3006 by: HebaruSan; reviewed: DasSkelett)
- [Core] Treat metapackage depends as not auto installed (#3008 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Improve modpack identifier validation (#3025 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Disable reinstall option for AD mods (#3034 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Consider supports in recommendations screen descriptions (#3028 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Netkan] Don't stage if compatible with current game version (#3031 by: HebaruSan; reviewed: DasSkelett)

## v1.27.0 (Bussard)

### Bugfixes

- [GUI] Fix null reference in recommendations (#2984 by: HebaruSan; reviewed: politas)
- [ConsoleUI] Fix NRE on download errors in ConsoleUI (#2987 by: HebaruSan; reviewed: politas, DasSkelett)
- [GUI] Only update provides tab from GUI thread (#2989 by: HebaruSan; reviewed: politas)
- [ConsoleUI] Fix ArgumentException in ConsoleUI recommendations (#2990 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Build] Update Cake to 0.37.0 (#2985 by: DasSkelett; reviewed: HebaruSan)
- [Spec] Update tags section of spec (#2991 by: HebaruSan; reviewed: DasSkelett)

## v1.26.10 (Alcubierre)

### Features

- [Multiple] Detect conflicts on recommendations screen (#2981 by: HebaruSan; reviewed: politas)

### Bugfixes

- [Core] Filter compatible modules by compatibility (#2980 by: HebaruSan; reviewed: politas)

### Internal

- [Build] Downgrade to building on Mono 5.20 (#2976 by: HebaruSan, DasSkelett)
- [Build] Refresh Info.plist if changelog has changed (#2978 by: HebaruSan; reviewed: DasSkelett)

## v1.26.8 (Kodiak)

### Features

- [Multiple] Custom mod labels, favorites, hiding (#2936 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Add "supports" relationships to recommendations screen (#2960 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Prompt user to delete non-empty folders after uninstall (#2962 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Edit modpack before export (#2971 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [GUI] Don't launch KSP 1.8 with -single-instance (#2931 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Handle multiple errors in same ErrorDialog (#2933 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Multiple manual downloads, uncached filter, purge option (#2930 by: HebaruSan; reviewed: DasSkelett)
- [CLI] Return failure on failed commands for headless prompt (#2941 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Obey system colors for dark theme support (#2937 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Workaround to launch URLs (#2958 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Memoize lazily evaluated sequences (#2953 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Don't check ZIP health in ModuleInstaller.GetModuleContentsList() (#2959 by: DasSkelett; reviewed: HebaruSan)
- [Core] Check all dependencies for compatibility checking (#2963 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Fix CKAN-installed modules shown as AD in some cases (#2969 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Build] Bump nuget to 5.3.1 on Windows (#2929 by: DasSkelett; reviewed: HebaruSan)
- [Build] Don't send notification to Discord if build succeeds (#2932 by: DasSkelett: reviewed: HebaruSan)
- [Netkan] Auto-epoch based on queue message attribute (#2824 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Better parsing errors for version files (#2939 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Retain URL hash cache, cache file hashes (#2940 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Get org members as authors, time-sort authors, sort tags to middle (#2942 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Coerce GitHub URLs into the authenticated API in Netkan (#2946 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Request fewer GitHub releases and cache string URLs in Netkan (#2950 by: HebaruSan)
- [GUI] Create ~/.local/share/applications/ if it doesn't exist on Linux (#1848 by: DinCahill; reviewed: ayan4ml, politas, dannydi12)
- [Netkan] Catch nested GameData folders in Netkan (#2948 by: HebaruSan; reviewed: techman83)
- [Netkan] Only stage auto-epoch when creating new file (#2947 by: HebaruSan; reviewed: DasSkelett)
- [Build] Use Mono 6.6 and more recent versions of everything else (#2964 by: HebaruSan; reviewed: DasSkelett, Olympic1)
- [Netkan] Stage modules with hardcoded game versions (#2970 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Move GUI files into subfolders, split controls out from Main (#2966 by: HebaruSan; reviewed: DasSkelett)

## v1.26.6 (Leonov)

### Features

- [GUI] Default to consoleui on MacOSX Catalina (#2911 by: HebaruSan; reviewed: pfFredd, DasSkelett)
- [GUI] Add language selection option to settings (#2925 by: DasSkelett; reviewed: HebaruSan)

### Bugfixes

- [Build] Use Debian machine-readable copyright format (#2853 by: bfrobin446; reviewed: HebaruSan)
- [GUI] Don't invalidate invisible rows (#2854 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Speed up autoremove search (#2855 by: HebaruSan; reviewed: DasSkelett)
- [Core] Find portable installs when enforcing cache limits (#2856 by: HebaruSan; reviewed: DasSkelett)
- [Build] Add libcurl dependency to RPM (#2858 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Allow totally incompatible modules in changeset (#2869 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Remove base64 encoded CKAN icon from resources (#2872 by: DasSkelett; reviewed: HebaruSan)
- [Multiple] Fail on http status codes >=300 for cURL downloads (#2879 by: DasSkelett; reviewed: HebaruSan)
- [Multiple] Normalize install paths (#2887 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Fixes for KSP in Windows drive root (#2857 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Use current metadata for installed module compatibility (#2886 by: HebaruSan; reviewed: DasSkelett)
- [Core] Notify when falling back to archive.org URL (#2892 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Only run Mono in 32bit mode when the GUI is launched (#2893 by: DasSkelett; reviewed: HebaruSan)
- [Build] Force logical name for generated resources (#2899 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Show target version for upgrades in change set (#2888 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Merge resources and include metanetkan (#2913 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Force redraw of recommendation listview headers on Mono (#2920 by: HebaruSan; reviewed: DasSkelett)
- [Core] Restructure Net.Download and Net.DownloadText (#2904 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Eliminate duplicate network calls in Netkan (#2928 by: HebaruSan; reviewed: DasSkelett, techman83)
- [ConsoleUI] Disable Upgrade All ConsoleUI menu option when nothing to upgrade (#2927 by: HebaruSan; reviewed: DasSkelett)

### Internal

- [Build] Redeploy Inflator after builds (#2889 by: HebaruSan; reviewed: techman83)
- [Netkan] Add owners of parent GitHub repos to authors (#2922 by: HebaruSan; reviewed: DasSkelett)
- [Build] Update dependencies (#2921 by: DasSkelett; reviewed: HebaruSan, politas)

## v1.26.4 (Orion)

### Features

- [Multiple] Auto-uninstall auto-installed modules (#2753 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Add scrollbars to metadata tab (#2759 by: DasSkelett; reviewed: HebaruSan)
- [Core] Detect Breaking Ground DLC (#2768 by: dbent; reviewed: Olympic1, HebaruSan)
- [GUI] Internationalize the GUI (#2749 by: HebaruSan, DasSkelett; reviewed: DasSkelett, Olympic1)
- [Netkan] Extract locales from downloads (#2760 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Add menu option to report issues in the CKAN repo (#2801 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Migrate Perl validation checks into netkan.exe (#2788 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Open ZIP button, instance name in status bar, description scroll bar (#2813 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Support BreakingGround-DLC in instance faking (#2773 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Queue module version changes in change set (#2821 by: HebaruSan; reviewed: Olympic1)
- [Build] Generate RPM packages (#2757 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [Core] Save registry inside of scan transaction (#2755 by: HebaruSan; reviewed: DasSkelett)
- [Core] Cache downloads individually upon completion (#2756 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Hide auto-installed checkbox for auto-detected mods (#2758 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Update GUI modlist if scan detects changes (#2762 by: HebaruSan; reviewed: DasSkelett)
- [Core] Don't assume string params to Install are identifiers (#2764 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Don't warn that a raw URL is non-raw (#2767 by: HebaruSan; reviewed: DasSkelett)
- [Multiple] Don't throw exceptions for dependency conflicts (#2766 by: HebaruSan; reviewed: DasSkelett)
- [Core] Suppress autostart warning (#2776 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Skip comments and allow capital letters in locale extractor (#2783 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix UK spelling of licence (#2794 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Mark auto-install correctly when installing from .ckan file (#2793 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Ignore locales with no strings (#2805 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Fix http kref validation (#2811 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Fix Netkan localization parser performance (#2816 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Support newlines in GUI description field (#2818 by: HebaruSan; reviewed: DasSkelett)
- [Core] Fix comparison of 1.0.0 to 1.0 (#2817 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Improve focus interactions of GUI filters and mod list (#2827 by: HebaruSan; reviewed: Olympic1)
- [GUI] Fix issues when `provides` is removed (#2740 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix 'ManageKspInstances' dialog logic (#2787 by: DasSkelett; reviewed: HebaruSan)

### Internal

- [Build] Update packages (#2775 by: Olympic1; reviewed: DasSkelett, HebaruSan)
- [Build] Fix fake/clone tests on Windows (#2778 by: HebaruSan; reviewed: DasSkelett)
- [Reporting] Update issue templates (#2777 by: Olympic1; reviewed: DasSkelett, HebaruSan)
- [Build] Fix ZipValid test on non-English Windows systems (#2781 by: HebaruSan; reviewed: DasSkelett)
- [Build] Fix for building with VS 2019 (#2834 by: Olympic1; reviewed: DasSkelett)
- [Netkan] Refactor Netkan for SQS mode (#2798 by: HebaruSan; reviewed: DasSkelett, techman83)
- [Build] Add Dockerfile + Deployment for NetKAN Inflator (#2838 by: techman83; reviewed: HebaruSan)
- [Core] Move config from Windows Registry to JSON file; Make CKAN-core .NET Standard 2.0 compliant (#2820 by: jbrot; reviewed: HebaruSan, Politas, Olympic1, DasSkelett)

## v1.26.2 (Dragon)

### Features

- [GUI] Right click copy option for mod info links (#2699 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Relabel Yes/No buttons for some questions (#2737 by: HebaruSan; reviewed: DasSkelett)
- [Netkan] Handle non-raw GitHub URLs for metanetkans and avc krefs (#2696 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Ignore spaces and special chars in mod search (#2709 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Combine recommendation and suggestion screens (#2744 by: HebaruSan; reviewed: DasSkelett)

### Bugfixes

- [GUI] Save restore position when minimized (#2725 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix home/end, url handler, typing nav, and dependency double click (#2727 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix size of relationships list (#2728 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Don't apply previous change set to checkboxes after successful install (#2730 by: HebaruSan; reviewed: DasSkelett)
- [Core] Compare RelationshipDescriptors with Equals instead of Same (#2735 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Auto size yes no dialog (#2729 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Fix Newly Compatible filter with repo autoupdate (#2734 by: HebaruSan; reviewed: DasSkelett)
- [GUI] Throw errors happening in UpdateModsList() (#2745 by: DasSkelett; reviewed: HebaruSan)
- [Core] Fix freeze on non CKAN files in cache folder (#2743 by: HebaruSan; reviewed: DasSkelett)
- [Core] Ignore dependency self conflicts (#2747 by: HebaruSan; reviewed: DasSkelett)

## v1.26.0 (Baikonur)

### Features

- [GUI] Checkbox to uninstall all mods and reset changeset (#2596 by: HebaruSan; reviewed: politas)
- [GUI] Add legend for relationships tab (#2592 by: HebaruSan; reviewed: politas)
- [GUI] Add Launch KSP option to tray icon menu (#2597 by: HebaruSan; reviewed: politas)
- [GUI] Confirm quit with pending change set or conflicts (#2599 by: HebaruSan; reviewed: politas)
- [Multiple] Warn before launching KSP with installed incompatible modules (#2601 by: HebaruSan; reviewed: politas)
- [GUI] Allow selection of text in mod info panel (#2610 by: DasSkelett; reviewed: HebaruSan)
- [Multiple] Show progress bar while loading registry (#2617 by: HebaruSan; reviewed: politas)
- [Multiple] Add possibility to clone KSP installs and create dummy ones (#2627 by: DasSkelett; reviewed: HebaruSan, politas)
- [ConsoleUI] Allow overriding menu tip in ConsoleUI (#2635 by: HebaruSan; reviewed: DasSkelett, politas)
- [Netkan] Get license from GitHub (#2663 by: HebaruSan; reviewed: politas)
- [Multiple] Cleanly switch versions of installed mod (#2669 by: HebaruSan; reviewed: politas)
- [Multiple] Implementation of clone and fake in GUI (#2665 by: DasSkelett; reviewed: HebaruSan, politas)
- [Multiple] Support depends on any_of lists (#2660 by: HebaruSan; reviewed: politas)
- [Build] Use Core.Utilities.CopyDirectory in tests (#2670 by: DasSkelett; reviewed: HebaruSan)
- [Core] Avoid redundant metadata downloads (#2682 by: HebaruSan; reviewed: DasSkelett, politas)
- [Netkan] Releases option for Netkan (#2681 by: HebaruSan; reviewed: politas)
- [Multiple] Support replaced_by property (#2671 by: politas, HebaruSan; reviewed: DasSkelett, politas)
- [GUI] Customisable columns in GUI Modlist (#2690 by: DasSkelett; reviewed: HebaruSan)

### Bugfixes

- [GUI] Fix platform checks and crash on Mac OS X (#2600 by: HebaruSan; reviewed: politas)
- [GUI] Fix file menu separator (#2593 by: HebaruSan; reviewed: politas)
- [GUI] Fix error popup text for dark themes (#2594 by: HebaruSan; reviewed: politas)
- [GUI] Stop splitters from migrating between sessions (#2598 by: HebaruSan; reviewed: politas)
- [Multiple] Don't auto-install recommendations when auditing recommendations (#2606 by: HebaruSan; reviewed: politas)
- [GUI] Suppress wrapping of status bar in Mono (#2607 by: HebaruSan; reviewed: politas)
- [GUI] Remove unnecessary parameter in Configuration methods (#2608 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Revert unintentional tray icon change from #2587 (#2609 by: HebaruSan; reviewed: politas)
- [GUI] Show correct error messages after canceling (un)installations/upgrades (#2602 by: DasSkelett; reviewed: HebaruSan)
- [Netkan] Jenkins as a Netkan "source" (#2613 by: HebaruSan; reviewed: politas)
- [Multiple] Encapsulate usages of WebClient (#2614 by: HebaruSan; reviewed: politas)
- [Netkan] Handle multiple game versions from Curse (#2616 by: HebaruSan; reviewed: politas)
- [GUI] Fix UpdateModsList crash on Mono (#2625 by: HebaruSan; reviewed: politas)
- [GUI] Fix System.ObjectDisposedException for TransparentTextBox (#2619 by: DasSkelett; reviewed: HebaruSan)
- [Build] Dispose caches in tests (#2628 by: HebaruSan; reviewed: politas)
- [GUI] Don't override menu renderers on Windows (#2632 by: HebaruSan; reviewed: politas)
- [ConsoleUI] Handle plus without shift in ConsoleUI (#2634 by: HebaruSan; reviewed: politas)
- [Multiple] `ckan ksp fake/clone` fixes (#2642 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Handle semi-virtual dependencies in Relationships tab (#2645 by: HebaruSan; reviewed: politas)
- [Multiple] Derive User classes from IUser interface instead of NullUser (#2648 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Show all recommendations of metapackages in GUI (#2653 by: HebaruSan; reviewed: politas)
- [GUI] Don't try to update filters if mod list missing (#2654 by: HebaruSan; reviewed: politas)
- [GUI] Invoke control accesses in UpdateModsList (#2656 by: DasSkelett; reviewed: politas)
- [GUI] Set focus to mod list after loading (#2657 by: HebaruSan; reviewed: politas)
- [GUI] Small text/number formatting changes to mod list (#2658 by: DasSkelett; reviewed: politas)
- [Multiple] Remove ConfirmPrompt from IUser (#2659 by: HebaruSan; reviewed: politas)
- [ConsoleUI] Handle manually installed mods in ConsoleUI (#2666 by: HebaruSan; reviewed: politas)
- [GUI] Add AD mods back into GUI's installed filter (#2668 by: HebaruSan; reviewed: politas)
- [GUI] Allow installation of missing dependencies in GUI (#2674 by: HebaruSan; reviewed: politas)
- [GUI] Fix window position on MacOSX (#2677 by: HebaruSan; reviewed: politas)
- [GUI] Fix upgrading and installing from .ckan in GUI (#2680 by: HebaruSan; reviewed: politas)
- [GUI] Fix RefreshPreLabel overlapping RefreshTextBox in Settings (#2686 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Force redraw of versions ListView on Mono (#2685 by: HebaruSan; reviewed: DasSkelett, politas)
- [GUI] Don't auto-install recommendations when reinstalling (#2689 by: HebaruSan; reviewed: politas)
- [GUI] Allow replacement by conflicting modules (#2695 by: HebaruSan; reviewed: politas)
- [GUI] Sort AD above empty checkboxes (#2691 by: HebaruSan; reviewed: politas)
- [Netkan] Reinstate no releases warnings for Netkan (#2692 by: HebaruSan; reviewed: politas)
- [GUI] Only update mod list once at GUI startup (#2694 by: HebaruSan; reviewed: politas)
- [GUI] Only show replace col if a replaced module is installed (#2697 by: HebaruSan; reviewed: politas)
- [GUI] Update all CKAN URL references (#2702 by: DasSkelett; reviewed: HebaruSan)
- [GUI] Select 2nd instead of 3rd cell on MarkAllUpdatesToolButton_Click (#2704 by: DasSkelett; reviewed: politas)
- [GUI] Prevent Null entry in CompatibleKSPVersions (#2707 by: politas; reviewed: DasSkelett)

## v1.25.4 (Kennedy)

### Features

- [Netkan] Purge downloads that failed to index from Netkan cache (#2526 by: HebaruSan; reviewed: politas)
- [Multiple] Add download count column to GUI (#2518 by: HebaruSan; reviewed: politas)
- [Netkan] Catch invalid $kref in Netkan (#2516 by: HebaruSan; reviewed: politas)
- [Netkan] Handle KSP-AVC krefs (#2517 by: HebaruSan; reviewed: politas)
- [Multiple] One Cache to Rule Them All (#2535 by: HebaruSan; reviewed: politas)
- [Multiple] Configurable cache size limit (#2536 by: HebaruSan; reviewed: politas)
- [GUI] Minimize CKAN to tray and automatic refreshing (#2565 by: Olympic1; reviewed: HebaruSan)
- [GUI] Show mod info for change set, recs/sugs, and providers (#2556 by HebaruSan; reviewed: politas)
- [Netkan] Add Netkan option to overwrite cached files (#2582 by HebaruSan; reviewed: politas)
- [GUI] Show recommendations of installed modules in GUI (#2577 by HebaruSan; reviewed: politas)
- [GUI] Remove newline from Done progress message (#2580 by HebaruSan; reviewed: politas)
- [GUI] Set window properties on X11 (#2586 by: HebaruSan; reviewed: politas)
- [Build] Add tests for recent changes (#2583 by: HebaruSan; reviewed: politas)

### Bugfixes

- [GUI] Show innermost download exceptions (#2528 by: HebaruSan; reviewed: politas)
- [GUI] Fix grid colors for dark themes (#2529 by: HebaruSan; reviewed: politas)
- [GUI] Fix YesNoDialog layout (#2530 by: HebaruSan; reviewed: politas)
- [GUI] Handle exception for missing libcurl (#2531 by: HebaruSan; reviewed: politas)
- [Core] Catch illegal characters in ZIP exceptions (#2515 by: HebaruSan; reviewed: politas)
- [Netkan] Handle two-part KSP-AVC versions (#2532 by: HebaruSan; reviewed: politas)
- [Core] Stop auto-moving cached files (#2538 by: HebaruSan; reviewed: politas)
- [GUI] Fix toolbar background colors for dark themes (#2541 by: HebaruSan; reviewed: Olympic1)
- [GUI] Fix text colors for dark themes (#2540 by: HebaruSan; reviewed: Olympic1)
- [GUI] Clean up mod list spacebar handling in GUI (#2543 by: HebaruSan; reviewed: politas)
- [Core] Use fresh WebClient for fallback URLs (#2539 by: HebaruSan; reviewed: politas)
- [GUI] Clean up popup positioning in GUI (#2544 by: HebaruSan; reviewed: politas)
- [Core] Don't throw exceptions when resetting cache dir (#2547 by: HebaruSan; reviewed: politas)
- [GUI] Fix crash at startup on Windows risen in #2536 (#2557 by: HebaruSan; reviewed: Olympic1)
- [Core] Allow game version of "any" with a vref (#2553 by: HebaruSan; reviewed: Olympic1)
- [Core] Fix null ref exception when repo has empty ckan file (#2549 by: HebaruSan; reviewed: Olympic1)
- [Multiple] Avoid null ksp_version in Netkan (#2558 by: HebaruSan; reviewed: politas)
- [GUI] Show latest updates after refresh (#2552 by: HebaruSan; reviewed: politas)
- [GUI] Don't show status progress bar till actually installing (#2560 by: HebaruSan; reviewed: Olympic1)
- [Core] Purge 5.5 MB of bloat from `registry.json` (#2179 by: HebaruSan; reviewed: Olympic1)
- [GUI] Allow uninstallation of conflicting mods (#2561 by: HebaruSan; reviewed: Olympic1)
- [Core] Cache listings of legacy cache dirs (#2563 by: HebaruSan; reviewed: politas)
- [Netkan] Treat AVC min without max as open range (#2571 by: HebaruSan; reviewed: politas)
- [GUI] Restore uninstallation of dependencies (#2579 by: HebaruSan; reviewed: politas)
- [GUI] Fix tray icon menu position and color on Linux (Gnome) (#2587 by: HebaruSan; reviewed: politas)
- [GUI] Fix crash on selecting filtered-out provider module (#2585 by: HebaruSan; reviewed: politas)

## v1.25.3 (Woomera)

### Features

- [GUI] Replace New in repository filter with Newly compatible filter (#2494 by: HebaruSan; reviewed: Olympic1, politas)
- [GUI] Add Install Date column to GUI mod list (#2514 by: HebaruSan; reviewed: politas)

### Bugfixes

- [Multiple] Fix crash when initializing CKAN dirs at argumentless GUI startup (#2482 by: HebaruSan; reviewed: politas)
- [Core] Allow installing modules without `download_size` (#2491 by: HebaruSan)
- [Multiple] Fix GUIMod crash when module doesn't have a compatible game version (#2486 by: HebaruSan; reviewed: cculianu, politas)
- [Core] Fix crash on invalid portable or Steam folder (#2506 by: HebaruSan; reviewed: politas)
- [GUI] Fix red X on HideTab (#2501 by: HebaruSan; reviewed: politas)
- [Core] Set default Exception.Message string for ModuleNotFoundKraken (#2493 by: HebaruSan; reviewed: politas)
- [Core] Fix missing CdFileMgr folder errors (#2492 by: HebaruSan; reviewed: politas)

## v1.25.2 (Goddard)

### Features

- [GUI] Limit future Max KSP column values based on known versions (#2437 by: yalov; reviewed: politas)
- [GUI] Add description to ModInfoTab (#2463 by: politas; reviewed: HebaruSan)

### Bugfixes

- [Core] Ignore conflicts between versions of same mod (#2430 by: HebaruSan; reviewed: politas)
- [GUI] Don't Force Apply button active when no update selected (#2429 by: DasSkelett; reviewed: politas)
- [Core] Improve handling of missing game version (#2444 by: HebaruSan; reviewed: politas)
- [Core] Handle zero byte registry.json file (#2435 by: HebaruSan; reviewed: politas)
- [Multiple] Pass game instance from cmdline to GUI/ConsoleUI (#2449 by: HebaruSan; reviewed: politas)
- [GUI] Show conflict messages in status bar (#2442 by: HebaruSan; reviewed: dbent, politas)
- [GUI] Remove v in installed version and latest version columns (#2451 by yalov; reviewed: politas)
- [Netkan] Support new Curse URLs (#2464 by: HebaruSan; reviewed: Olympic1, politas)
- [Netkan] Fix Netkan error message when both `ksp_version` and min/max are present (#2480 by: HebaruSan; reviewed: politas)

### Internal

- [Core] Test upgrading mod with conflict on its own provides (#2431 by: HebaruSan; reviewed: politas)

## v1.25.1 (Broglio)

### Features

- [GUI] Replace empty max KSP version string with "any" (#2420 by: DasSkelett; reviewed: HebaruSan, politas)

### Bugfixes

- [GUI] Splitter and tabstrip visual improvements (#2413 by: HebaruSan; reviewed: politas)
- [GUI] Fix "Collection was modified" exception for redundant optional dependencies (#2423 by: HebaruSan; reviewed: politas)
- [Core] Treat installed DLC as compatible dependency (#2424 by: HebaruSan; reviewed: politas)
- [GUI] Ignore splitter exceptions (#2426 by: HebaruSan; reviewed: politas)

### Internal

- [Build] Add more tests (#2410 by: HebaruSan, DasSkelett; reviewed: politas)
- [Updater] AutoUpdate: tokens and tests (#2411 by: HebaruSan; reviewed: politas)

## v1.25.0 (Wallops)

### Features

- [Core] Detect DLC and allow as a dependency (#2326 by: dbent; reviewed: politas)
- [GUI] Install old mod versions by version list double-click (#2364 by: HebaruSan; reviewed: politas)
- [Core] Allow installations to the Missions folder (#2371 by: Olympic1; reviewed: politas)
- [GUI] Sort by "update"-column on clicking "add available updates"-button (#2392 by: DasSkelett; reviewed: politas)

### Bugfixes

- [Multiple] Fix crash when trying to view empty auth token list (#2301 by: HebaruSan; reviewed: politas)
- [GUI] Handle mod not found in versions tab (#2303 by: HebaruSan; reviewed: politas)
- [GUI] Better resizing for Select KSP Install window (#2306 by: HebaruSan; reviewed: politas)
- [GUI] Fix GUI sort by size (#2311 by: HebaruSan; reviewed: politas)
- [Core] Don't crash if `download_hash` isn't set (#2313 by: HebaruSan; reviewed: politas)
- [GUI] Fix GUI instance name checking (#2316 by: HebaruSan; reviewed: politas)
- [Core] Fix ArgumentOutOfRangeException when removing files from game root (#2332 by: HebaruSan; reviewed: politas)
- [Core] Obey version properties of conflicts and depends relationships in sanity checks (#2339 by: HebaruSan; reviewed: politas)
- [Netkan] Invalidate stale cached files from GitHub in Netkan (#2337 by: HebaruSan; reviewed: politas)
- [Build] Allow building CKAN.app on macOS (#2356 by: phardy; reviewed: HebaruSan)
- [GUI] Always switch to progress tab when starting a [re/un]install (#2351 by: HebaruSan; reviewed: Olympic1)
- [Core] Support CC-BY-ND licences in code (#2369 by: HebaruSan; no review)
- [GUI] Improve response to checkbox changes (#2354 by: HebaruSan; reviewed: politas)
- [Core] Allow downloader to be used multiple times (#2365 by: HebaruSan; reviewed: politas)
- [GUI] Clean up URL Handler registration (#2366 by: HebaruSan; reviewed: politas)
- [Multiple] Deal with threading and download errors (#2374 by: HebaruSan; reviewed: politas)
- [GUI] More verbose and accurate version displays (#2382 by: HebaruSan; reviewed: politas)
- [Core] Encode spaces in URL output (#2386 by: HebaruSan; reviewed: politas)
- [Multiple] Clean-up and debuggability (#2399 by: HebaruSan; reviewed: politas)
- [Netkan] Don't double encode GitHub download URLs (#2402 by: HebaruSan; reviewed: politas)
- [Netkan] Option to override SpaceDock version with AVC version (#2406 by: HebaruSan; reviewed: politas)
- [GUI] Move AutoUpdate.CanUpdate check to resolve VisualStudio Designer Error (#2407 by: DasSkelett; reviewed: Olympic1, politas)
- [GUI] Better AutoUpdate.CanUpdate Error Message (#2408 by: DasSkelett; reviewed: politas)

### Internal

- [Build] Improve CKAN.app launch script (#2329 by: HebaruSan; reviewed: politas)
- [Build] Fix building on macOS (#2341 by: phardy; reviewed: HebaruSan, politas)
- [Build] Fix autoupdater tests on TLS-fragile platforms (#2344 by: HebaruSan; reviewed: politas)
- [Build] Remove extra copies of various files (#2363 by: HebaruSan; reviewed: Olympic1)

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
- [Updater] Move AskForAutoUpdates dialog to center of screen (#2165 by: politas; reviewed: Olympic1)
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
- [Netkan] Adapt Curse API to new widget (#2189 by: HebaruSan; reviewed: Olympic1)
- [Reporting] Improvement of issues template to help with bug reporting (#2201 by: HebaruSan; reviewed: Olympic1)

## v1.22.6 (Guiana)

### Bugfixes

- [GUI] Fix search box tab order (#2141 by: HebaruSan; reviewed: politas)
- [Core] Check for stale lock files (#2139 by: HebaruSan; reviewed: politas)
- [Netkan] Improve error output (#2144 by: HebaruSan; reviewed: Olympic1)
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
- [Netkan] Convert spaces to %20 in Curse URLs (#2041 by: politas; reviewed: -)

### Features

- [Build] Use Cake for build and overhaul/cleanup build (#1589 by: dbent; reviewed: techman83, politas)
- [Build] Docker updates to support cake! (#1988 by: mathuin; reviewed: dbent)
- [Build] Update Build packages (#2028 by: dbent; reviewed: Olympic1)
- [Build] Update Build for Mono 5.0.0 (#2049 by: dbent; reviewed: politas)
- [Build] Update Update build (#2050 by: dbent; reviewed: politas)
- [Core] Update KSP builds (#2056 by: Olympic1; reviewed: linuxgurugamer)
- [Netkan] Canonicalize non-raw GitHub URIs (#2054 by: dbent; reviewed: politas)

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
- [Netkan] Add regexp second test for filespecs (#1919 by: politas; reviewed: ayan4m1)
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
- [Multiple] Refactoring variables per project guidelines, Add Report Issue link in Help Menu, make SafeAdd safer, cleanup some Doco wording (#1849 by: ayan4m1; reviewed: politas)
- [GUI] Resize KSP Version Label to keep in window (#1837 by: Telanor, #1854 by: politas; reviewed: ayan4m1)
- [GUI] Fix GUI exceptions when installing/uninstalling after sorting by KSP max version (#1882, #1887 by: ayan4m1; reviewed: politas)
- [Core] Fix Windows-only NullReferenceException and add more ModuleInstaller tests (#1880 by: ayan4m1; reviewed: politas)

### Features

- [GUI] Add Back and Forward buttons to the GUI to jump to selected mods (#1841 by: b-w; reviewed: politas)
- [Netkan] GitHub Transformer now extracts the repository name and transforms it to a usable ckan name (#1613 by: Olympic1; reviewed: pjf)
- [GUI] Don't show suggested/recommended for mods that can't be installed (#1427 by: Postremus; reviewed: politas)
- [Core] Remove empty directories when uninstalling mods (#1873 by: politas; reviewed: ayan4m1)
- [Core] Users are less able to run two copies of CKAN at the same time. (#1265 by: mgsdk, #1357 by: pjf, #1828 by: politas; reviewed: ayan4m1)
- [Netkan] Add Curse as a $kref source (#1608 by: airminer, Olympic1; reviewed: dbent, pjf, techman83, ayan4m1)
- [Multiple] Relationship changes now prompt reinstalling (#1730 by: dbent, #1885 by: ayan4m1; reviewed: plague006, pjf)
- [GUI] Add "X" icon to filter text boxes that clears the control (#1883 by: ayan4m1; reviewed: politas)

## v1.18.1 (StarTrammier)

### Bugfixes

- [CLI] Improve legend on `ckan list` functionality. (#1664 by: politas; reviewed: pjf)
- [Core] Workaround string.Format() bug in old Mono versions (#1784 by: dbent; reviewed: postremus)

### Features

- [CLI] `ckan.exe ksp list` now prints its output as a table and includes the version of the installation and its default status. (#1656 by: dbent; reviewed: pjf)
- [GUI] Auto-updater shows download progress (#1692 by: trakos, #1359 by: Postremus; reviewed: politas)
- [GUI] About dialog pointing to new CKAN thread on forums (#1824 by: politas; reviewed: -)
- [GUI] Ignore ksp instances if we are launching the gui anyway (#1809 by: Postremus; reviewed: politas)

### Internal

- [Build] Travis/Build: Prevent mozroots from asking for human input (#1825 by: pjf; reviewed: politas)

## v1.18.0 (StarTram)

### Bugfixes

- [Core] In certain cases a `NullReferenceException` could be produced inside error handling code when processing the registry. (#1700 by: keyspace; reviewed: dbent)
- [GUI] Fix typo in export options. (#1718 by: dandrestor; reviewed: plague006)
- [GUI] Fix unit of measure for download speed. (#1732 by: plague006; reviewed: dbent)
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
- [Netkan] Catch ValueErrors rather than printing the trace (#1648 by: techman83; reviewed: Daz)
- [Netkan] Catch `ksp_version` from SpaceDocks newly implemented `game_version` (#1655 by: dbent; reviewed: -)
- [Netkan] Allow specifying when an override is executed (#1684 by: dbent; reviewed: techman83)
- [Netkan] Redirects to the download file are now resolved when using HTTP $krefs (#1696 by: dbent; reviewed: techman83)
- [Netkan] Remote AVC files will be used in preference to ones stored in the archive if they have the same version (#1701 by: dbent; reviewed: techman83)
- [Netkan] Sensible defaults are used when fetching abstract and homepage from github. (#1726, #1723 by: dbent; reviewed: politas)
- [Netkan] Add Download Attribute Transformer (#1710 by: techman83; reviewed: dbent)
- [Netkan] Add ksp_version_strict to property sort order (#1722 by: dbent; reviewed: plague006)
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

- [GUI] General code tidy-up. (#1582, #1602 by: ChucklesTheBeard; reviewed: plague006, Olympic1)
- [GUI] Avoidance of a future bug involving how we query users regarding choices. (#1538 by: pjf, RichardLake; reviewed: Postremus)
- [GUI] Fixed mispellings in the word "directory". (#1624 by: tonygambone; reviewed: pjf)
- [Spec] Updated Spec with newer `netkan.exe` features. (#1581 by: dbent; reviewed: Dazpoet)
- [Netkan] `netkan.exe` now has support for downloading GitHub sources of a release. (#1587 by: dbent; reviewed: Olympic1)
- [Netkan] `netkan.exe` checks for malformed url's and prevents them from being added to the metadata. (#1580 by: dbent; reviewed: Olympic1)
- [Netkan] `netkan.exe` will now add all authors listed on SpaceDock (#1600, #1620 by: dbent; reviewed: techman83)
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

- [Netkan] `netkan.exe` will now sort `conflicts` relationships next to other relationships. (dbent, #1496)
- [Netkan] `netkan.exe` now has much better support for Jenkins CI servers, allowing full automation. (dbent, #1512)

## v1.14.3 (Haumea)

### Bugfixes

- [Core] CKAN is more likely to find your KSP install. (McJones, #1480)
- [Core] Uninstalled mods will no longer be reported as upgradeable when another mod provides their fuctionality. (Postremus, #1449)
- [GUI] Installing a `.ckan` virtual mod will crash the client with an NRE less often (Postremus, #1478)
- [GUI] The "Installing mods" tab is now called the "Status log" tab, as it's used for upgrading and removing mods too. (plague006, #1460)
- [GUI] Links to `ckan://` resources under Linux are more likely to be handled correctly. (Postremus, #1434)
- [GUI] Mods upgrades with additional dependencies are better handled and displayed to the user. (Postremus, #1447)

### Features

- [GUI] The CKAN Identifier for each mod is now shown in their metadata panel. (plague006, #1476)
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

- [Multiple] When installing and uninstalling mods, the mod name and version will be used rather than the internal identifier. (Postremus, #1401)
- [GUI] The GUI changeset screen now provides explanations as to why changes are being made. (Postremus, #1412)

### Internal

- [Netkan] `netkan.exe` will now report its version and exit if run with the `--version` switch. (pjf, #1415)

## v1.14.0 (Mimas)

### Bugfixes

- [GUI] The CKAN client is less likely to believe that *all* mods are new when auto-updating metadata is enabled. (Postremus, #1369)
- [GUI] The latest CKAN-available version of a mod is always shown in the 'latest' column, even if that mod is not compatible with the installed KSP version. (Postremus, #1396)
- [GUI] Pressing a key with an empty modlist will no longer crash the client. (Postremus, #1329)
- [GUI] The 'mark for update' button no longer highlights when the only upgrade candidates are autodetected mods we can't actually upgrade. (Postremus, #1392)
- [Core] Installing from `.ckan` files (such as exported modlists) is more likely to succeed even when dependencies are complex. (#1337, Postremus)
- [CLI] `ckan.exe --verbose` and `ckan.exe --debug` now start the GUI with the appropriate logging mode. (#1403, Postremus)
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
- [Netkan] Reading of information from `.version` files greatly improved, especially when mixing metadata from other sources like github and kerbalstuff. (dbent, #1299)
- [Core] Files can now be installed to `saves/scenarios` using the `Scenarios` target. (pjf, #1360)
- [Spec] Grammar corrections. (Postremus, #1400)
- [Netkan] Files produced by `netkan.exe` have more stable field ordering. (dbent, #1304)
- [Netkan] `netkan.exe` can use regexps to manipulate version strings. (dbent, #1321)
- [GUI] Refactoring to remove duplicated code. (Postremus, #1362)

## v1.12.0 (Veil Nebula)

### Bugfixes

- [GUI] Hitting `cancel` is much more likely to actually cancel an install-in-progress (Postremus, #1325)
- [GUI] Fewer crashes when double-clicking on an auto-detected mod (Postremus, #1237)
- [CLI] `ckan compare` fails more gracefully when called without arguments (Postremus, #1283)
- [CLI] `ckan show` more accurately displays the cached filename (mgsdk, #1266)
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

- [Netkan] Cached files have more descriptive file extensions (dbent, #1308)
- [Netkan] A warning is generated if a file without a `.netkan` extension is processed (dbent, #1308)
- [Netkan] We better handle null values in retrieved metadata (dbent, #1324)
- [Netkan] Better handling of license strings with leading and trailing spaces (dbent, #1305)
- [Core] We no longer try to use `libcurl` on systems where the .NET web classes are sufficient (dbent, #1294)
