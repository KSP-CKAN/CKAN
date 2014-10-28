#!/usr/bin/perl
use 5.010;
use strict;
use warnings;
use autodie qw(:all);
use JSON::PP qw(decode_json);
use FindBin qw($Bin);
use Try::Tiny;

my $NETKAN_EXE = "$Bin/../netkan.exe";
my $NETKAN_DIR = "$Bin/../NetKAN";
my $CKAN_META  = "$Bin/../../CKAN-meta";

# Convert KerbalStuff and GitHub releases into CKAN metadata!
# It's the Networked Kerbal Archive Network. (NetKAN) :)

chdir($NETKAN_DIR);

foreach my $file (glob("*.ckan")) {

    say "$file...";

    try {
        system($NETKAN_EXE, "--outputdir=$CKAN_META", $file);
    }
    catch {
        warn "Processing $file FAILED\n";
    };
}
