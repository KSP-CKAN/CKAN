#!/usr/bin/perl -w
use 5.010;
use strict;
use warnings;
use autodie;
use JSON;
use File::Slurp qw(read_file write_file);
use Getopt::Std;

# Find and optionally rewrite old mod versions that have overly vague
# KSP versions.
#
# This turns two-part KSP versions (eg: 0.90 or 1.0) into three-part
# versions (0.90.0 or 1.0.0) *if* we detect a later version that also
# specifies a two-part version with the same prefix. This was to fix #1156
# which could result in users being offered older releases when new KSP
# versions came out.
#
# You must be in the CKAN-meta directory to run this. Add the '-w' switch
# to rewrite files; otherwise it just reports what it found.
#
# Note: You may need to run this MULTIPLE times, as it only finds the
# first file in each sequence to have incorrect data.
#
# Note: This may potentially rewrite a new mod with a generalised version
# if an older copy of the same mod has a specific version, *and* the mods
# sequence differently in lexical order to CKAN-spec order.
#
# We probably won't need this code in the future as #1161 fixes the underlying
# bug, but I'm adding this to our toolbox just in case.

my %already_reported;
my %opts = (
    w => 0,     # Rewrite files containing bugs
);

my $json = JSON->new;

getopts('w',\%opts);

foreach my $dir (glob("*")) {
    # Skip anything but directories
    next if not -d $dir;

    my ($two_part_version, $two_part_file);

    foreach my $file (glob("$dir/*")) {
        # Skip anything but .ckan files
        next if $file !~ /\.ckan$/;

        my $ckan = eval { $json->decode(scalar read_file $file); };

        if ($@) { die "Error reading $file - $@"; }

        my $ksp_version = $ckan->{ksp_version};

        # Skip things without a KSP version
        next unless $ksp_version;

        # Check if this is a two-part version.
        if ($ksp_version =~ /^\d+\.\d+$/) {
            $two_part_version = $ksp_version;
            $two_part_file    = $file;
        }
        elsif ($two_part_version and $ksp_version =~ /^\Q$two_part_version\E\.\d+$/) {
            # A later release with a more specific version? That's a bug!

            next if $already_reported{$two_part_file}++;

            say "$two_part_file";
            say "# $two_part_file contains errorneous version $two_part_version obsoleted by $file";

            if ($opts{w}) {
                my $file_content = read_file $two_part_file;
                my $metadata = $json->decode($file_content);

                # Rather than simply adjust the JSON and rewrite (which causes a BIG diff)
                # we instead adjust the line in place (causing a small diff)

                $file_content =~ s{
                    ("ksp_version"\s*:\s*)"\Q$metadata->{ksp_version}\E"
                }{$1"$metadata->{ksp_version}.0"}msx
                    or die "Version substitution failed on:\n $file_content";

                write_file($two_part_file, $file_content);
                say "# Rewritten $two_part_file to use version $metadata->{ksp_version}.0";
            }
        }
    }
}
