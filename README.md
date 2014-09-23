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

### Example Metadata

    {
        "name"     : "Example Mod",
        "identifier": "example",
        "abstract" : "A neat mod for KSP."
        "author"   : "Jeb Kerbin <jeb@example.com>",
        "license"  : "MIT",
        "version"  : "1.25",
        "prereqs"  : {
            "runtime" : {
                "requires" : {
                    "FooJeb" : "1.23",
                    "Karbol" : "0",
                }
            }
        },
        "release_status" : "stable",
        "min_ksp" : "0.23.0",
        "max_ksp" : "0.23.5",
        "resources" : {
            "homepage" : "http://forum.example.com/post/release-thread",
            "download" : {
                url : "http://gitjeb.example.com/Jeb/ExampleMod/releases"
            }
        }
    }

*TODO*

- Install instructions in meta-data
- Recommends example
- Overrides example (eg: RSS overrides TAC-LS)
- Includes example (for things including MM, Firespitter, etc)
- Config example (config files should be preseved across versions?)
- Should we auto-detect releases when we have github info?

### Metadata description

The metadata file provides machine-readable information about a
distribution.

#### Mandatory fields

##### name

This is the human readable name of the mod, and may contain any
printable characters. Eg: "Ferram Aërospace Research (FAR)",
"Real Solar System".

##### identifer

This is the gloablly unique identifier for the mod, and is how the mod
will be referred to by other CKAN documents.  It may only consist of
letters, numbers, underscores, and minus signs. Eg: "FAR" or
"RealSolarSystem". This is the identifier that will be used whenever
the mod is referenced (by `depends`, `conflicts`, or elsewhere).

If the mod would generate a `FOR` pass in ModuleManager, then the
identifier *must* be same as the ModuleManager name.

##### version

- TODO: Support version_from
- - file (eg: existing KSP versioning mod)
- - config (this might even make sense so modules can be effectively
    sniffed by game elements)
- - Filename (awful, because filenames can change, but could allow
    for GitHub releases to work.)
- Do we allow leading v's? Github tags have them, but they make everything
  else harder.

#### Optional fields

##### resources

    "resources" : {
        "homepage" : "http://examele.com/jebinator",
        "github"   : {
            "url"      : "http://github.com/example/jebinator",
            "releases" : "true"
        }
    }

The `resources` field describes additional information that a user or
program may wish to know about the mod. Presently the following fields
are described

- `homepage` is a URL that goes to the preferred landing page for the mod.
- `github` is a hash which *must* contain a `url` pointing to the
  github page for the project. It *may* include a `releases` key
  with a boolean value (which defaults to false) indicating if github releases
  should be used when searching for updates.

### TODOs

- Automatic generation from github webhooks on release (may require
  a special "from file" version option).
- Tools to create initial META.json.

