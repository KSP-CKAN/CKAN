# The Comprehensive Kerbal Archive Network (CKAN)

**The CKAN Spec can be found [here](Spec.md)**.

## What's the CKAN?

The CKAN is a metadata respository and associated tools to allow you to find, install, and manage mods for Kerbal Space Program. It provides strong assurances that mods are installed in the way prescribed by their metadata files, for the correct version of Kerbal Space Program, alongside their dependencies, and without any conflicting mods.

CKAN is great for players _and_ for authors:
- players can find new content and install it with just a few clicks;
- modders don't have to worry about misinstall problems or outdated versions;

The CKAN has been inspired by the solid and proven metadata formats from both the Debian project and the CPAN, each of which manages tens of thousands of packages.

## What's the status of the CKAN?

The CKAN is currently under [active development](https://github.com/KSP-CKAN/CKAN/commits/master).
We very much welcome contributions, discussions, and especially pull-requests.

## The CKAN spec

At the core of the CKAN is the **[metadata specification](Spec.md)**,
which comes with a corresponding [JSON Schema](CKAN.schema).

This repository includes a JSON schema validator that you can use to [validate your files](https://github.com/KSP-CKAN/CKAN-support/wiki/Adding-a-mod-to-the-CKAN#testing-your-file).

## CKAN for players

CKAN can download, install and update mods in just a few clicks. See the [User guide](https://github.com/KSP-CKAN/CKAN-support/wiki/User-guide) to get started with CKAN.

## CKAN for modders

If you are an author, you might want to provide metadata to ensure that your mod installs correctly. While CKAN can usually figure out most of the metadata by itself, you can add your own file to provide dependencies, recommendations and installation instructions.

Check out the page about [adding a mod to the CKAN](https://github.com/KSP-CKAN/CKAN-support/wiki/Adding-a-mod-to-the-CKAN) on the wiki; you might also want to take a look at the [CKAN spec](Spec.md) and [CKAN schema](CKAN.schema), they can useful when writing your custom CKAN files.

## Helping the development

The CKAN client is a C# application that targets Mono 4.0, and therefore it runs natively on all the major platforms. It lives in the `CKAN` directory of this repository.

Contributions are welcome:

* We have [a wiki](https://github.com/KSP-CKAN/CKAN-support/wiki) that you are
encouraged to use and contribute to.

* Our [issues page](https://github.com/KSP-CKAN/CKAN/issues)
lists things that need doing, or are being worked upon. Feel free to
add to this!

* Hop onto the [#ckan](http://webchat.esper.net/?channels=ckan) IRC
channel (irc.esper.net) to chat with the team, lend a hand, or
ask questions.

* Ask the authors of your favourite mods to join the CKAN: [adding a mod to the CKAN](https://github.com/KSP-CKAN/CKAN-support/wiki/Adding-a-mod-to-the-CKAN) is very easy and will only take a few minutes.
