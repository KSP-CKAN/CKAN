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
- The meta-data file *may* be included in the distribution, to facilitate
- easier indexing. CKAN files may be placed anywhere inside a distribution.
- It is an error for a distribution (zipfile) to contain more than one
  CKAN file.

Presently the authoritative CKAN metadata repository is
[hosted on github](https://github.com/KSP-CKAN/CKAN-meta).

## Validation

A [JSON Schema](CKAN.schema) is provided for validation purposes.
Any CKAN file *must* conform to this schema to be considered valid.

## The CKAN file

A CKAN file is designed to contain all the relevant meta-info
about a mod, including its name, license, download location,
dependencies, compatible versions of KSP, and the like. CKAN
files are simply JSON files.

CKAN files *should* have a naming scheme of their mod's identifier,
followed by a dash, followed by the version number, followed by
the extension `.ckan`. For example: `RealSolarSystem-7.3.ckan`.

The CKAN metadata spec is inspired by the
[CPAN metadata spec](https://metacpan.org/pod/CPAN::Meta::Spec),
the [Debian Policy Manual](https://www.debian.org/doc/debian-policy/)
and the
[KSP-RealSolarSystem-Bundler](https://github.com/NathanKell/KSP-RealSolarSystem-Bundler)

### Example CKAN file

    {
        "spec_version"   : 1,
        "name"           : "Advanced Jet Engine (AJE)",
        "abstract"       : "Realistic jet engines for KSP",
        "identifier"     : "AJE",
        "download"       : "https://github.com/camlost2/AJE/archive/1.6.zip",
        "license"        : "LGPLv2.1",
        "version"        : "1.6",
        "release_status" : "stable",
        "ksp_version"    : "0.25",
        "resources" : {
            "homepage" : "http://forum.kerbalspaceprogram.com/threads/70008",
            "github"   : {
                "url"      : "https://github.com/camlost2/AJE",
                "releases" : true
            }
        },
        "install" : [
            {
                "file"       : "AJE-1.6/GameData",
                "install_to" : "GameData"
            }
        ],
        "depends" : [
            { "name" : "FerramAerospaceResearch" },
            { "name" : "ModuleManager", "min_version" : "2.3.5" }
        ],
        "recommends" : [
            { "name" : "RealFuels" },
            { "name" : "HotRockets" }
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
The same rules as per the
[debian license specification](https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/#license-specification) apply, with the following modifications:

* The `MIT` license is always taken to mean the [Expat license](https://www.debian.org/legal/licenses/mit).
* The creative commons licenses are permitted without a version number, indicating the
  author did not specify which version applies.
* Stripping of trailing zeros is not recognised.

The following license strings are also valid and indicate other licensing not
described above:

- `open-source`: Other Open Source Initiative (OSI) approved license
- `restricted`: Requires special permission from copyright holder
- `unrestricted`: Not an OSI approved license, but not restricted
- `unknown`: License not provided in metadata

A single license, or list of licenses may be provided. The following
are both valid, the first describing a mod released under the BSD license,
the second under the user's choice of BSD-2-clause or GPL-2.0 licenses.

    "license" : "BSD-2-clause"

    "license" : [ "BSD-2-clause", "GPL-2.0" ]

##### version

The version of the mod. Versions have the format `[epoch:]mod_version`.

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
  Valid values for this entry are `GameData`, `Ships`, `Tutorial`,
  and `GameRoot` (which should be used sparingly, if at all).
  Paths will be preserved, but directories will *only*
  be created when installing to `GameData` or `Tutorial`.

An install directive may also include the following optional fields:

- `depends`: Indicates this install directive should only be triggered
  if the required mod has already been installed.
- `overwrite`: A boolean value, if set to true, then this allows files
  to be overwritten during install, even if those files are from another
  mod.
- `optional`: A boolean value. If set to true, this component is considered
  optional. Defaults to false.
- `description`: A human readable description of this component.
  Recommeded for any optional sections.

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
            "depends"    : "OtherMod",
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

##### ksp_version

The version of KSP this mod is targetting. This may be the string "any",
a number (eg: `0.23.5`) or may only contain the first two parts of
the version strin (eg: `0.25`). In the latter case, any realease
starting with the `ksp_version` is considered acceptable.

If no KSP target version is included, a default of "any" is assumed.

##### ksp_version_min

The minimum version of KSP the mod requires to operate correctly.
Same format as `ksp_version`. It is an error to include both this
and the `ksp_version` field.

##### ksp_version_max

The maximum version of KSP the mod requires to operate correctly.
Same format as `ksp_version`. It is an error to include both this
and the `ksp_version` field.

### Relationships

Relationships are optional fields which describe this mod's relationship
to other mods. They can be used to ensure that a mod is installed with
one of its graphics packs, or two mods which conflicting functionality
are not installed at the same time.

At its most basic, this is an array of objects, each being a name
and identifier:

    "depends" : [
        { "name" : "ModuleManager" },
        { "name" : "RealFuels" },
        { "name" : "RealSolarSystem" }
    ]

Each relationship is an array of entries, each entry *must*
have a `name`, and the optional fields `min_version`, `max_version`,
and `version`, to more precisely describe which vesions are needed:

    "depends" : [
        { "name" : "ModuleManager",   "min_version" : "2.1.5" },
        { "name" : "RealSolarSystem", "min_version" : "7.3"   },
        { "name" : "RealFuels" }
    ]

It is an error to mix `version` (which specifies an exact vesion) with
either `min_version` or `max_version` in the same object.

##### depends

A list of mods which are *required* for the current mod to operate.
This mods *must* be installed along with the urrent mod being installed.

##### pre_depends

A list of mods which *must* be installed *before* the current mod is
installed. This should be very *sparingly*, but is sometimes required
when one mod overwrites part of another mod.

##### recommends

A list of mods which are *recommended*, but not required, for a typical
user. This is a strong recommendation, and recommended mods *will* be installed
unless the user requests otherwise.

##### suggests

A list of mods which are suggested for installation alongside this mod.
This is a weak recommendation, and by default these mods *will not* be
installed unless the user requests otherwise.

##### conflicts

A list of mods which *conflict* with this mod. The current mod
*will not* be installed if any of these mods are already on the system.

##### resources

The `resources` field describes additional information that a user or
program may wish to know about the mod, but which are not required
for its installation or indexing. Presently the following fields
are described:

- `homepage` is a URL that goes to the preferred landing page for the mod.
- `bugtracker` is a URL that goes to the mod's bugtracker if it exists.
- `github` is an object which *must* contain a `url` pointing to the
  github page for the project. It *may* include a `releases` key
  with a boolean value (which defaults to false) indicating if github releases
  should be used when searching for updates.
- `kerbalstuff` is an object which *must* contain a `url` pointing to the
  mod hosted on KerbalStuff.

Example resources:

    "resources" : {
        "homepage"   : "http://tinyurl.com/DogeCoinFlag",
        "bugtracker" : "https://github.com/pjf/DogeCoinFlag/issues",
        "github"   : {
            "url"      : "http://github.com/pjf/DogeCoinFlag",
            "releases" : "true"
        },
        "kerbalstuff"  : {
            "url"      : "https://kerbalstuff.com/mod/269/Dogecoin%20Flag"
        }
    }

#### Special use fields

These fields are optional, and should only be used with good reason.
Typical mods *should not* include these special use fields.

##### bundles

Where possible, it is recommended to use relationships (
eg: *depends*, *recommended*, and *suggests*) rather than bundles. This ensures
that mods are installed from their authoritative source, and means that
related mods are installed in a known, reproduceable state. It
also allows the most recent version of a related mod to be installed,
which is important in ensuring bugfixes and features can be deployed
in a timely fashion.

Even if your distribution does bundle an additional mod, it is still
recommended that you use relationships, rather than require the CKAN
to install the bundle.

However if required, a list of bundles definitions may be provided,
which describe mods which are included with this distribution. In
this case, the following fields are mandatory:

- `file`: A path to the bundled mod.
- `identifier`: The identifier of the bundled mod.
- `version`: The version of the bundled mod.
- `install_to` Where the bundled mod should be installed to.
  (Currently `GameData` is the only valid value.)
- `license`: The license or list of licenses which allowed this mod to be
  bundled.
- `required`: Whether this mod is required for operation.

Bundled mods *will not* be installed if the same or later version of the
mod is already installed.

As an example, here is a `bundles` section which includes both
ModuleMunger and CustomBiomes

    "bundles" : [
        {
            "file"       : "ModuleMunger.2.3.3.dll",
            "identifier" : "ModuleMunger",
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

##### download_size

If supplied, `download_size` is the number of bytes to expect when
downloading from the `download` URL. It is recommended this this field
only be generated by automated tools (where it is encouraged),
and not filled in by hand.

#### Extensions

Any field starting with `x_` (an x, followed by an underscore) is considered
an *extension field*. The CKAN tool-chain will *ignore* any such fields.
These fields may be used to include additional machine or human-readable
data in the files.

For example, one may have an `x_maintained_by` field, to indicate the
maintainer of a CKAN file, or an `x_generated_by` field to indicate
it's the result of a custom build process.

Extension fields are unrestricted, and may contain any sort of data,
including lists and objects.

#### Special use fields

##### $kref

The `$kref` field is a special use field that indicates that data
should be filled in from an external service provider. Documents
containing the `$kref` field are *not* valid CKAN files, but they
may be used by external tools to *generate* valid CKAN files.

For example:

    "$kref" : "#/ckan/kerbalstuff"

The following `$kref` values are understood. Only *one* `$kref`
field may be present in a document.

###### #/ckan/kerbalstuff

Indicates that data should be fetched from KerbalStuff. When used,
the following fields will be auto-filled if not already present:

- name
- license
- abstract
- author
- version
- download
- download_size
- homepage
- resources/kerbalstuff
- ksp_version

###### #/ckan/github

Indicates data should be fetched from Github. When used, the following
fields will be auto-filled if not already present:

- author
- version
- download
- download_size
- resources/github
