# The Comprehensive Kerbal Archive Network (CKAN)

This is a Request For Comments on the Comprehensive Kerbal Archive
Network (CKAN). Please open discussions in the issues list, and
send pull requests to patch this document and associated files.

This document, and all associated files, are licensed under the MIT/Expat
license.

The key words "*must*", "*must not*", "*required*", "*shall*", 
"*shall not*", "*should*", "*should not*", "*recommended*", "*may*" and 
"*optional*" in this document are to be interpreted as described in 
[RFC 2119](https://www.ietf.org/rfc/rfc2119.txt).

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
  easier indexing. CKAN files may be placed anywhere inside a distribution.
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
        "license"        : "LGPL-2.1",
        "version"        : "1.6",
        "release_status" : "stable",
        "ksp_version"    : "0.25",
        "resources" : {
            "homepage"     : "http://forum.kerbalspaceprogram.com/threads/70008",
            "repository"   : "https://github.com/camlost2/AJE"
        },
        "install" : [
            {
                "file"       : "AJE-1.6",
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

The version number of the CKAN specification used to create this .ckan file.

A `vx.x` string (eg: `"v1.2"`), being the minimum version of the
reference CKAN client that will read this file.

For compatibility with pre-release clients, and the v1.0 client, the special
*integer* `1` should be used.

This document describes the CKAN specification 'v1.10'. Changes since spec `1`
are marked with **v1.2** through to **v1.8** respectively. For maximum
compatibility, using older spec versions is preferred when newer features are
not required.

##### name

This is the human readable name of the mod, and may contain any
printable characters. Eg: "Ferram Aërospace Research (FAR)",
"Real Solar System".

##### abstract

A short, one line description of the mod and what it does.

##### identifer

This is the gloablly unique identifier for the mod, and is how the mod
will be referred to by other CKAN documents.  It may only consist of
letters, numbers and dashes. Eg: "FAR" or
"RealSolarSystem". This is the identifier that will be used whenever
the mod is referenced (by `depends`, `conflicts`, or elsewhere).

If the mod would generate a `FOR` pass in ModuleManager, then the
identifier *should* be same as the ModuleManager name. For most mods,
this means the identifier *should* be the name of the directory in
`GameData` in which the mod would be installed, or the name of the `.dll`
with any version and the `.dll` suffix removed.

##### download

A fully formed URL, indicating where a machine may download the
described version of the mod.

##### license

The license (**v1.0**), or list of licenses (**v1.8**), under which the mod is released.
The same rules as per the
[debian license specification](https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/#license-specification) apply, with the following modifications:

* The `MIT` license is always taken to mean the [Expat license](https://www.debian.org/legal/licenses/mit).
* The creative commons licenses are permitted without a version number, indicating the
  author did not specify which version applies.
* Stripping of trailing zeros is not recognised.
* (**v1.2**) `WTFPL` is recognised as a valid license.

The following license strings are also valid and indicate other licensing not
described above:

- `open-source`: Other Open Source Initiative (OSI) approved license
- `restricted`: Requires special permission from copyright holder
- `unrestricted`: Not an OSI approved license, but not restricted
- `unknown`: License not provided in metadata

A single license (**v1.0**) , or list of licenses (**v1.8**) may be provided. The following
are both valid, the first describing a mod released under the BSD license,
the second under the *user's choice* of BSD-2-clause or GPL-2.0 licenses.

    "license" : "BSD-2-clause"

    "license" : [ "BSD-2-clause", "GPL-2.0" ]

If different assets in the mod have different licenses, the *most restrictive*
license should be specified, which may be `restricted`.

A future version of the spec may provide for per-file licensing declarations.

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

While the CKAN will accept *any* string as a `mod_version`, mod authors are
encouraged to restrict version names to alphanumerics and the characters `.`
`+` (full stop, plus), and should start with a digit.

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

#### Optional fields

##### install

A list of install directives for this mod, each *must* contain exactly one of
three source directives:

- `file`: The file or directory root that this directive pertains to.
  All leading directories are stripped from the start of the filename
  during install. (Eg: `MyMods/KSP/Foo` will be installed into
  `GameData/Foo`.)
- `find`: (**v1.4**) Locate the top-most directory which exactly matches
  the name specified. This is particularly useful when distributions
  have structures which change based upon each release.
- `find_regexp`: (**v1.10**) Locate the top-most directory which matches
  the specified regular expression. This is particularly useful when
  distributions have structures which change based upon each release, but
  `find` cannot be used because multiple directories or files contain the
  same name. Directories separators will have been normalised to
  forward-slashes first, and the trailing slash for each directory removed
  before the regular expresssion is run.
  *Use sparingly and with caution*, it's *very* easy to match the wrong
  thing with a regular expression.

In addition a destination directive *must* be provided:

- `install_to`: The location where this section should be installed.
  Valid values for this entry are `GameData`, `Ships`, `Tutorial`,
  and `GameRoot` (which should be used sparingly, if at all).
  Paths will be preserved, but directories will *only*
  be created when installing to `GameData` or `Tutorial`.

(**v1.2**) For `GameData` *only* one *may* specify the path to a specific
subfolder; for example: `GameData/MyMod/Plugins`. The client *must* check this
path and abort the install if any attempts to traverse up directories are found
(eg: `GameData/../Example`).

Optionally, one or more filter directives *may* be provided:

- `filter` : A string, or list of strings, of file parts that should not
  be installed. These are treated as literal things which must match a
  file name or directory. Examples of filters may be `Thumbs.db`,
  or `Source`. Filters are considered case-insensitive.
- `filter_regexp` : A string, or list of strings, which are treated as
  case-sensitive C# regular expressions which are matched against the
  full paths from the installing zip-file. If a file matches the regular
  expression, it is not installed.

If no install sections are provided, a CKAN client *must* find the
top-most directory in the archive that matches the module identifier,
and install that with a target of `GameData`.

A typical install directive only has `file` and `install_to` sections:

    "install" : [
        {
            "file"       : "GameData/ExampleMod",
            "install_to" : "GameData"
        },
    ]

##### comment

A comment field, if included, is ignored. It is not displayed to users,
nor used by programs. It's primary use is to convey information to humans
examining the CKAN file manually

##### author

The author, or list of authors, for this mod. No restrictions are
placed upon this field.

##### description

A free form, long text description of the mod, suitable for displaying detailed information about the mod.

##### release_status

The release status of the mod, one of `stable`, `testing` or `development`,
in order of increasing instability.  If not specified, a value of `stable` is
assumed.

##### ksp_version

The version of KSP this mod is targetting. This may be the string "any",
a number (eg: `0.23.5`) or may only contain the first two parts of
the version string (eg: `0.25`). In the latter case, any release
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
have a `name`.

The optional fields `min_version`, `max_version`,
and `version`, may more precisely describe which vesions are needed:

    "depends" : [
        { "name" : "ModuleManager",   "min_version" : "2.1.5" },
        { "name" : "RealSolarSystem", "min_version" : "7.3"   },
        { "name" : "RealFuels" }
    ]

It is an error to mix `version` (which specifies an exact vesion) with
either `min_version` or `max_version` in the same object.

(**v1.0**) Clients implementing versions of the spec older than `v1.8`
*must* allow for the optional version fields to be present, but *may* choose
to treat them as if they were absent. (**v1.8**) Clients implementing the
`v1.8` spec and above *must* respect the optional version fields if
present.

##### depends

A list of mods which are *required* for the current mod to operate.
This mods *must* be installed along with the current mod being installed.

##### recommends

A list of mods which are *recommended*, but not required, for a typical
user. This is a strong recommendation, and recommended mods *will* be installed
unless the user requests otherwise.

##### suggests

A list of mods which are suggested for installation alongside this mod.
This is a weak recommendation, and by default these mods *will not* be
installed unless the user requests otherwise.

#### supports

(**v1.2**) A list of mods which are supported by this mod.  This means that
these mods may not interact or enhance this mod, but they will work correctly
with it. These mods *should not* be installed, this is an informational field
only.

##### conflicts

A list of mods which *conflict* with this mod. The current mod
*will not* be installed if any of these mods are already on the system.

##### resources

The `resources` field describes additional information that a user or
program may wish to know about the mod, but which are not required
for its installation or indexing. Presently the following fields
are described. Unless specified otherwise, these are URLs:

- `homepage` : The preferred landing page for the mod.
- `bugtracker` : The mod's bugtracker if it exists.
- `license` : The mod's license.
- `repository` : The repository where the module source can be found.
- `ci` :  (**v1.6**) Continuous Integration (e.g. Jenkins) Server where the module is being built. `x_ci` is an alias used in netkan.
- `kerbalstuff` : The mod on KerbalStuff.
- `manual` : The mod's manual, if it exists.

Example resources:

    "resources" : {
        "homepage"     : "http://tinyurl.com/DogeCoinFlag",
        "bugtracker"   : "https://github.com/pjf/DogeCoinFlag/issues",
        "repository"   : "http://github.com/pjf/DogeCoinFlag",
        "ci"           : "https://ksp.sarbian.com/jenkins/DogecoinFlag"
        "kerbalstuff"  : "https://kerbalstuff.com/mod/269/Dogecoin%20Flag"
    }

While all currently defined resources are all URLs, future revisions of the spec may provide for more complex types.

It is permissible to have fields prefixed with an `x_`. These are considered
custom use fields, and will be ignored. For example:

    "x_twitter" : "https://twitter.com/pjf"

#### Special use fields

These fields are optional, and should only be used with good reason.
Typical mods *should not* include these special use fields.

##### provides

A list of identifiers, that this module *provides*. This field
is intended for use in modules which require one of a selection of texture
downloads, or one of a selection of mods which provide equivalent
functionality.  For example:

    "provides"  : [ "RealSolarSystemTextures" ]

It is recommended that this field be used *sparingly*, as all mods with
the same `provides` string are essentially declaring they can be used
interchangably.

It *is* considered acceptable to use this field if a mod is renamed,
and the old name of the mod is listed in the `provides` field. This
allows for mods to be renamed without updating all other mods which
depend upon it.

A module may both provide functionality, and `conflict` with the same
functionality. This allows relationships that ensure only one set
of assets are installed. (Eg: `CustomBiomesRSS` and `CustomBiomesKerbal`
both provide and conflict with `CustomBiomesData`, ensuring that both
cannot be installed at the same time.)

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

###### #/ckan/kerbalstuff/:ksid

Indicates that data should be fetched from KerbalStuff, using the `:ksid` provided. For example: `#/ckan/kerbalstuff/269`.

When used, the following fields will be auto-filled if not already present:

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

###### #/ckan/github/:user/:repo[/asset_match/:filter_regexp]

Indicates data should be fetched from Github, using the `:user` and `:repo` provided. For example: `#/ckan/github/pjf/DogeCoinFlag`.

When used, the following fields will be auto-filled if not already present:

- author
- version
- download
- download_size
- resources/repository

Optionally, one asset `:filter_regexp` directive *may* be provided:

- `filter_regexp` : A string which is treated as
  case-sensitive C# regular expressions which are matched against the
  name of the released artifact. An example for this may be found
  in the netkan files for the Active Texture Management and
  Environmental Visual Enhancements addons where multiple zip
  files are uploaded for each version and netkan has to identify
  the correct one.

###### #/ckan/jenkins/:buildname

Indicates data should be fetched from a Jenkins server, using the `:buildname` provided. For example: `#/ckan/jenkins/CrewManifest`.

When used, the resource field `ci` must exist as well, with a fallback to `x_ci`.
Both pieces of information will be used to create an URL to the json documents describing
the build on the target jenkins server.

When used, the following fields will be auto-filled if not already present:

- download
- download_size

When clients are adapted to v1.6, the following field will be auto-filled if not already present:

- resources/ci

###### #/ckan/http/:url

Indicates data should be fetched from a HTTP server, using the `:url` provided. For example: `#/ckan/http/https://ksp.marce.at/Home/DownloadMod?modId=2`.

When used, the following fields will be auto-filled if not already present:

- download
- download_size

This method depends on the existence of an AVC file in the download file
to determine:

- version

##### $vref

The `$vref` field is a special use field that indicates that version
data should be filled in from an external service provider.  Documents
containing the `$vref` field are *not* valid CKAN files, but they
may be used by external tools to *generate* valid CKAN files.

If provided, the data fetched from `$vref` field will overwrite that
povided by a `$kref` expansion.

Only *one* `$vref` field may be present in a document.

###### #/ckan/ksp-avc

If present, a `$vref` symbol of `#/ckan/ksp-avc` states that version
information should be retrieved from an embedded KSP-AVC `.version` file in the
file downloaded by the `download` field. The following conditions apply:

* Only `.version` files that would be *installed* for this mod are considered.
* It is an error if more than one `.version` file would be considered.
* It is an error if the `.version` file does not validate according to
  [the KSP-AVC spec](http://ksp.cybutek.net/kspavc/Documents/README.htm).
* The `KSP_VERSION` field for the `.version` file will be ignored if the
  `KSP_VERSION_MIN` and `KSP_VERSION_MAX` fields are set.

When used, the folowing fields are auto-generated, overwriting those
from `$kref`, but not those specified in the CKAN document itself (if present):

- `ksp_version`
- `ksp_version_min`
- `ksp_version_max`

Future releases of the spec may allow for additional fields to generated.
