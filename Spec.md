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
files are simply JSON files using UTF-8 as character-encoding.

Except where stated otherwise all strings *should* be printable unicode only.


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
        "tags" : [
            "physics",
            "resources",
            "atmospheric",
            "engines",
            "nasa-enginesim",
            "b9-turbofans"
        ],
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

This document describes the CKAN specification 'v1.20'. Changes since spec `1`
are marked with **v1.2** through to **v1.20** respectively. For maximum
compatibility, using older spec versions is preferred when newer features are
not required.

##### name

This is the human readable name of the mod, and may contain any
printable characters. Eg: "Ferram Aërospace Research (FAR)",
"Real Solar System".

##### abstract

A short, one line description of the mod and what it does.

##### identifier

This is the globally unique identifier for the mod, and is how the mod
will be referred to by other CKAN documents. It may only consist of ASCII-letters, ASCII-digits and `-` (dash). Eg: "FAR" or
"RealSolarSystem". This is the identifier that will be used whenever
the mod is referenced (by `depends`, `conflicts`, or elsewhere).

Identifiers must be both: case sensitive for machines, and unique regardless of capitalization for human consumption and case-ignorant systems. Example: MyMod must always be expressed as MyMod, but another module
cannot assume the mymod identifier.

If the mod would generate a `FOR` pass in ModuleManager, then the
identifier *should* be same as the ModuleManager name. For most mods,
this means the identifier *should* be the name of the directory in
`GameData` in which the mod would be installed, or the name of the `.dll`
with any version and the `.dll` suffix removed.

##### download

A fully formed URL, indicating where a machine may download the
described version of the mod. Note: This field is not required if the `kind` is `metapackage`.

##### license

The license (**v1.0**), or list of licenses (**v1.8**), under which the mod is released.
The same rules as per the
[Debian license specification](https://www.debian.org/doc/packaging-manuals/copyright-format/1.0/#license-specification) apply, with the following modifications:

* The `MIT` license is always taken to mean the [Expat license](https://www.debian.org/legal/licenses/mit).
* The creative commons licenses are permitted without a version number, indicating the
  author did not specify which version applies.
* Stripping of trailing zeros is not recognised.
* (**v1.2**) `WTFPL` is recognised as a valid license.
* (**v1.18**) `Unlicense` is recognised as a valid license.

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
encouraged to restrict version names to ASCII-letters, ASCII-digits, and the characters `.` `+` `-` `_`
(full stop, plus, dash, underscore) and should start with a digit.

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
  before the regular expression is run.
  *Use sparingly and with caution*, it's *very* easy to match the wrong
  thing with a regular expression.

In addition a destination directive *must* be provided:

- `install_to`: The target location where the matched file or directory should be installed.
  -  Valid values for this entry are `GameData`, `Ships`, `Ships/SPH`(**v1.12**), `Ships/VAB`(**v1.12**), `Ships/@thumbs/VAB`(**v1.16**), `Ships/@thumbs/SPH`(**v1.16**), `Tutorial`, `Scenarios` (**v1.14**)
  and `GameRoot` (which should be used sparingly, if at all).
  -  A path to a given subfolder location can be specified *only* under `GameData` (**v1.2**);
  for example: `GameData/MyMod/Plugins`. The client *must* check this path and abort the install
  if any attempts to traverse up directories are found (eg: `GameData/../Example`).
  -  Subfolder paths under a matched directory will be preserved, but directories will *only*
  be created when installing to `GameData`, `Tutorial`, or `Scenarios`.

In addition, any number of optional directives *may* be provided:

- `as` : (**v1.18**) The name to give the matching directory or file when installed. Allows renaming directories or
  files.
- `filter` : A string, or list of strings, of file parts that should *not*
  be installed. These are treated as literal things which must match a
  file name or directory. Examples of filters may be `Thumbs.db`,
  or `Source`. Filters are considered case-insensitive.
- `filter_regexp` : A string, or list of strings, which are treated as
  case-sensitive C# regular expressions which are matched against the
  full paths from the installing zip-file. If a file matches the regular
  expression, it is *not* installed.
- `include_only` : (**v1.24**) A string, or list of strings, of file parts that should
  be installed. These are treated as literal things which must match a
  file name or directory. Examples of this may be `Settings.cfg`,
  or `Plugin`. These are considered case-insensitive.
- `include_only_regexp` : (**v1.24**) A string, or list of strings, which are treated as
  case-sensitive C# regular expressions which are matched against the
  full paths from the installing zip-file. If a file matches the regular
  expression, it is installed.
- `find_matches_files` : (**v1.16**) If set to `true` then both `find` and
  `find_regexp` will match files in addition to directories.

If no install sections are provided, a CKAN client *must* find the
top-most directory in the archive that matches the module identifier,
and install that with a target of `GameData`.

A typical install directive only has `file` and `install_to` sections:

    "install" : [
        {
            "file"       : "GameData/ExampleMod",
            "install_to" : "GameData"
        }
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

The version of KSP this mod is targeting. This may be the string "any",
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

##### ksp_version_strict

(**v1.16**)

If `true`, the mod will only be installed if the user's KSP version is
exactly targeted by the mod.

If `false`, the mod will be installed if the KSP version it targets is
"generally recognised" as being compatible with the KSP version
the user has installed. It is up to the CKAN client to determine what is
"generally recognised" as working.

As an example, a mod with a `ksp_version` of `1.0.3` will also install in
KSP `1.0.4` (but not any other version) when `ksp_version_strict` is false.

This field defaults to `false`, including for `spec_version`s less than
`v1.16`, however CKAN clients prior to `v1.16` would only perform strict
checking.

##### tags

(**v<TBA>**) The `tags` field describes keywords that a user or program may
use to classify or filter the mod in a list, but which are not required.
These may include general tags which define how the mod interacts with or
alters KSP or specific tags defining what has been added or changed from
stock gameplay. Tags may contain lowercase alphanumeric characters or
hyphens.

Example tags:

    "tags" : [
        "physics",
        "parts",
        "oceanic",
        "thermal",
        "science",
        "my-1-custom-tag"
    ]

The first defined keyword listed in the tag field which matches one of a
predefined set of 'standard' tags should be considered the most important
and used to populate a sortable column in online indices, though it is
intended that all included tags will be indexed and searchable.

Tags have not yet been implemented in the client. They can be added to
.ckan/.netkan files, but will not be displayed in the client yet.

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
and `version`, may more precisely describe which versions are needed:

    "depends" : [
        { "name" : "ModuleManager",   "min_version" : "2.1.5" },
        { "name" : "RealSolarSystem", "min_version" : "7.3"   },
        { "name" : "RealFuels" }
    ]

It is an error to mix `version` (which specifies an exact version) with
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

##### supports

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
- `spacedock` : The mod on SpaceDock.
- `curse` :  (**v1.20**) The mod on Curse.
- `manual` : The mod's manual, if it exists.

Example resources:

    "resources" : {
        "homepage"     : "https://tinyurl.com/DogeCoinFlag",
        "bugtracker"   : "https://github.com/pjf/DogeCoinFlag/issues",
        "repository"   : "https://github.com/pjf/DogeCoinFlag",
        "ci"           : "https://ksp.sarbian.com/jenkins/DogecoinFlag"
        "spacedock"    : "https://spacedock.info/mod/269/Dogecoin%20Flag"
        "curse"        : "https://kerbal.curseforge.com/projects/220221"
    }

While all currently defined resources are all URLs, future revisions of the spec may provide for more complex types.

It is permissible to have fields prefixed with an `x_`. These are considered
custom use fields, and will be ignored. For example:

    "x_twitter" : "https://twitter.com/pjf"

#### Special use fields

These fields are optional, and should only be used with good reason.
Typical mods *should not* include these special use fields.

##### kind

Specifies the type of package the .ckan file delivers. This field defaults to `package`, the other option (and presently the only time the field is explicitly declared) is `metapackage`. Metapackages allow for a distributable .ckan file that has relationships to other mods while having no `download` of its own. **v1.6**

##### provides

A list of identifiers, that this module *provides*. This field
is intended for use in modules which require one of a selection of texture
downloads, or one of a selection of mods which provide equivalent
functionality.  For example:

    "provides"  : [ "RealSolarSystemTextures" ]

It is recommended that this field be used *sparingly*, as all mods with
the same `provides` string are essentially declaring they can be used
interchangeably.

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
downloading from the `download` URL. It is recommended that this field is
only generated by automated tools (where it is encouraged),
and not filled in by hand.

##### download_hash

If supplied, `download_hash` is an object of hash digests. Currently
SHA1 and SHA256 calculated hashes of the resulting file downloaded.
It is recommended that this field is only generated by automated
tools (where it is encouraged), and not filled in by hand.

    "download_hash": {
        "sha1": "1F4B3F21A77D4A302E3417A7C7A24A0B63740FC5",
        "sha256": "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855"
    }

##### download_content_type

If supplied, `download_content_type` is the content type of the file
downloaded from the `download` URL. It is recommended that this field is
only generated by automated tools (where it is encouraged),
and not filled in by hand.

    "download_content_type": "application/zip"

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

#### NetKAN Fields

NetKAN is the name the tool which is used to automatically generate CKAN files from a variety of sources. NetKAN
consumes `.netkan` files to produce `.ckan` files. `.netkan` files are a *strict superset* of `.ckan` files. Every
`.ckan` file is a valid `.netkan` file but not vice versa. NetKAN uses the following fields to produce `.ckan` files.

##### `$kref`

The `$kref` field indicates that data should be filled in from an external service provider. The following `$kref`
values are understood. Only *one* `$kref` field may be present in a `.netkan` file.

###### `#/ckan/spacedock/:sdid`

Indicates that data should be fetched from SpaceDock, using the `:sdid` provided. For example: `#/ckan/spacedock/269`.

When used, the following fields will be auto-filled if not already present:

- `name`
- `license`
- `abstract`
- `author`
- `version`
- `download`
- `download_size`
- `download_hash`
- `download_content_type`
- `resources.homepage`
- `resources.spacedock`
- `resources.repository`
- `resources.x_screenshot`
- `ksp_version`

###### `#/ckan/curse/:cid`

Indicates that data should be fetched from Curse, using the `:cid` provided. For example: `#/ckan/curse/220221`.

When used, the following fields will be auto-filled if not already present:

- `name`
- `license`
- `author`
- `version`
- `download`
- `download_size`
- `download_hash`
- `download_content_type`
- `resources.curse`
- `ksp_version`

###### `#/ckan/github/:user/:repo[/asset_match/:filter_regexp]`

Indicates that data should be fetched from GitHub, using the `:user` and `:repo` provided.
For example: `#/ckan/github/pjf/DogeCoinFlag`.

When used, the following fields will be auto-filled if not already present:

- `name`
- `abstract`
- `author`
- `version`
- `download`
- `download_size`
- `download_hash`
- `download_content_type`
- `resources.repository`

Optionally, one asset `:filter_regexp` directive *may* be provided:

- `filter_regexp`: A string which is treated as  case-sensitive C# regular expressions which are matched against the
  name of the released artifact.

An `x_netkan_github` field may be provided to customize how the metadata is fetched from GitHub. It is an `object` with the following fields:

- `use_source_archive` (type: `boolean`) (default: `false`)<br/>
  Specifies that the source ZIP of the repository itself will be used instead of any assets in the release.

###### `#/ckan/jenkins/:joburl`

Indicates data should be fetched from a [Jenkins CI server](http://jenkins-ci.org/) using the `:joburl` provided. For
example: `#/ckan/jenkins/https://jenkins.kspmods.example/job/AwesomeMod/`.

The following fields will be auto-filled if not already present:

- `version`
- `download`
- `download_size`
- `download_hash`
- `download_content_type`
- `resources.ci`

An `x_netkan_jenkins` field may be provided to customize how the metadata is fetched from the Jenkins server. It is
an `object` with the following fields:

- `build` (type: `string`, enumerated) (default: `"stable"`)<br/>
   Specifies the type of build to use. Possible values are `"any"`, `"completed"`, `"failed"`, `"stable"`,
   `"successful"`, `"unstable"`, or `"unsuccessful"`. Many of these values do not make sense to use in practice but
   are provided for completeness.
- `asset_match` (type: `string`, regex) (default: `"\\.zip$"`)<br/>
  Specifies a regex which selects which artifact to use by filename (case-insensitively). Not having exactly one
  matching asset is an error.
- `use_filename_version` (type: `boolean`, default: `false`)<br/>
  Specifies if the filename of the matched artifact should be used as the value of the `version` property. Combined
  with the `x_netkan_version_edit` property this allows the version to be extracted from the filename itself.
  Otherwise the expectation is that the archive will have an AVC `.version` file which will be used to generate the
  `version` value.

If any options are not present their default values are used.

An example `.netkan` excerpt:
```json
{
    "$kref": "#/ckan/jenkins/https://jenkins.kspmods.example/job/AwesomeMod/",
    "x_netkan_jenkins": {
        "build": "stable",
        "asset_match": "\\.zip$",
        "use_filename_version": false
    }
}
```
###### `#/ckan/http/:url`


Indicates data should be fetched from a HTTP server, using the `:url` provided. For example: `#/ckan/http/https://ksp.marce.at/Home/DownloadMod?modId=2`.

When used, the following fields will be auto-filled if not already present:

- `download`
- `download_size`
- `download_hash`
- `download_content_type`

This method depends on the existence of an AVC `.version` file in the download file
to determine:

- `version`

###### `#/ckan/netkan/:url`

Indicates that data should be fetched from another `.netkan` file hosted remotely.
For example: `#/ckan/netkan/https://www.kspmods.example/AwesomeMod.netkan`.

The remote `.netkan` file is downloaded and used as if it were the original. `.netkan` files which contain such a
reference are known as *recursive netkans* or *metanetkans*. They are primarily used so that mod authors can provide
authoritative metadata.

The following conditions apply:
- A metanekan may not reference another metanetkan, otherwise an error is produced.
- Any fields specified in the metanetkan will override any fields in the target netkan file.

An example `.netkan` including all required fields for a valid metanetkan:
```json
{
    "spec_version": 1,
    "identifier": "AwesomeMod",
    "$kref": "#/ckan/netkan/https://www.kspmods.example/AwesomeMod.netkan"
}
```

##### `$vref`

The `$vref` field indicates that version data should be filled in from an external service provider. Only *one*
`$vref` field may be present in a document.

###### `#/ckan/ksp-avc[[/path]/avcfilename.version]`

If present, a `$vref` symbol of `#/ckan/ksp-avc` states that version
information should be retrieved from an embedded KSP-AVC `.version` file in the
file downloaded by the `download` field. The following conditions apply:

* Only `.version` files that would be *installed* for this mod are considered. (In theory. Transformer ordering may cause files outside the installed folders being considered)
* It is an error if more than one `.version` file would be considered.
* It is an error if the `.version` file does not validate according to
  [the KSP-AVC spec](http://ksp.cybutek.net/kspavc/Documents/README.htm).
* The `KSP_VERSION` field for the `.version` file will be ignored if the
  `KSP_VERSION_MIN` and `KSP_VERSION_MAX` fields are set.
* Netkan will first attempt to use anything after `ksp-avc` as a literal
   path within the zip file, and if that fails, will use the string as a
   regexp to search for a matching file to use.

When used, the following fields are auto-generated:

- `ksp_version`
- `ksp_version_min`
- `ksp_version_max`

Version information is generated in such a way as to ensure maximum compatibility. For example if the `.version` file
specifies that the mod is compatible with KSP version `1.0.2` but the existing `version` specifies `1.0.5` then the
version information generated will give a `ksp_version_min` of `1.0.2` and a `ksp_version_max` of `1.0.5`.

If (and only if) no mod version number has been identified (eg a `#/ckan/http/:url`), then the following field will also be auto-generated:

- `version`

##### `x_netkan_epoch`

The `x_netkan_epoch` field is used to specify a particular `epoch` number in the `version` field. Its value should be
an unsigned 32-bit integer.

An example `.netkan` excerpt:
```json
{
    "x_netkan_epoch": 1
}
```

##### `x_netkan_force_v`

The `x_netkan_force_v` field is used to specify that a `v` should be prepended to the `version` field. It is a
`boolean` field.

A combination of `x_netkan_epoch` and `x_netkan_version_edit` should be used instead to ensure that the `version`
field *only* contains the actual version string.

An example `.netkan` excerpt:
```json
{
    "x_netkan_force_v": true
}
```

##### `x_netkan_version_edit`

The `x_netkan_version_edit` field is used to edit the final value of the `version` field. `x_netkan_version_edit` is
an `object` with the following fields:

- `find` (type: `string`, regex) (default: *none*)<br/>
   A regex to match against the existing `version` field.
- `replace` (type: `string`, regex substitution) (default: `"${version}"`)<br/>
  Specifies a [regex substitution string](https://msdn.microsoft.com/en-us/library/ewy2t5e0%28v=vs.110%29.aspx) which
  will be used as the value of the new `version` field.
- `strict` (type: `boolean`, default: `true`)<br/>
  Specifies if NetKAN should produce an error if `find` fails to produce a match against the `version` field.

`x_netkan_version_edit` can also be a `string` in which case its value is treated as the value the `find` field and
the default values for the `replace` and `strict` fields are used.

An example `.netkan` excerpt:
```json
{
    "$kref": "#/ckan/jenkins/https://jenkins.kspmods.example/job/AwesomeMod/",
    "x_netkan_version_edit": {
        "find": "^[vV]?(?<version>.+)$",
        "replace": "${version}",
        "strict": true
    }
}
```

##### `x_netkan_override`

The `x_netkan_override` field is used to override field values based on the value of the `version` field.
`x_netkan_override` is an `array` of `object`s. Each `object` may have the following fields:

- `version` (type: `array` of `string`, version comparison)<br/>
  An array of version comparison strings that are used to match against `version`. Version comparison strings are of
  the form `"[operator]<version>"` where `operator` is one of `=`, `<`, `>`, `<=`, or `>=`. If no `operator` is given
  it is equivalent to specifying `=`. In order for the override to match *all* the comparisons must be true. Therefore
  a range may be specified as such: `[ ">=1.0", "<2.0" ]`. A `string` may also be specified instead of an `array` in
  which case it is treated as an array with a single element equal to the value of the string.
- `before` (type: `string`, transformation name)<br/>
  The name of a transformation this override to happen directly before.
- `after` (type: `string:, transformation name)<br/>
  The name of a transformation this override to happen directly after.
- `override` (type: `object`)<br/>
  An object whose fields will override the fields already present if a match occurs. No merging of values occurs, the
  values of the fields are entirely replaced.
- `delete` (type: `array` of `string`)<br/>
  An array of strings which are the names of fields to remove if a match occurs.

The possible values of `before` and `after` are:

- `$none`
- `$all`
- `avc`
- `download_attributes`
- `epoch`
- `forced_v`
- `generated_by`
- `github`
- `http`
- `internal_ckan`
- `jenkins`
- `metanetkan`
- `optimus_prime`
- `property_sort`
- `spacedock`
- `curse`
- `strip_netkan_metadata`
- `version_edit`
- `versioned_override`

If no `before` or `after` is specified then the override occurs at a "reasonable" point in the transformation process.
Most overrides should **not** specify a `before` or `after` unless there is a specific need to.

When any metadata changes occur which are version specific, for example a new dependency is added, overrides are the
recommended means of specifying them. Overrides may also be used to *stage* metadata changes, for example when new
dependencies are anticipated to be added in yet unreleased versions of a mod. This allows mod metadata to be
up-to-date as soon as possible without requiring excessive coordination.
