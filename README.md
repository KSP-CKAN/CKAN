# The Comprehensive Kerbal Archive Network (CKAN)

**This document is a draft and may change. Contributions welcome.**

This is a Request For Comments on the Comprehensive Kerbal Archive
Network (CKAN). Please open discussions in the issues list, and
send pull requests to patch this document and associated files.

This document, and all associated files, are licensed under your
choice of the Creative Commons Attribution license 4.0 (CC-BY 4.0),
Lesser GNU Public License (LGPL), or the MIT license (MIT).

## Introduction

There have been many comprehensive archive networks for various
languages and platforms. While the original network was for TeX
(the CTAN), the most successful has been for Perl (the CPAN),
with over 11,000 contributors and 30,000 distributions.

The goal of the CKAN is to provide a network that is easy to use
for both mod authors and end users. By providing a standardised way
to release and install modules, it is hoped that many of the misinstall
problems will be eliminated (reducing the workload on authors), and
a more straightforward path of installing mods is provided (making
it easier for users to use mods).

## Design

The fundamental design of the CKAN is as follows:

- Each *distribution* (a mod and its associated files) *must* have an
  associated meta-data file that describes its contents.
- The meta-data file *must* be detachable from the distribution
  itself. This facilities easy building of indexes, and means meta-data
  can be created independently of the distribution itself, easing
  adoption by authors.
- The meta-data file *should* be included in the distribution whenever
  possible.

## Validation

A [JSON Schema](CKAN.schema) is provided for validation purposes.
Any CKAN file *must* conform to this schema to be considered valid.

## The CKAN file

A CKAN file is designed to contain all the relevant meta-info
about a mod, including its name, license, download location,
dependencies, compatible versions of KSP, and the like. CKAN
files are simply JSON files.

When included in a distribution, the metadata *must* be included
in a file with a `.ckan` extension, which contains JSON data. The
guidelines for the file name and location are:

- The name of the file *should* match the `identifier` field for
  the mod it describes. (Eg: `RealSolarSystem.ckan`)

- The name of the file *may* be appended with a dash, followed
  by the version number of the mod it describes
  (Eg: `RealSolarSystem-7.3.ckan`).

- When bundled with the mod, the CKAN file *should* be placed in the
  same directory as the main mod itself. (Eg:
  `RealSolarSystem/RealSolarSystem.ckan` or
  `GameData/ExampleMod/ExampleMod.ckan`).

The CKAN metadata spec is inspired by the
[CPAN metadata spec](https://metacpan.org/pod/CPAN::Meta::Spec)
and the
[KSP-RealSolarSystem-Bundler](https://github.com/NathanKell/KSP-RealSolarSystem-Bundler)

### Example CKAN file

    {
        "spec_version": 1,
        "name"        : "Real Solar System",
        "identifier"  : "RealSolarSystem",
        "abstract"    : "Resizes and rearranges the Kerbal system to more closely resemble he Solar System",
        "download"    : "https://github.com/NathanKell/RealSolarSystem/releases/download/v7.3/RealSolarSystem_v7.3.zip",
        "license"     : "CC-BY-NC-SA",
        "version"     : "7.3",
        "release_status" : "stable",
        "min_ksp" : "0.24.2",
        "max_ksp" : "0.24.2",
        "requires" : [
            { "name" : "RealSolarSystemTextures" }
        ],
        "recommends" : [
            { "name" : "RealismOverhaul" }
        ],
        "resources" : {
            "homepage" : "http://forum.kerbalspaceprogram.com/threads/55145",
            "github"   : {
                "url"      : "https://github.com/NathanKell/RealSolarSystem",
                "releases" : true
            }
        },
        "install" : [
            {
                "file"       : "RealSolarSystem",
                "install_to" : "GameData"
            }
        ],
        "bundles" : [
            {
                "file"       : "ModuleManager.2.3.3.dll",
                "identifier" : "ModuleManager",
                "version"    : "2.3.3",
                "install_to" : "GameData",
                "license"    : "CC-BY-SA",
                "required"   : true
            },
            {
                "file"       : "CustomBiomes",
                "identifier" : "CustomBiomes",
                "version"    : "1.6.6",
                "install_to" : "GameData",
                "license"    : "CC-BY-NC-SA",
                "required"   : false
            }
        ]
    }

### Metadata description

The metadata file provides machine-readable information about a
distribution.

#### Mandatory fields

##### spec_version

The version number of the CKAN specification used to create this .ckan file. The
value of this field is an unsigned integer. The currently latest version of the
spec is `1`.

##### name

This is the human readable name of the mod, and may contain any
printable characters. Eg: "Ferram Aërospace Research (FAR)",
"Real Solar System".

##### abstract

A human readable description of the mod and what it does.

##### identifer

This is the gloablly unique identifier for the mod, and is how the mod
will be referred to by other CKAN documents.  It may only consist of
letters, numbers, underscores, and minus signs. Eg: "FAR" or
"RealSolarSystem". This is the identifier that will be used whenever
the mod is referenced (by `depends`, `conflicts`, or elsewhere).

If the mod would generate a `FOR` pass in ModuleManager, then the
identifier *must* be same as the ModuleManager name.

##### download

A fully formed URL, indicating where a machine may download the
described version of the mod.

##### license

The license, or list of licenses, under which the mod is released.
The following are valid license strings:

- CC-BY
- CC-BY-SA
- CC-BY-ND
- CC-BY-NC
- CC-BY-NC-SA
- CC-BY-NC-ND
- GPLv1
- GPLv2
- GPLv3
- BSD
- MIT
- LGPLv2.1
- LGPLv3

The following license strings are also valid and indicate other licensing not
described above:

- `open_source`: Other Open Source Initiative (OSI) approved license
- `restricted`: Requires special permission from copyright holder
- `unrestricted`: Not an OSI approved license, but not restricted
- `unknown`: License not provided in metadata

A single license, or list of licenses may be provided. The following
are both valid, the first describing a mod released under the BSD license,
the second under the user's choice of BSD or MIT licenses.

    "license" : "BSD"

    "license" : [ "BSD", "MIT" ]

##### version

The version of the mod. Versions have the format `[epoch]:mod_version`.

###### epoch

`epoch` is a single (generally small) unsigned integer. It may be omitted, in
which case zero is assumed.

It is provided to allow mistakes in the version numbers of older versions of a
package, and also a package's previous version numbering schemes, to be left
behind.

###### mod_version

`mod_version` is the main part of the version number. It is usually the version
number of the original mod from which the CKAN file is created. Usually this
will be in the same format as that specified by the mod author(s); however, it
may need to be reformatted to fit into the package management system's format
and comparison scheme.

The comparison behavior of the package management system with respect to the
`mod_version` is described below. The `mod_version` portion of the version
number is mandatory.

The `mod_version` may contain only alphanumerics and the characters `.` `+`
(full stop, plus) and should start with a digit.

###### version ordering

When comparing two version numbers, first the `epoch` of each are compared, then
the `mod_version` if `epoch` is equal. `epoch` is compared numerically. The
`mod_version` part is compared by the package management system using the
following algorithm:

The strings are compared from left to right.

First the initial part of each string consisting entirely of non-digit
characters is determined. These two parts (one of which may be empty) are
compared lexically. If a difference is found it is returned. The lexical
comparison is a comparison of ASCII values modified so that all the letters sort
earlier than all the non-letters.

Then the initial part of the remainder of each string which consists entirely of
digit characters is determined. The numerical values of these two parts are
compared, and any difference found is returned as the result of the
comparison. For these purposes an empty string (which can only occur at the end
of one or both version strings being compared) counts as zero.

These two steps (comparing and removing initial non-digit strings and initial
digit strings) are repeated until a difference is found or both strings are
exhausted.

Note that the purpose of epochs is to allow us to leave behind mistakes in
version numbering, and to cope with situations where the version numbering
scheme changes. It is not intended to cope with version numbers containing
strings of letters which the package management system cannot interpret (such as
ALPHA or pre-), or with silly orderings.

##### install

A list of install directives for this mod, each must contain the two
mandatory directives: 

- `file`: The file or directory root that this directive pertains to.
  All leading directories are stripped from the start of the filename
  during install. (Eg: `MyMods/KSP/Foo` will be installed into
  `GameData/Foo`.)
- `install_to`: The location where this section should be installed.
  Presently the only valid value for this entry is `GameData`. Paths
  will be preserved, 

An install directive may also include the following optional fields:

- `requires`: Indicates this install directive should only be triggered
  if the required mod has already been installed.
- `overwrite`: A boolean value, if set to true, then this allows files
  to be overwritten during install, even if those files are from another
  mod.

An example set of install directives, including one that overwrites
parts of `OtherMod` (if installed) is shown below:

    "install" : [
        {
            "file"       : "GameData/ExampleMod",
            "install_to" : "GameData"
        },
        {
            "file"       : "GameData/OtherMod",
            "install_to" : "GameData",
            "requires"   : "OtherMod",
            "overwrite"  : true
        }
    ]

#### Optional fields

##### comment

A comment field, if included, is ignored. It is not displayed to users,
nor used by programs. It's primary use is to convey information to humans
examining the CKAN file manually

##### author

The author, or list of authors, for this mod. No restrictions are
placed upon this field.

##### release_status

The release status of the mod, one of `alpha`, `beta`, `stable`,
or `development`. If not specified, a value of `stable` is assumed.

##### min_ksp

The minimum version of KSP the mod requires to operate correctly.
Eg `0.23.5`. If not specified, a default value of `any` is assumed.

##### max_ksp

The maximum version of KSP the mod requires to operate correctly.
Eg `0.24.2`. If not specified, a default value of `any` is assumed.

##### requires

A list of mods which are *required* for the current mod to operate.
At its most basic, this is an array of objects, each being a name
and identifier:

    "requires" : [
        { "name" : "ModuleManager" },
        { "name" : "RealFuels" },
        { "name" : "RealSolarSystem" }
    ]

Each object may also contain the optional fields `min_version`, `max_version`,
and `version`, to more precisely describe which vesions are needed:

    "requires" : [
        { "name" : "ModuleManager",   "min_version" : "2.1.5" },
        { "name" : "RealSolarSystem", "min_version" : "7.3"   },
        { "name" : "RealFuels" }
    ]

It is an error to mix `version` (which specifies an exact vesion) with
either `min_version` or `max_version` in the same object.

##### recommends

A list of mods which are *recommended* by this mod for an optimal
playing experience. This uses the same format as the `requires` field
above.

##### conflicts

A list of mods which *conflict* with this mod. This uses the same format
as the `requires` field above.

##### resources

The `resources` field describes additional information that a user or
program may wish to know about the mod, but which are not required
for its installation or indexing. Presently the following fields
are described:

- `homepage` is a URL that goes to the preferred landing page for the mod.
- `github` is an object which *must* contain a `url` pointing to the
  github page for the project. It *may* include a `releases` key
  with a boolean value (which defaults to false) indicating if github releases
  should be used when searching for updates.

Example resources:

    "resources" : {
        "homepage" : "http://examele.com/jebinator",
        "github"   : {
            "url"      : "http://github.com/example/jebinator",
            "releases" : "true"
        }
    }

##### bundles

A list of mods which are *included* with this mod distribution, each
of which is an object with the following mandatory fields:

- `file`: A path to the bundled mod.
- `identifier`: The identifier of the bundled mod.
- `version`: The version of the bundled mod.
- `install_to` Where the bundled mod should be installed to.
  (Currently `GameData` is the only valid value.)
- `license`: The license or list of licenses which allowed this mod to be
  bundled.
- `required`: Whether this mod is required for operation.

Bundled mods should *not* be installed if a later version of the same
mod is already installed.

As an example, here is a `bundles` section which includes both
ModuleManager and CustomBiomes

    "bundles" : [
        {
            "file"       : "ModuleManager.2.3.3.dll",
            "identifier" : "ModuleManager",
            "version"    : "2.3.3",
            "install_to" : "GameData",
            "license"    : "CC-BY-SA",
            "required"   : true
        },
        {
            "file"       : "CustomBiomes",
            "identifier" : "CustomBiomes",
            "version"    : "1.6.6",
            "install_to" : "GameData",
            "license"    : "CC-BY-NC-SA",
            "required"   : false
        }
    ]

##### provides

An identifier, or list of identifiers, that this module *provides*. This field
is intended for use in modules which require one of a selection of texture
downloads, or one of a selection of mods which provide equivalent
functionality.  For example:

    "provides"  : "RealSolarSystemTextures"

It is recommended that this field be used *sparingly*, as all mods with
the same `provides` string are essentially declaring they can be used
interchangably.

It *is* considered acceptable to use this field if a mod is renamed,
and the old name of the mod is listed in the `provides` field. This
allows for mods to be renamed without updating all other mods which
depend upon it.
