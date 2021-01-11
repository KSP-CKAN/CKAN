#!/usr/bin/python3

# https://github.com/KSP-CKAN/CKAN/wiki/Releasing-a-new-CKAN-client#when-merging-pull-requests

import sys
from pathlib import Path
from subprocess import run
from exitstatus import ExitStatus
import click
from click import command, option, argument
from git import Repo
from github import Github

@command()
@option('--repo-path', type=click.Path(exists=True, file_okay=False),
        default='.', help='Path to CKAN working copy')
@option('--token', required=False, envvar='GITHUB_TOKEN')
@argument('pr_num', type=click.INT)
def merge_pr(repo_path: str, token: str, pr_num: int) -> None:
    r = Repo(repo_path)
    pr = Github(token).get_repo('KSP-CKAN/CKAN').get_pull(pr_num)
    # Make sure master is checked out
    if r.head.is_detached or r.head.ref.name != 'master':
        print('Not on master branch!')
        # Bail and let user get his or her repo in order
        sys.exit(ExitStatus.failure)
    # Make sure master is up to date
    master_remote = r.remotes[r.head.ref.tracking_branch().remote_name]
    print(f'Fetching {master_remote.name}...')
    master_remote.fetch()
    remote_master = r.head.ref.tracking_branch()
    if r.head.commit.hexsha != remote_master.commit.hexsha:
        print(f'master branch is not up to date!')
        sys.exit(ExitStatus.failure)
    # Get reviewers from PR
    reviewers = [rvw.user.login for rvw in pr.get_reviews() if rvw.state == 'APPROVED']
    if not reviewers:
        print(f'PR #{pr_num} is not approved!')
        sys.exit(ExitStatus.failure)
    # Get title from PR
    pr_title = pr.title
    # Get author from PR
    author = pr.user.login
    # Make sure we have the commits from the branch
    r.git.fetch(pr.head.repo.clone_url, pr.head.ref)
    # Get the commit
    branch = r.commit(pr.head.sha)
    if not branch:
        print(f'PR #{pr_num} commit {pr.head.sha} not found!')
        sys.exit(ExitStatus.failure)
    # Merge the branch with no-commit and no-ff
    base = r.merge_base(branch, r.head)
    r.index.merge_tree(branch, base=base)
    # Update the working copy
    r.index.checkout(force=True)
    # Print line to add to CHANGELOG.md at top of file, user needs to move it to the right spot
    changelog_path = Path(repo_path) / 'CHANGELOG.md'
    with open(changelog_path, 'r+') as changelog:
        lines = [f'- [UNKNOWN] {pr_title} (#{pr_num} by: {author}; reviewed: {", ".join(reviewers)})\n',
                 *changelog.readlines()]
        changelog.seek(0)
        changelog.writelines(lines)
    # Edit CHANGELOG.md
    editor=r.config_reader().get('core', 'editor')
    run([editor, changelog_path])
    # Stage change log
    r.index.add([changelog_path.as_posix()])
    # Commit
    r.index.commit(f'Merge #{pr_num} {pr_title}',
                   parent_commits=(r.head.commit, branch))

    # Don't push, let the user inspect and decide
    sys.exit(ExitStatus.success)

if __name__ == '__main__':
    merge_pr()
