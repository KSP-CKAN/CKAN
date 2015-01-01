#!/usr/bin/env python

from ckan_github_utils import *

import os, sys
import argparse

def main():
    parser = argparse.ArgumentParser(description='Promote nightly releases to production')

    parser.add_argument('--user', dest='user', action='store', help='Sets the GitHub user for the API', required=True)
    parser.add_argument('--token', dest='token', action='store', help='Sets the GitHub access token for the API', required=True)
    parser.add_argument('--dst-repository', dest='dst_repository', action='store', help='Sets the GitHub repository in which to make the release. Syntax: :owner/:repo', required=True)
    parser.add_argument('--src-repository', dest='src_repository', action='store', help='Sets the GitHub repository from which to take the release. Syntax: :owner/:repo', required=True)
    parser.add_argument('--tag', dest='tag', action='store', help='Sets the target tag', required=True)
    parser.add_argument('--name', dest='name', action='store', help='Sets the name of the release that will be created', required=True)
    parser.add_argument('--body', dest='body', action='store', help='Sets the body text of the release', required=True)
    parser.add_argument('--draft', dest='draft', action='store_true', help='Sets the release as draft', required=False)
    parser.add_argument('--prerelease', dest='prerelease', action='store_true', help='Sets the release as a pre-release', required=False)
    parser.add_argument('--push-build-tag-file', dest='build_tag_file', action='store_true', help='Pushes a special build-tag file to the repository', required=False)
    parser.add_argument('artifacts', metavar='file', type=str, nargs='+', help='build artifact')

if __name__ == "__main__":
    main()
