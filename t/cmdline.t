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

# Without any config, we should be able to get a version.

my ($version, $git_tag);

lives_ok { $version = capturex($CMDLINE, "version") } "ckan version execute";
lives_ok { $git_tag = capture("git describe --long --tags") } "git version";

is($version, $git_tag, "Version should match git tag");

done_testing;
