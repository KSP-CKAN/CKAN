# The Comrpehensive Kerbal Archive Network (CKAN)

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
[active development](https://github.com/pjf/CKAN/commits/master).
It is not yet suitable for regular use, but testing by authors
and experienced users is strongly encouraged. We very much welcome
contributions, discussions, and especially pull-requests.

## The CKAN spec

At the core of the CKAN is the **[metadata specification](Spec.md)**,
which comes with a corresponding [JSON Schema](CKAN.schema). This
repository includes a JSON schema validator in the
`[bin](https://github.com/pjf/CKAN/tree/master/bin)` directory.

## Using the CKAN as a user

As the CKAN is still under active development, you'll need to compile
the CKAN client to use it. The command `ckan help` will list available
features.

## Using the CKAN as a developer

The CKAN client is written in C#, targets Mono 4.0, and lives in
the `CKAN` directory of this repository. Contributions are welcome.

## Adding CKAN support to mods

The CKAN is designed so that metadata can be provided for
mods without altering the release process or the workflow of authors.
For those authors who wish to provide additional support,
a [CKAN file](Spec.md) can be bundled, facilitating easier
installation and indexing. Support for
[github releases](https://github.com/pjf/CKAN/issues/2) will also
be provided.

During development we have limited support for
[selected mods](https://github.com/pjf/CKAN/tree/master/meta) that
we're using for testing. You are encouraged to submit CKAN files
for additional mods. If you are a mod author, please
[open a github issue](https://github.com/pjf/CKAN/issues/new) saying
you'd like to provide support, and we'll work with you to
make sure everything goes smoothly.

## How can I help more?

Head on over to our [issues page](https://github.com/pjf/CKAN/issues),
or ask `pjf` on [IRC](http://webchat.esper.net/?channels=kspmodders)
how you can lend a hand.
