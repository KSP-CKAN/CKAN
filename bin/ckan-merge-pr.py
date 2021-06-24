#!/usr/bin/python3

# https://github.com/KSP-CKAN/CKAN/wiki/Releasing-a-new-CKAN-client#when-merging-pull-requests

import sys
from pathlib import Path
from subprocess import run
from exitstatus import ExitStatus
import click
from click import command, option, argument
from git import Repo, Remote, RemoteReference, Commit
from github import Github, PullRequest
from typing import List, Dict

class CkanRepo(Repo):

    def remote_master(self) -> RemoteReference:
        return self.heads.master.tracking_branch()

    def master_remote(self) -> Remote:
        return self.remotes[self.remote_master().remote_name]

    def on_master(self) -> bool:
        return not self.head.is_detached and self.head.ref.name == 'master'

    def master_up_to_date(self) -> bool:
        print(f'Fetching {self.master_remote().name}...')
        self.master_remote().fetch()
        return self.heads.master.commit.hexsha == self.remote_master().commit.hexsha

    def changelog_path(self) -> Path:
        return Path(self.working_dir) / 'CHANGELOG.md'

    def prepend_line(self, path: Path, new_line: str) -> None:
        with open(path, 'r+') as changelog:
            lines = [new_line, *changelog.readlines()]
            changelog.seek(0)
            changelog.writelines(lines)

    def user_edit_file(self, path: Path) -> None:
        editor=self.config_reader().get('core', 'editor')
        run([editor, str(path)])

class CkanPullRequest:

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
        return [rvw.user.login for rvw in self.pull_request.get_reviews() if rvw.state == 'APPROVED']

    def badge(self) -> str:
        labels = {lbl.name for lbl in self.pull_request.labels}
        badges = {self.LABEL_BADGES[lbl] for lbl in labels if lbl in self.LABEL_BADGES}
        if len(badges) > 1:
            return 'Multiple'
        elif len(badges) == 1:
            return next(iter(badges))
        else:
            return 'UNKNOWN'

    def changelog_entry(self) -> str:
        return f'- [{self.badge()}] {self.pull_request.title} (#{self.pull_request.number} by: {self.pull_request.user.login}; reviewed: {", ".join(self.approvers())})\n'

    def latest_commit(self, repo: CkanRepo) -> Commit:
        print(f'Fetching {self.pull_request.head.repo.clone_url} {self.pull_request.head.ref}...')
        repo.git.fetch(self.pull_request.head.repo.clone_url, self.pull_request.head.ref)
        return repo.commit(self.pull_request.head.sha)

    def merge_commit_message(self) -> str:
        return f'Merge #{self.pull_request.number} {self.pull_request.title}'

    def merge_into(self, repo: CkanRepo) -> bool:
        if not self.approvers():
            print(f'PR #{self.pull_request.number} is not approved!')
            return False
        if not repo.on_master():
            print(f'Not on master branch!')
            return False
        if not repo.master_up_to_date():
            print(f'master branch is not up to date!')
            return False
        branch = self.latest_commit(repo)
        if not branch:
            print(f'PR #{self.pull_request.number} commit {self.pull_request.head.sha} not found!')
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
@option('--repo-path', type=click.Path(exists=True, file_okay=False),
        default='.', help='Path to CKAN working copy')
@option('--token', required=False, envvar='GITHUB_TOKEN')
@argument('pr_num', type=click.INT)
def merge_pr(repo_path: str, token: str, pr_num: int) -> None:
    ckr = CkanRepo(repo_path)
    ckpr = CkanPullRequest(Github(token).get_repo('KSP-CKAN/CKAN').get_pull(pr_num))
    sys.exit(ExitStatus.success
             if ckpr.merge_into(ckr)
             else ExitStatus.failure)

if __name__ == '__main__':
    merge_pr()
