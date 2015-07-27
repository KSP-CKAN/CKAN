# LPIP v1.0

### Local Package Installer Plug-In
**©2015, Firelands Inc.**

##Overview

LPIP is a local KSP mod package installer, compiler and deleter. It comes with two Info-Zip binaries needed to make the operations. It use Local Package Files (.lpf) to create it's operations. They can also be created in this Batch runtime.


##Instructions

To install a mod, use LPIP hotkey or lpip.bat inside the core folder.
To create a local package file (.lpf) inside the makemods\ready folder, put your mod's folder (the folder which is inside GameData when the mod is installed, such as GameData\MyModDir) inside makemods\bin and run lpipmkp.bat inside core. You can later delete the mod's folder manually or not using the Windows Explorer.
To delete a lpf file inside packages, run lpipdel.bat inside core.
The binaries of InfoZip can't be used, this would violate InfoZip's license.


##License

This package is redistributable (and it is encouraged to redistribute it). Other permissions and whatnot are inside the GNU General Public License, 3.0 or newer.
