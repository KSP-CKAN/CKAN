#!/usr/bin/python3
"""CKAN pull request merging script

- Validates that the working copy is clean and ready for merge
- Validates that the pull request is approved
- Merges the PR's branch into main branch
- Updates the changelog based on pull request's properties, with user editing
- Formats the merge commit message as specified by this wiki:
  https://github.com/KSP-CKAN/CKAN/wiki/Releasing-a-new-CKAN-client#when-merging-pull-requests
"""

import sys
from pathlib import Path
from subprocess import run
from typing import List, Dict

from exitstatus import ExitStatus
import click
from click import command, option, argument
from git import Repo, Remote, RemoteReference
from git.objects import Commit
from github import Github
from github.PullRequest import PullRequest

class CkanRepo(Repo):
    """
    Represents a working copy of the CKAN git repo.
    Provides operations for validating branches and updating the changelog.
    """

    def remote_primary(self) -> RemoteReference:
        """Looks up the main branch in the repo on GitHub"""
        return next(filter(self.ref_is_head, self.refs))

    @staticmethod
    def ref_is_head(ref: RemoteReference) -> bool:
        """Checks whether reference is head"""
        return 'HEAD' in ref.name

    def primary_name(self) -> str:
        """Returns the name of the main branch (e.g. master or main)"""
        full = self.remote_primary().ref.name
        return full[full.index('/')+1:]

    def primary_remote(self) -> Remote:
        """Returns the remote where the upstream repo lives"""
        return self.remotes[self.remote_primary().remote_name]

    def on_primary_branch(self) -> bool:
        """True if the main branch is checked out, false otherwise"""
        return not self.head.is_detached and self.head.ref.name == self.primary_name()

    def primary_branch_up_to_date(self) -> bool:
        """True if the main branch is up to date with remote, false otherwise"""
        print(f'Fetching {self.primary_remote().name}...')
        self.primary_remote().fetch()
        return getattr(self.heads, self.primary_name()).commit.hexsha \
               == self.remote_primary().commit.hexsha

    def changelog_path(self) -> Path:
        """Path to the changelog file"""
        return Path(self.working_dir) / 'CHANGELOG.md'

    def prepend_line(self, path: Path, new_line: str) -> None:
        """Adds 'str' to the start of the file pointed to by 'path'"""
        with open(path, 'r+', encoding='utf-8') as changelog:
            lines = [new_line, *changelog.readlines()]
            changelog.seek(0)
            changelog.writelines(lines)

    def user_edit_file(self, path: Path) -> None:
        """Launch the editor configured in git to edit the given file"""
        editor = self.config_reader().get('core', 'editor')
        run([editor, str(path)], check=True)

class CkanPullRequest:
    """
    Represents a pull request in the CKAN repo on GitHub.
    Provides operations for validation and retrieving formatted info.
    """

    LABEL_BADGES: Dict[str, str] = {
        'Build':           'Build',
        'Cake':            'Build',
        'Core (ckan.dll)': 'Core',
        'Cmdline':         'CLI',
        'ConsoleUI':       'ConsoleUI',
        'GUI':             'GUI',
        'AutoUpdate':      'Updater',
        'Netkan':          'Netkan',
        'Spec':            'Spec',
    }

    def __init__(self, pull_request: PullRequest):
        self.pull_request = pull_request

    def approvers(self) -> List[str]:
        """Returns the list of users who approved the pull request"""
        return [rvw.user.login
                for rvw in self.pull_request.get_reviews()
                if rvw.state == 'APPROVED']

    def badge(self) -> str:
        """Returns the badge to use in the changelog based on the PR's labels"""
        labels = {lbl.name for lbl in self.pull_request.labels}
        badges = {self.LABEL_BADGES[lbl] for lbl in labels if lbl in self.LABEL_BADGES}
        if len(badges) > 1:
            return 'Multiple'
        elif len(badges) == 1:
            return next(iter(badges))
        else:
            return 'UNKNOWN'

    def changelog_entry(self) -> str:
        """
        Returns the string to add to the changeset for this PR.
        Format:  - [GUI] Title of PR (#1234 by: Developer; reviewed: Reviewers)
        """
        return (f'- [{self.badge()}]'
                f' {self.pull_request.title}'
                f' (#{self.pull_request.number}'
                f' by: {self.pull_request.user.login};'
                f' reviewed: {", ".join(self.approvers())})\n')

    def latest_commit(self, repo: CkanRepo) -> Commit:
        """Fetches changes for the given repo and returns this PR's branch's head commit"""
        print(f'Fetching {self.pull_request.head.repo.clone_url} {self.pull_request.head.ref}...')
        repo.git.fetch(self.pull_request.head.repo.clone_url, self.pull_request.head.ref)
        return repo.commit(self.pull_request.head.sha)

    def merge_commit_message(self) -> str:
        """
        Returns the string to use for the merge commit message.
        Format:  Merge #1234 Title of PR
        """
        return f'Merge #{self.pull_request.number} {self.pull_request.title}'

    def merge_into(self, repo: CkanRepo, self_review: bool) -> bool:
        """
        Merges this PR's changes into the working copy.
        Validates everything and updates/edits the changelog.
        """
        if not self_review and not self.approvers():
            print(f'PR #{self.pull_request.number} is not approved!')
            return False
        if not repo.on_primary_branch():
            print('Not on primary branch!')
            return False
        if not repo.primary_branch_up_to_date():
            print('Primary branch is not up to date!')
            return False
        branch = self.latest_commit(repo)
        if not branch:
            print(f'PR #{self.pull_request.number} commit {self.pull_request.head.sha} not found!')
            return False
        pr_commits = self.pull_request.get_commits()
        incomplete_checks = [run.name
                             for run
                             in pr_commits[pr_commits.totalCount - 1].get_check_runs()
                             if run.status != 'completed'
                                or run.conclusion not in ('success', 'skipped')]
        if incomplete_checks:
            print('Incomplete checks:', ', '.join(incomplete_checks))
            return False
        # Valid; do it!
        # repo.index.merge_tree doesn't auto resolve conflicts
        repo.git.merge(branch, no_commit=True, no_ff=True)
        repo.prepend_line(repo.changelog_path(), self.changelog_entry())
        repo.user_edit_file(repo.changelog_path())
        # repo.index.add() doesn't respect core.autocrlf
        repo.git.add(str(repo.changelog_path()))
        # repo.index.commit doesn't properly resolve a merge started via repo.git.merge
        repo.git.commit(m=self.merge_commit_message())
        return True

@command()
@option('--repo-path', required=False,
        type=click.Path(exists=True, file_okay=False),
        default='.', help='Path to CKAN working copy')
@option('--token', required=False, envvar='GITHUB_TOKEN')
@option('--self-review', required=False, is_flag=True, default=False)
@argument('pr_num', type=click.INT)
def merge_pr(repo_path: str, token: str, self_review: bool, pr_num: int) -> None:
    """Main driver; gets the repo and PR and attempts the merge"""
    ckr = CkanRepo(repo_path)
    ckpr = CkanPullRequest(Github(token).get_repo('KSP-CKAN/CKAN').get_pull(pr_num))
    sys.exit(ExitStatus.success
             if ckpr.merge_into(ckr, self_review)
             else ExitStatus.failure)

if __name__ == '__main__':
    merge_pr()  # pylint: disable=no-value-for-parameter
