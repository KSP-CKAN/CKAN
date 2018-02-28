#!/usr/bin/perl -w
use 5.010;
use strict;
use warnings;
use autodie;

use Net::GitHub;
use Date::Parse qw(str2time);

# Old support ticket closer. (GH #942)
#
# Finds tickets tagged with support, which haven't been active in 7
# days, which are unassigned, and which has comments, and closed them.
# Requires a github token. Ideally run from a bot account.
#
# License: Same as CKAN itself.

my $token = $ARGV[0] or die "Usage: $0 [token]\n";

my $github = Net::GitHub->new(
    RaiseError   => 1,
    access_token => $token,
);

# 7 days ago
my $date_cutoff = time() - 7 * 86400;

foreach my $repo (("CKAN", "NetKAN")) {
    $github->set_default_user_repo("KSP-CKAN", $repo);

    my $issues = $github->issue;

    # Get all our candidate issues

    my @candidates = $issues->repos_issues({
        state    => 'open',
        labels   => 'support',
        assignee => "none",
    });

    # Walk through each one, and see if we can close it

    foreach my $candidate (@candidates) {
        my $id     = $candidate->{number};
        my $title  = "$candidate->{title} (#$id)";
        my $author = $candidate->{user}{login};

        my $num_comments = +$candidate->{comments};
        if ($num_comments == 0) {
            say "Skipped (no comments)    : $title";
            next;
        }

        # Skip if last comment is by OP
        my @comments     = $issues->comments($id);
        my $last_comment = $comments[$num_comments - 1];
        if ($last_comment->{user}{login} eq $author) {
            say "Skipped (author comment) : $title";
            next;
        }

        my $last_update = str2time($candidate->{updated_at});

        if ($last_update > $date_cutoff) {
            say "Skipped (recent update)  : $title";
            next;
        }

        # Yay! Something we can close!
        say "Closing $title";

        close_ticket($issues, $id);
    }

}
say "Done!";

sub close_ticket {
    my ($issues, $id) = @_;

    $issues->create_comment($id, {
        body => "Hey there! I'm a fun-loving automated bot who's responsible for making sure old support tickets get closed out. As we haven't seen any activity on this ticket for a while, we're hoping the problem has been resolved and I'm closing out the ticket automaically. If I'm doing this in error, please add a comment to this ticket to let us know, and we'll re-open it!"
    });

    $issues->update_issue( $id, { state => "closed" });
}
