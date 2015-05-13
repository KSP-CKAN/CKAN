#!/usr/bin/perl -w
use 5.010;
use strict;
use warnings;
use autodie qw(:all);
use FindBin qw($Bin);
use Getopt::Std;

# Shows everything which changed since the last release.
# This should be easy, but the current build processes don't
# actually tag the repos. So we have to look in the `build-tag`
# file. Still a better love story than Twilight.

my %opts = (
    'f' => 0,           # Fetch from upstream
    'u' => 'origin',    # Upstream name
);

# Why doesn't C# have something this easy for option handling?

getopts('fu:', \%opts);

open(my $fh, '<', "$Bin/../build-tag");

my %last_release_commit;

my $tags = <$fh>;

foreach my $segment (split(/\|/,$tags)) {
    $segment =~ /^(?<repo>[^+]+)\+(?<commit>[0-9a-f]+)$/
        or die "Cannot parse $segment";

    $last_release_commit{$+{repo}} = $+{commit};
}

# Walk through each repo and display changes.

foreach my $repo (keys %last_release_commit) {
    chdir("$Bin/../$repo");

    say "\n== $repo ==\n";

    system("git fetch origin") if $opts{f};

    # By piping to cat, we avoid git invoking the pager.
    system(qq{git log --no-merges --pretty="format:%h %s (%aN)" --abbrev-commit $last_release_commit{$repo}..$opts{u}/master});
}
