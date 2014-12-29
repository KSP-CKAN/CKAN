#!/usr/bin/env python

import os, sys

BUILD_INFO_MESSAGE = """
*************************************************************
*************************************************************
*************************************************************

This CKAN build references the following repositories and commit hashes:
%s

You can fetch the source used for this build by running:
%s

*************************************************************
*************************************************************
*************************************************************
"""

def main():
    if len(sys.argv) < 2:
        print 'Usage:'
        print sys.argv[0] + ' <repository names>'
    
    repositories = sys.argv[1:]
    
    os.system('touch hashes')
    
    cwd = os.getcwd()
    
    for repo in repositories:
        os.chdir(os.path.join(cwd, repo))
        os.system('git rev-parse HEAD >> ../hashes')
    
    os.chdir(cwd)
    
    hashes = {}

    with open('hashes', 'r') as hashes_file:
        line_index = 0
        
        for line in hashes_file.readlines():
            hashes[repositories[line_index]] = line.strip()[:-1]
            line_index += 1
    
    repo_msg = ''
    fetch_msg = ''
    
    for repo, commit_hash in hashes.iteritems():
        repo_msg += '* %s - %s\n' % (repo, commit_hash)
        fetch_msg += 'git clone https://github.com/KSP-CKAN/%s; cd %s; git checkout %s; cd ..;\n' % (repo, repo, commit_hash)
        
    print BUILD_INFO_MESSAGE % (repo_msg, fetch_msg)
    
if __name__ == "__main__":
    main()
