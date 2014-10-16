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
my $ASSEMBLY_INFO = File::Spec->catdir($BUILD,"CKAN/Properties/AssemblyInfo.cs");

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
open(my $assembly_fh, ">>", $ASSEMBLY_INFO);
say {$assembly_fh} qq{[assembly: AssemblyInformationalVersion ("$VERSION")]};
close($assembly_fh);

# Change to our build directory
chdir($BUILD);

# And build..
system("xbuild", "/property:Configuration=$TARGET", "CKAN.sln");

say "\n\n=== Repacking ===\n\n";

chdir("$Bin/..");

system(
    $REPACK,
    "--out:ckan.exe",
    "--lib:build/CKAN/bin/$TARGET",
    "build/CKAN/bin/$TARGET/CKAN.exe",
    glob("build/CKAN/bin/$TARGET/*.dll"),
);

say "\n\n=== Tidying up===\n\n";

unlink("$OUTNAME.mdb");

say "Done!";
