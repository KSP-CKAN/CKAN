# The Comprehensive Kerbal Archive Network (CKAN)

**The CKAN Spec can be found [here](Spec.md)**.

## What's the CKAN?

The CKAN is a metadata respository and associated tools to allow
you to find, install, and manage mods for Kerbal Space Program.
It provides strong assurances that mods are installed in the way
prescribed by their metadata files, for the correct version of Kerbal
Space Program, alongside their dependencies, and without any
conflicting mods.

By providing a standardised way to release and install modules, it is
hoped that many of the misinstall problems will be eliminated
(reducing the workload on authors), and a more straightforward path of
installing mods is provided (making it easier for users).

The CKAN has been inspired by the solid and proven metadata formats
from both the Debian project and the CPAN, each of which manages
tens of thousands of packages.

## What's the status of the CKAN?

The CKAN is currently under
[active development](https://github.com/KSP-CKAN/CKAN/commits/master).
It is not yet suitable for regular use, but testing by authors
and experienced users is strongly encouraged. We very much welcome
contributions, discussions, and especially pull-requests.

## The CKAN spec

At the core of the CKAN is the **[metadata specification](Spec.md)**,
which comes with a corresponding [JSON Schema](CKAN.schema). This
repository includes a JSON schema validator in the
[`bin`](https://github.com/KSP-CKAN/CKAN/tree/master/bin) directory.

## Using the CKAN as a user

```
$ ckan help

CKAN v0.04-17-gd42c8d6
Copyright CKAN Team, https://github.com/KSP-CKAN/CKAN
CC-BY 4.0, LGPL, or MIT; you choose!

  update       Update list of available mods

  available    List available mods

  install      Install a KSP mod

  remove       Remove an installed mod

  scan         Scan for manually installed KSP mods

  list         List installed modules

  show         Show information about a mod

  clean        Clean away downloaded files from the cache

  config       Configure CKAN

  version      Show the version of the CKAN client being used.

```

You can download one of our [releases](https://github.com/KSP-CKAN/CKAN/releases),
but be aware that anything marked 'pre-release' is considered unstable.

When reporting bugs, please run the client with the `--debug` switch
(eg: `ckan install --debug SomeMod`), and provide the output of
`ckan version`.

## Using the CKAN as a developer

The CKAN client is written in C#, targets Mono 4.0, and lives in
the `CKAN` directory of this repository. Contributions are welcome.

## Adding a mod to the CKAN

We have a wiki guide for
[adding a mod to the CKAN](https://github.com/KSP-CKAN/CKAN/wiki/Adding-a-mod-to-the-CKAN).
Please **be bold** and improve that guide however you see fit.

You will also find the [CKAN spec](Spec.md) and
[CKAN schema](CKAN.schema) useful when writing CKAN files.

## How I find out or help more?

* We have [a wiki](https://github.com/KSP-CKAN/CKAN/wiki) that you are
encouraged to use and contribute to.

* Our [issues page](https://github.com/KSP-CKAN/CKAN/issues)
lists things that need doing, or are being worked upon. Feel free to
add to this!

* Hop onto the [#ckan](http://webchat.esper.net/?channels=ckan) IRC
channel (irc.esper.net) to chat with the team, lend a hand, or
ask questions.
