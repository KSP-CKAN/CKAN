package CkanTests;
use 5.010;
use strict;
use warnings;
use FindBin qw($Bin);

=method cmdline

Returns the C<ckan.exe> command executable, or throws an
exception if not found.

=cut

sub cmdline {
    # This isn't the best way to locate our executable, but until
    # we have tests in sub-dirs, it should work.
    my $cmdline = "$Bin/../ckan.exe";
    -x $cmdline or die "Cannot find $cmdline";
    return $cmdline;
}

1;
