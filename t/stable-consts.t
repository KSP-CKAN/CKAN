#!/usr/bin/perl
use Test::Most;
use IPC::System::Simple qw(capture);
use FindBin qw($Bin);
use File::Find;

my $branch = capture("git rev-parse --abbrev-ref HEAD");
chomp $branch;

# If our branch has a "stable-looking" name, then make sure we have stable
# build symbols defined. Otherwise, we don't care.

# Stable looking branches have the word 'stable' in them, or end in a
# `vx.y` number, with 'y' being even. (Eg: `release-v1.0` is stable,
# as is `235_stable_bugfix_foo)

# If the CKAN_STABLE environmen variable is set, we're always considered stable.

unless ($ENV{CKAN_STABLE} or $branch =~ /(\b|_)stable(\b|_)|v\d+\.\d*[02468]$/) {
    plan skip_all => "$branch is not considered stable";
}

# Find all our .csproj files and test them.
find(\&test_csproj, "$Bin/../CKAN");

done_testing;

sub test_csproj {
    return unless /\.csproj$/;  # Only process .csproj files

    local $/;   # File-slurp on read mode.

    open(my $fh, '<', $_);

    my $contents = <$fh>;

    # We're not too picky, just make sure STABLE is in a DefineConstants somewhere...

    ok($contents =~ m{<DefineConstants>[^<]*\bSTABLE\b}, "Stable constants in $File::Find::name");
}
