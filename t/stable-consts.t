#!/usr/bin/perl
use Test::Most;
use IPC::System::Simple qw(capture);
use FindBin qw($Bin);
use File::Find;

# If our branch has a "stable-looking" name, then make sure we have stable
# build symbols defined. Otherwise, we don't care.

# We'll examine both the current branch, and the branch in $ENV{TRAVIS_BRANCH}
# if it exists. This means that PRs to stable branches will always be tested,
# even if their former branch names missed it.

# Stable looking branches have the word 'stable' in them, or end in a
# `vx.y` number, with 'y' being even. (Eg: `release-v1.0` is stable,
# as is `235_stable_bugfix_foo)

# If the CKAN_STABLE environment variable is set, we're always considered stable.

unless (is_stable()) {
    plan skip_all => "Not a stable branch";
}

# Find all our .csproj files and test them.
find(\&test_csproj, "$Bin/../CKAN");

done_testing;

sub is_stable {
    return 1 if $ENV{CKAN_STABLE};

    my $branch = capture("git rev-parse --abbrev-ref HEAD");
    chomp $branch;

    my $travis = $ENV{TRAVIS_BRANCH} || "";

    diag "Checking $branch / $travis for stability";

    foreach ($branch, $travis) {
        return 1 if m{
            (\b|_)stable(\b|_)|   # Contains stable as a word (underscores ok)
            v\d+\.\d*[02468]$     # Ends with vx.y, where y is even.
        }x;
    }

    # Nope, not stable.
    return 0;
}

sub test_csproj {
    return unless /\.csproj$/;  # Only process .csproj files

    local $/;   # File-slurp on read mode.

    open(my $fh, '<', $_);

    my $contents = <$fh>;

    # We're not too picky, just make sure STABLE is in a DefineConstants somewhere...

    ok($contents =~ m{<DefineConstants>[^<]*\bSTABLE\b}, "Stable constants in $File::Find::name");
}
