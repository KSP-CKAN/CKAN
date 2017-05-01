# Abstract

This document describes the behaviour that an application must implement
in order to be a CKAN-compatible mod manager.

Adherence to this document ensures that all mod-management applications
that consume CKAN metadata behave consistently.

This document refers to version 1 of the CKAN metadata specification.



# Definitions

The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED", "MAY", and "OPTIONAL" in this document are to be interpreted as described in  [RFC 2119](http://www.faqs.org/rfcs/rfc2119.html).



# Metadata fields

CKAN metadata consists of a JSON file
conforming to the official JSON schema.

The following sections define how the information contained in the metadata files
should be interpreted and handled by ckan clients.


## Required Fields

The fields described in this section are required by the ckan metadata specification
and must be present for all valid metadata files.
A ckan client may assume that all these fields are present in all valid metadata files.
Clients may assume that all metadata contained in the official CKAN repositories is valid;
however, client-side validation against the JSON schema is recommended.

### identifier

This is the unique identifier for a module.
The same identifier is shared by multiple versions of the same module.

A client must use this identifier to discriminate different modules
and must not allow the user to install two modules with the same identifier.

### version

A string defining the version of the module.
The specification allows for any string to be used as a version:
since there is no standard regarding versioning schemes,
a client should not assume any.

Clients must implement a version comparison algorithm
compatible with the [debian versioning scheme](https://www.debian.org/doc/debian-policy/ch-controlfields.html#s-f-Version).

Clients must not allow the installation of multiple versions
of the same module at the same time.

### download

The download field contains an url pointing to file
that can be used to install the module.
Redirects are allowed in this url.

The downloaded file will usually be a zip archive;
clients must support at least this format.
Support for other types of archive is recommended.

### spec-version

This field contains the version of the metadata specification
that must be used to process the metadata file.

A client must not allow the installation of a file that requires
a version of the specification that it does not implement completely.
It is recommended to hide incompatible modules from the list of available modules, if any.

### name

This field contains a user-friendly name that can be used to present the mod to the user.
This field is not necessarily unique;

clients must not identify a package using its `name`.
However, it is recommended to use this field to help users
search for the identifier of a module.

### abstract

This field contains a short description of what a module contains.
Clients should use the value of this field to provide a description to the user, if any.

### license

This field is required by the metadata specification.
Clients are not prescribed any behaviour regarding the value of this field.


## Optional Fields

The following fields are not required by the specification.
Clients should not assume they are present in valid metadata;
however, if they are present, clients must implement
the behaviours prescribed in the following sections.

### description

The description field contains a longer description of the module
than the one provided in the `abstract` field.
Clients may use this field to display a verbose description of the module.

### comment

A `comment` field may appear at any point of the metadata.
These fields will always contain a string.
The value of a `comment` should not be displayed to the user,
but may be employed for any purpose.

### author

Either a string, or a list of strings.
The content of the `author` field should be shown to the user,
and it should be used to search for specific modules if present.

### download_size

Size (in bytes) of the file downloaded through the `download` link.
Showing the total download size to the user before any download
is recommended, but not required.

### release_status

This field can have the following values:
- `stable`
- `testing`
- `development`

This field may be use to restrict the list of installable packages
(e.g, to only allow `stable` modules).

If this field is missing, clients must assume its value to be `stable`.

### ksp_version

This field may contain either:
- `any`
- a string matching the regular expression:
		[0-9]+\\.[0-9]+(\\.[0-9]+)?

If the field contains the string `any`,
clients must allow the installation of the module
on any version of the game.

Otherwise, the client must only allow the installation of the module
into a copy of the game with a matching version number.

### ksp_version_min and ksp_version_max

Like the `ksp_version` field, these fields can contain either the string `any`
or a version number.

These fields are mutually exclusive with the `ksp_version` field.
Clients should consider metadata files that contain both types of fields as invalid.
In this case, it is suggested to enforce the most specific version constraint.


## install stanzas

Installation instructions are specified using the `install` field.
This *optional* field contains an array of install directive
(called "stanzas" in some of the documentation).
Each stanza defines where to install a set of files.

If no install sections are provided,
a CKAN client must find the top-most directory in the archive that matches the module identifier,
and install that with a target of `GameData`.

### Required: file and install_to

If an `install` stanza is present,
it must contain at least two fields:

- `file`: path of a file or folder, relative to the top-level of the zip file obtained through the `download` link. The file (or folder) must be extracted to the location specified by the `install_to` field.
- `install_to`: the location where the file or folder must be extracted. This field can only take one of the following 4 values:
	. `GameRoot`: the root folder of the KSP installation;
	. `GameData`: equal to `$(GameRoot)/GameData`;
    . `Ships`: equal to `$(GameRoot)/Ships`
    . `Tutorial`: equal to #TODO: where!?

The client must not allow the installation of any file in any directory other than those listed above.
The file or folder located by the `file` field must be extracted to the destination directory
preserving the directory tree structure in the zip file.
Installation of any file outside of the game's directory is strictly forbidden.

### Optional: filters

If a `filter` field is specified,
it contains one or more files that must be excluded from the installation
even if they are selected by the corresponding `file` field.
The `filter` field only affects files in the same install stanza
(that is, install stanzas are independent from each other).

Analogously, if a `filter_regexp` is specified
the client must exclude from the installation
all the files that match one or more of the listed regular expressions.

### File overwriting

File overwriting is not permitted.
Any attempt to overwrite another file
must result in an error,
aborting the installation attempt.

### Rolling back

In case of any error during an installation,
the client must be able to roll back any change
restoring the state of the system
prior to the installation attempt.

Partial installations are not permitted:
the whole installation process must be implemented
as an atomic operation
(i.e, it either succeeds or fails completely).


## Module relationships

Modules can be put in relationship with one another using one or more relationship descriptors.

There are 4 types of relationship fields:
1. `depends`
2. `recommends`
3. `suggests`
4. `conflicts`

These 4 fields are all optional.
If present, they contain a list of relationship descriptors with the following fields:
- `name`: contains the `identifier` of the related module (required);
- `version`: the specific version of the related module (optional);
- `min_version` and `max_version`: allows to specify a range of versions (optional).

Version ranges are only valid for the `conflicts` relationship:
if version ranges are encountered in other relationship descriptors,
the behaviour is undefined: clients may choose how to resolve the ambiguity.

### depends

Indicates that a module cannot be installed unless all its dependencies are not installed too.
Clients must not install a module unless all of its dependencies are either installed,
or are about to be installed alongside the module.

Clients may implement a way for the user to override the dependency checking:
this mechanism, if implemented, must be explicitly selected by the user
for each install process.

### recommends

A client must automatically select for installation all the packages
that are recommended by any of the modules about to be installed.
The selection must not be recursive:
packages that are recommended by a package that was recommended
must not be selected too.

Clients should implement a way for the user to refuse one or more recommended packages,
and may implement a way to enable recursive recommendations.

### suggests

A client must not automatically select for installation all the packages
that are suggested by any of the modules about to be installed.
The selection must not be recursive:
packages that are recommended by a package that was suggested
must not be selected too.

Clients should implement a way for the user to select one or more suggested packages,
and may implement a way to enable recursive suggestions.

### conflicts

Indicates that two or more modules cannot be installed at the same time.
Clients must not allow installation of conflicting modules at the same time.

Clients may implement a way for the user to override the conflict checking:
this mechanism, if implemented, must be explicitly selected by the user
for each install process.

### provides

Additionally, modules may indicate they provide a name.
Providing must be used to signal interchangeable modules:
modules may be in relationships with a provided name.

Version constraints are not valid when referring to a provided name.

If a package conflicts with a provided name,
clients must consider all the packages that provide that name
as conflicting and deal with them as described in the `conflicts` section.

For all other relationships, the behaviour is undefined
and clients are free to resolve the relationship as they desire.
However, clients should defer the choice to the user
presenting them with the list of interchangeable packages.

## Additional fields

The fields described in this section are not strictly required
to manage the modules and are intended to supplement the user experience.

### resources

The `resources` field contains optional links to various online resources.
Clients may use the data contained in these fields for any purpose.
Refer to the schema for the list of available fields.

### Spec extensions

Any field starting with the string `x_` is allowed to appear in the metadata.
Clients may inspect and use the data in these fields freely