#!/usr/bin/perl
use 5.010;
use strict;
use warnings;
use autodie;
use Test::More;
use Test::Exception;
use FindBin qw($Bin);
use lib "$Bin/lib";
use CkanTests;
use IPC::System::Simple qw(capturex capture);

my $CMDLINE = CkanTests->cmdline;

# If we're making a dev build, we may not have a tag to describe from.
diag "Any `No names found` messages are harmless, it just means a dev build.";
my $GIT_TAG = eval { capture("git describe --long --tags") };

my $version;

lives_ok { $version = capturex($CMDLINE, "version") } "ckan version execute";

SKIP: {
    unless ($GIT_TAG) { skip "Development build", 1; }

    chomp $GIT_TAG;
    like($version, qr/^\Q$GIT_TAG\E/, "Version should start with git tag");
}

done_testing;
