#!/usr/bin/perl
use 5.010;
use strict;
use warnings;
use autodie qw(:all);
use FindBin qw($Bin);
use File::Path qw(remove_tree);
use IPC::System::Simple qw(systemx capturex);
use File::Spec;

# Simple script to build and repack

my $REPACK  = "CKAN/packages/ILRepack.1.25.0/tools/ILRepack.exe";
my $TARGET  = "Release";     # 'Debug' is okay too.
my $OUTNAME = "ckan.exe";   # Or just `ckan` if we want to be unixy
my $BUILD   = "$Bin/../build";
my $SOURCE  = "$Bin/../CKAN";
my @CP      = qw(cp -r --reflink=auto --sparse=always);
my $VERSION = capturex(qw(git describe --long));
my @ASSEMBLY_INFO = (
    File::Spec->catdir($BUILD,"CKAN/Properties/AssemblyInfo.cs"),
    File::Spec->catdir($BUILD,"CmdLine/Properties/AssemblyInfo.cs"),
    File::Spec->catdir($BUILD,"GUI/Properties/AssemblyInfo.cs"),
);

# Remove newline
chomp($VERSION);

# Make sure we clean any old build away first.
remove_tree($BUILD);

# Copy our project files over.
systemx(@CP, $SOURCE, $BUILD);

# Remove any old build artifacts
remove_tree(File::Spec->catdir($BUILD, "CKAN/bin"));
remove_tree(File::Spec->catdir($BUILD, "CKAN/obj"));

# Before we build, add our version number in.

foreach my $assembly (@ASSEMBLY_INFO) {
    open(my $assembly_fh, ">>", $assembly);
    say {$assembly_fh} qq{[assembly: AssemblyInformationalVersion ("$VERSION")]};
    close($assembly_fh);
}

# Change to our build directory
chdir($BUILD);

# And build..
system("xbuild", "/property:Configuration=$TARGET", "CKAN.sln");

say "\n\n=== Repacking ===\n\n";

chdir("$Bin/..");

# Repack ckan.exe

my @cmd = (
    $REPACK,
    "--out:ckan.exe",
    "--lib:build/CmdLine/bin/$TARGET",
    "build/CmdLine/bin/$TARGET/CmdLine.exe",
    glob("build/CmdLine/bin/$TARGET/*.dll"),
    "build/CmdLine/bin/$TARGET/CKAN-GUI.exe", # Yes, bundle the .exe as a .dll
);

system(@cmd);

# Repack ks2ckan

my @cmd = (
    $REPACK,
    "--out:ks2ckan.exe",
    "--lib:build/KerbalStuff/bin/$TARGET",
    "build/KerbalStuff/bin/$TARGET/ks2ckan.exe",
    glob("build/KerbalStuff/bin/$TARGET/*.dll"),
);

system(@cmd);

say "\n\n=== Tidying up===\n\n";

unlink("$OUTNAME.mdb");
unlink("ks2ckan.exe.mdb");

say "Done!";
