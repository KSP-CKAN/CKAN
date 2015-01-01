#!/usr/bin/env python

import os, sys
import argparse


def main():
    parser = argparse.ArgumentParser(description='Builds CKAN from a list of commit hashes')

    parser.add_argument('--ckan-core-hash', dest='core_hash', action='store', help='The commit hash for CKAN-core', required=True)
    parser.add_argument('--ckan-gui-hash', dest='gui_hash', action='store', help='The commit hash for CKAN-GUI', required=True)
    parser.add_argument('--ckan-cmdline-hash', dest='cmdline_hash', action='store', help='The commit hash for CKAN-cmdline', required=True)
    args = parser.parse_args()
    
    build_ckan(core_hash, gui_hash, cmdline_hash)
    
if __name__ == "__main__":
    main()
