# Changing our version comparsion routine

(Originally posted as [issue #963](https://github.com/KSP-CKAN/CKAN/issues/963))

It seems that a day doesn't go past when I don't have to explain why changing versions is *hard*, so I'm going to do that here. I'm also going to make us a nice policy folder where things like this can go, so we've got something that's easy to refer to, but also undergoes peer review before changing.

So, why can't we change how we do version comparisons?

## It can break existing mods

We index more than 700 mods, and have more than 2,500 releases indexed. Of those, we've only had a fraction where the Debian versioning spec has not worked. Changing the versioning implementation runs a very real risk of breaking existing mods, and nobody seems to submit the changes along with a proof that all 2,500+ existing documents won't be adversely affected.

## It's almost never needed

The netkan pre-processor now has additional options for post-processing of versions into standard forms, or bumping the epoch when versions are truly wacky. These come with no risk of breaking existing mods, because we can simply apply them as-needed. In particular, we have:

* `x_netkan_force_v` - Force a friggin' `v` onto the start of the version string.
* `x_netkan_epoch` - Force a friggin' epoch string onto the front of the version.

OMG, a mod is missing a v? Use the first of these options.
OMG, a mod went backwards in its order (it's happened), named a version after what the author had for breakfast (it will happen), or completely changed how they do versioning? Use the second.

Seriously. Adjusting how we generate metadata for a couple of mods means we're backwards compatible with all extant clients, and comes with a *much* smaller risk profile.

## Changing versioning schemes is much harder than everyone seems to think

If we spot a document implementing the old existing spec (v1.6 at time of writing), then we should use the current versioning scheme. If we spot a document with the "new" spec (whatever it is you propose), then we should use the new versioning scheme. Holy fark, does that mean we need *two* implementations of the versioning scheme in clients? Yes, it does. Does that make things twice as complex as it currently is? You bet! Does that mean we're likely to see MORE bugs reported? I think so! And what happens when we need to compare an old version to a new version? We cry, that's what.

The only time we can get away with not doing this is if the versioning scheme compares *all* existing documents the same (yes, all 2,500+ of them). Nobody suggests this, because all the suggestions seem to be about *changing* how versions work, rather than extending it. Even if we did this, we'd have to bump our spec version for all new metadata to make sure old clients don't try to use the new spec format.

**TL;DR:** We're not changing the friggin' spec just because your favourite mod has inconsistent versions. Use the netkan options instead.
