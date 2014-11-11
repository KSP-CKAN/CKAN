#!/usr/bin/perl
use 5.010;
use strict;
use warnings;
use autodie qw(:all);
use FindBin qw($Bin);
use File::Path qw(remove_tree);
use IPC::System::Simple qw(systemx capturex);
use File::Spec;
use File::Copy::Recursive qw(rcopy);
use autodie qw(rcopy);
use Fcntl qw(SEEK_SET);

# Simple script to build and repack

my $REPACK  = "CKAN/packages/ILRepack.1.25.0/tools/ILRepack.exe";
my $TARGET  = "Release";     # 'Debug' is okay too.
my $OUTNAME = "ckan.exe";   # Or just `ckan` if we want to be unixy
my $BUILD   = "$Bin/../build";
my $SOURCE  = "$Bin/../CKAN";
my $VERSION = capturex(qw(git describe --tags --long));
my $METACLASS = "build/CKAN/Meta.cs";

my @PROJECTS = qw(CmdLine CKAN CKAN-GUI NetKAN Tests);

# Remove newline
chomp($VERSION);

# Make sure we clean any old build away first.
remove_tree($BUILD);

# Copy our project files over.
copy($SOURCE, $BUILD);

# Remove any old build artifacts
foreach my $project (@PROJECTS) {
    remove_tree(File::Spec->catdir($BUILD, "$project/bin"));
    remove_tree(File::Spec->catdir($BUILD, "$project/obj"));
}

# Before we build, add our version number in.

set_build($METACLASS, $VERSION);

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

# Repack netkan

@cmd = (
    $REPACK,
    "--out:netkan.exe",
    "--lib:build/NetKAN/bin/$TARGET",
    "build/NetKAN/bin/$TARGET/netkan.exe",
    glob("build/NetKAN/bin/$TARGET/*.dll"),
);

system(@cmd);

say "\n\n=== Tidying up===\n\n";

# We don't tidy up any more, these provide useful debugging.
# unlink("$OUTNAME.mdb");
# unlink("netkan.exe.mdb");

say "Done!";

# Do an appropriate copy for our system
sub copy {
    my ($src, $dst) = @_;

    if ($^O eq "MSWin32") {
        # Use File::Copy::Recursive under Windows
        rcopy($src, $dst);
    }
    else {
        # Use friggin' awesome btrfs magic under Linux.
        # This still works, even if not using btrfs (justh with less magic)
        my @CP      = qw(cp -r --reflink=auto --sparse=always);
        system(@CP,$src,$dst);
    }
    return;
}

sub set_build {
    my ($file, $build) = @_;

    local $/;   # Slurp entire files on read

    open(my $fh, '+<', $file);
    my $contents = <$fh>;

    $contents =~ s{(BUILD_VERSION\s*=\s*)null}{$1"$build"}
        or die "Could not find BUILD_VERSION string";

    # Truncate our file and overwrite with new info
    truncate($fh,0);
    seek($fh,SEEK_SET,0);

    print {$fh} $contents;
    close($fh);
}
