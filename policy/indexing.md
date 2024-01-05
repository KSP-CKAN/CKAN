# Indexing and de-indexing mods

Our policy is based on two principles:

* CKAN takes mod authors' licensing rights seriously
* CKAN acts in the interests of its users **and** mod authors

Note that for purposes of this document, the term "author" does not necessarily mean the original author of a mod. It should be understood to include a maintainer, custodian, etc.

## Indexing

The CKAN team will not add a mod to CKAN without the author's permission. Checking the "Add to CKAN" checkbox on SpaceDock is taken as a request to have the mod added to CKAN (unless the uploader is not the author, see below).

Indexing a mod on CKAN can be a burden on authors—they may need to make decisions about the metadata, and it is one more thing they are responsible for supporting. Indexing a mod on CKAN can also benefit the author: it can make installation easier, reducing support requests; it can enforce depdendencies and conflicts automatically; it provides an easy way for users to update the mod; and it can increase a mod's visibility. Minimizing the burden and maximizing the benefits to mod authors is a core goal of the CKAN team.

A collaborative environment is crucial. When something goes wrong—and it will—sometimes the cause is the CKAN metadata, sometimes it's a bug in CKAN itself, and sometimes it's an error by a mod author. Recognizing this and working together to resolve issues yields the greatest benefit to users and mod authors. The CKAN team strives to support mod authors in fixing such issues. The CKAN team can be reached on [Discord](https://discord.gg/Mb4nXQD) (use Help→Discord in the CKAN client) or you can file an issue at:

* <https://github.com/KSP-CKAN/CKAN/issues/new/choose> (for issues related to the client)
* <https://github.com/KSP-CKAN/NetKAN/issues/new/choose> (for issues related to metadata for KSP1)
* <https://github.com/KSP-CKAN/KSP2-NetKAN/issues/new/choose> (for issues related to metadata for KSP2)

Pull requests are always welcome.

When someone requests a mod for indexing, the CKAN team will attempt to verify that the person making the request is the author of the mod and that they comply with any licenses on the mod's content. For requests by a third party, we attempt to contact the author and only add the mod if they approve. Obviously this cannot be perfect. If you believe your content is indexed on CKAN and is in violation of its license, please alert the CKAN team via any of the above links. Note that CKAN does not host or distribute any mod content—it only links to mod content on sites like SpaceDock, GitHub, etc., and provides a method to fetch that content similar to a web browser.

When a mod has a permissive license (GPL, MIT, CC, etc.), someone else may adopt or fork the mod and request that their version be indexed on CKAN. This ensures that abandoned mods can be continued and support requests go to the new author instead of the original one. An existing [identifier](https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md#identifier) or a new identifier may be used as collaboratively decided by the author and the CKAN team. The CKAN team recommends using all possible avenues to contact the original author (GitHub, forum private message, email, etc.) and waiting a reasonable amount of time for a response before adopting a mod. The CKAN team may consider input from the previous author or other factors before indexing the adopted mod.

When a mod has a restrictive licenses, we require proof of permission to adopt the mod from the original author before it is indexed on CKAN to ensure that previous author is OK with their work being used in this way.

The CKAN team may reject a request to index a mod for any reason. Primarily the CKAN team will consider the impact on users and other mod authors when making this decision—for example mods that are likely to cause conflicts, data loss, compatibility issues, etc.

CKAN maintains [a list of authors who have requested that their mods not be added to CKAN without their explicit request](Opt-out-list.md). A mod by an author on this list will not be added to CKAN unless requested by the author.

## De-indexing

If an author asks for their mod to be de-indexed, the CKAN team may inquire as to the reason, in case it is related to an issue that we can fix for them and they were not aware that we can do that. If we can, we'll work to fix the problem to the mod author's satisfaction. If not, or if we can't fix the issue, then we will de-index the mod promptly.
