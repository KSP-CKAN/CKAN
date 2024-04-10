# CKAN'S RPM repository

We have created an RPM repository that you can add to your RPM-based OS to install CKAN. This will allow you to run CKAN from the system app menus or via `ckan` from your command line. Your system's package manager will pull in dependencies and update CKAN automatically. There's even a man page.

## Stable builds

These are [the main releases](https://github.com/KSP-CKAN/CKAN/releases), recommended for most users. You will have the same features at the same time as everyone else, but you will have the added conveniences of DNF managing the updates for you.

```
sudo dnf config-manager --add-repo https://ksp-ckan.s3-us-west-2.amazonaws.com/rpm/stable/ckan_stable.repo
sudo dnf install ckan
```

### Or if you are on OpenSUSE

```
sudo zypper addrepo https://ksp-ckan.s3-us-west-2.amazonaws.com/rpm/stable/ckan_stable.repo
sudo zypper install ckan
```

## Nightly builds

If you like to live dangerously, these are the bleeding edge builds that are generated every time we merge changes to the main branch. On the plus side, you'll get fixes and enhancements faster than everyone else. On the minus side, these builds are essentially untested; we don't know whether they're reliable until we take a close look at them and make sure they're complete and won't break things, at which point they turn into a stable build (if that sounds more like what you want, scroll up to the previous section).

Things may break! But if they do and you [report it to us](https://github.com/KSP-CKAN/CKAN/issues/new/choose), you'll be a hero to CKAN users everywhere, whether they know it or not.

```
sudo dnf config-manager --add-repo https://ksp-ckan.s3-us-west-2.amazonaws.com/rpm/nightly/ckan_nightly.repo
sudo dnf install ckan
```

### Or if you are on OpenSUSE

```
sudo zypper addrepo https://ksp-ckan.s3-us-west-2.amazonaws.com/rpm/nightly/ckan_nightly.repo
sudo zypper install ckan
```
