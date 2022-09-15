# CKAN's APT repository

We have created an APT repository that you can add to your Debian-based OS to install CKAN. This will allow you to run CKAN from the system app menus or via `ckan` from your command line. Your system's package manager will pull in dependencies and update CKAN automatically. There's even a man page.

(These instructions are here instead of a wiki because the `curl` command adds our trusted key to your APT configuration, and we do not want that line to be editable by the entire world, in case it might be altered maliciously.)

See https://wiki.debian.org/DebianRepository/UseThirdParty for the Debian policy that we used to generate these steps.

## Stable builds

These are [the main releases](https://github.com/KSP-CKAN/CKAN/releases), recommended for most users. You will have the same features at the same time as everyone else, but you will have the added conveniences of APT managing the updates for you.

```
sudo curl -sS -o /usr/share/keyrings/ksp-ckan-archive-keyring.gpg https://raw.githubusercontent.com/KSP-CKAN/CKAN/master/debian/ksp-ckan.gpg
echo 'deb [arch=amd64 signed-by=/usr/share/keyrings/ksp-ckan-archive-keyring.gpg] https://ksp-ckan.s3-us-west-2.amazonaws.com/deb stable main' | sudo tee /etc/apt/sources.list.d/ksp-ckan.list > /dev/null
sudo apt update
sudo apt install ckan
```

## Nightly builds

If you like to live dangerously, these are the bleeding edge builds that are generated every time we merge changes to the main branch. On the plus side, you'll get fixes and enhancements faster than everyone else. On the minus side, these builds are essentially untested; we don't know whether they're reliable until we take a close look at them and make sure they're complete and won't break things, at which point they turn into a stable build (if that sounds more like what you want, scroll up to the previous section).

Things may break! But if they do and you [report it to us](https://github.com/KSP-CKAN/CKAN/issues/new/choose), you'll be a hero to CKAN users everywhere, whether they know it or not.

```
sudo curl -sS -o /usr/share/keyrings/ksp-ckan-archive-keyring.gpg https://raw.githubusercontent.com/KSP-CKAN/CKAN/master/debian/ksp-ckan.gpg
echo 'deb [arch=amd64 signed-by=/usr/share/keyrings/ksp-ckan-archive-keyring.gpg] https://ksp-ckan.s3-us-west-2.amazonaws.com/deb nightly main' | sudo tee /etc/apt/sources.list.d/ksp-ckan.list > /dev/null
sudo apt update
sudo apt install ckan
```
