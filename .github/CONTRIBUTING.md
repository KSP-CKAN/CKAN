# Contributing to CKAN

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

The following is a set of guidelines for contributing to the Comprehensive Kerbal Archive Network. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

#### Table Of Contents

* [Code of Conduct](#code-of-conduct)
* [Reporting issues](#reporting-issues)
  * [Reporting a bug](#reporting-a-bug)
  * [Requesting a new feature](#requesting-a-new-feature)
* [Helping with issues](#helping-with-issues)
  * [Verifying issues](#verifying-issues)
  * [Testing patches](#testing-patches)
* [Translating](#translating)
* [Creating pull requests](#creating-pull-requests)
  * [Creating a patch](#creating-a-patch)
  * [Creating a pull request](#creating-a-pull-request)
  * [Rebasing a pull request](#rebasing-a-pull-request)
* [Keeping your branch up-to-date](#keeping-your-branch-up-to-date)
* [Coding conventions](#coding-conventions)

## Code of Conduct

This project and everyone participating in it is governed by the [CKAN Code of Conduct][1]. By participating, you are expected to uphold this code. Please report unacceptable behavior to mykdowling@gmail.com.

## Reporting issues
### Reporting a bug

If you've found a problem in CKAN, do a search on GitHub under [Issues][2] in case it has already been reported.

If you are unable to find any open GitHub issues addressing the problem you found, your next step will be to [open a new one][2] using the respective 'Bug' template.

Your issue report should contain a title and as much relevant information as possible of the issue. Your goal should be to make it easy for yourself - and others - to reproduce the bug and figure out a fix.

### Requesting a new feature

If there's a new feature that you want to see added to CKAN, you can [open an issue][2] using the respective 'Feature' template, or you can write the code yourself.

## Helping with issues
### Verifying issues

For starters, it helps just to verify bug reports. Can you reproduce the reported issue on your own computer? If so, you can add a comment to the issue saying that you're seeing the same thing.

If an issue is very vague, can you help narrow it down to something more specific? Maybe you can provide additional information to help reproduce a bug, or help by eliminating needless steps that aren't required to demonstrate the problem.

### Testing patches

You can also help out by examining pull requests that have been submitted to CKAN via GitHub. In order to apply someone's changes, you need to first create a dedicated branch:

```
$ git clone https://github.com/KSP-CKAN/CKAN.git
$ cd CKAN
$ git checkout -b testing_branch
```

Then, you can use their remote branch to update your codebase. For example, let's say the GitHub user JohnSmith has forked and pushed to a topic branch "orange" located at https://github.com/JohnSmith/CKAN.

```
$ git remote add JohnSmith https://github.com/JohnSmith/CKAN.git
$ git fetch JohnSmith
$ git checkout JohnSmith/orange
```

After applying their branch, test it out! Comment on the GitHub issue to apply some changes or by giving your approval.

## Translating

We recently started working on translating CKAN and are happy to have people volunteer to translate with us. Just follow these steps:

* Fork this repository by clicking on the 'Fork' button on the top of this page.

* Click on the green 'Clone' button and copy the link.

* Open a terminal and run following code:

```
$ git clone https://github.com/YourName/CKAN.git
$ cd CKAN
$ git checkout -b new_branch
``` 

* Add a folder for your own language, for example: ` GUI/Localization/it-IT` for Italian.

* Copy the contents of ` GUI/Localization/en-US` into your own language folder.

* Rename the files from `en-US` to `it-IT`.

* Start translating the files.

**Translations we already have:**

* English (US)
* English (AU)
* German

## Creating pull requests

Before you make a pull request, you have to agree to contribute under these licenses:

* Contributions to the CKAN source *must* be licensed under the [MIT license][3].

* Contributions of CKAN metadata files (.netkan or .ckan) *must* be be contributed under the [CC-0][4] license.

### Creating a patch

* Fork this repository by clicking on the 'Fork' button on the top of this page.

* Click on the green 'Clone' button and copy the link.

* Open a terminal and run following code:

```
$ git clone https://github.com/YourName/CKAN.git
$ cd CKAN
$ git remote add upstream https://github.com/KSP-CKAN/CKAN.git
$ git checkout -b new_branch
``` 

* Now get busy and add/edit code. Make sure to follow our [Coding conventions](#coding-conventions).

* Commit your changes:

```
$ git add -A
$ git commit -m "Brief description of your change"
$ git push origin new_branch
```

### Creating a pull request

* Open a new GitHub pull request with your patch.

* Ensure the PR description clearly describes the problem and solution. Include the relevant issue number if applicable.

### Rebasing a pull request

It can sometimes happen that other commits get merged into the master branch and that your pull request will need a rebase.

```
$ git fetch --all
$ git checkout new_branch
$ git rebase upstream/master
```

## Keeping your branch up-to-date

To keep your 'master' branch up-to-date, you can run following commands:

```
$ git fetch --all
$ git checkout master
$ git pull upstream/master
```

## Coding conventions

This is open source software. Consider the people who will read your code, and make it look nice for them.

* Four spaces, no tabs (for indentation).
* No trailing whitespace. Blank lines should not have any spaces.
* Use `a = b` and not `a=b`.
* Follow the conventions in the source you see used already.

the Comprehensive Kerbal Archive Network (CKAN) is a volunteer effort. We encourage you to pitch in and [join the team][5]!

Thanks! :heart: :heart: :heart:

The CKAN Team

[1]: https://github.com/KSP-CKAN/CKAN/blob/master/.github/CODE_OF_CONDUCT.md
[2]: https://github.com/KSP-CKAN/CKAN/issues
[3]: https://github.com/KSP-CKAN/CKAN/blob/master/LICENSE.md
[4]: https://creativecommons.org/publicdomain/zero/1.0/
[5]: https://github.com/KSP-CKAN/CKAN/graphs/contributors
