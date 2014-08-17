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

## Meta-data

When included in a distribution, the metadata *must* be included
in a file called `META.json`. When extracted from a distribution,
it *must* be given an identical name as the file it came from, but
with the `.zip` or other extension replaced with `.json`.

The CKAN metadata spec is based upon the
[CPAN metadata spec](https://metacpan.org/pod/CPAN::Meta::Spec).

### Example Metadata

    {
        "name"     : "Example Mod",
        "abstract" : "A neat mod for KSP."
        "author"   : [ "Jeb Kerbin <jeb@example.com>" ],
        "license"  : [ "mit" ],
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

### Metadata description

The metadata file provides machine-readable information about a
distribution.

### TODOs

- Automatic generation from github webhooks on release (may require
  a special "from file" version option).
- Tools to create initial META.json.

