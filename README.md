# The Comprehensive Kerbal Archive Network (CKAN)

[Click here to open a new CKAN issue](https://github.com/KSP-CKAN/CKAN-support/issues/new)

[Click here to go to the CKAN wiki](https://github.com/KSP-CKAN/CKAN-support/wiki)

**The CKAN Spec can be found [here](Spec.md)**.

## What's the CKAN?

The CKAN is a metadata respository and associated tools to allow you to find, install, and manage mods for Kerbal Space Program. It provides strong assurances that mods are installed in the way prescribed by their metadata files, for the correct version of Kerbal Space Program, alongside their dependencies, and without any conflicting mods.

CKAN is great for players _and_ for authors:
- players can find new content and install it with just a few clicks;
- modders don't have to worry about misinstall problems or outdated versions;

The CKAN has been inspired by the solid and proven metadata formats from both the Debian project and the CPAN, each of which manages tens of thousands of packages.

## What's the status of the CKAN?

The CKAN is currently under [active development][1].
We very much welcome contributions, discussions, and especially pull-requests.

## The CKAN spec

At the core of the CKAN is the **[metadata specification](Spec.md)**,
which comes with a corresponding [JSON Schema](CKAN.schema).

This repository includes a JSON schema validator that you can use to [validate your files][3].

## CKAN for players

CKAN can download, install and update mods in just a few clicks. See the [User guide][2] to get started with CKAN.

## CKAN for modders

If you are an author, you might want to provide metadata to ensure that your mod installs correctly. While CKAN can usually figure out most of the metadata by itself, you can add your own file to provide dependencies, recommendations and installation instructions.

Check out the page about [adding a mod to the CKAN][4] on the wiki; you might also want to take a look at the [CKAN spec](Spec.md) and [CKAN schema](CKAN.schema), they can useful when writing your custom CKAN files.

## Helping the development

The CKAN client is a C# application that targets Mono 4.0, and therefore it runs natively on all the major platforms. It lives in the `CKAN` directory of this repository.

Contributions are welcome:

* We have [a wiki][5] that you are
encouraged to use and contribute to.

* Our [issues page][6]
lists things that need doing, or are being worked upon. Feel free to
add to this!

* Hop onto the [#ckan][7] IRC
channel (irc.esper.net) to chat with the team, lend a hand, or
ask questions.

* Ask the authors of your favourite mods to join the CKAN: [adding a mod to the CKAN][4] is very easy and will only take a few minutes.

 [1]:https://github.com/KSP-CKAN/CKAN/commits/master
 [2]:https://github.com/KSP-CKAN/CKAN-support/wiki/User-guide
 [3]:https://github.com/KSP-CKAN/CKAN-support/wiki/Adding-a-mod-to-the-CKAN#testing-your-file
 [4]:https://github.com/KSP-CKAN/CKAN-support/wiki/Adding-a-mod-to-the-CKAN
 [5]:https://github.com/KSP-CKAN/CKAN-support/wiki
 [6]:https://github.com/KSP-CKAN/CKAN/issues
 [7]:http://webchat.esper.net/?channels=ckan
