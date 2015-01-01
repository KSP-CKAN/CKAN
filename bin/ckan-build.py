#!/usr/bin/env python

from ckan_github_utils import * 

import os, sys
import argparse

def main():
    parser = argparse.ArgumentParser(description='Builds CKAN from a list of commit hashes')

    parser.add_argument('--ckan-core-hash', dest='core_hash', action='store', help='The commit hash for CKAN-core', required=False)
    parser.add_argument('--ckan-gui-hash', dest='gui_hash', action='store', help='The commit hash for CKAN-GUI', required=False)
    parser.add_argument('--ckan-cmdline-hash', dest='cmdline_hash', action='store', help='The commit hash for CKAN-cmdline', required=False)
    parser.add_argument('--ckan-release-version', dest='release_version', action='store', help='The version with which to stamp ckan.exe', required=False)
    args = parser.parse_args()
    
    build_ckan(args.core_hash, args.gui_hash, args.cmdline_hash, args.release_version)
    
if __name__ == "__main__":
    main()
