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
    os.system('touch urls')
    
    cwd = os.getcwd()
    
    for repo in repositories:
        os.chdir(os.path.join(cwd, repo))
        os.system('git rev-parse HEAD >> ../hashes')
        os.system('git config --get remote.origin.url >> ../urls')
    
    os.chdir(cwd)
    
    hashes = {}
    urls = {}

    with open('hashes', 'r') as hashes_file:
        with open('urls', 'r') as urls_file:
            line_index = 0
            url_lines = urls_file.readlines()
        
            for line in hashes_file.readlines():
                hashes[repositories[line_index]] = line.strip()[:-1]
                urls[repositories[line_index]] = url_lines[line_index]
                line_index += 1
                
    repo_msg = ''
    fetch_msg = ''
    
    for repo, commit_hash in hashes.iteritems():
        repo_msg += '* %s - %s\n' % (repo, commit_hash)
        fetch_msg += 'git clone %; cd %s; git checkout %s; cd ..;\n' % (urls[repo], repo, commit_hash)
        
    print BUILD_INFO_MESSAGE % (repo_msg, fetch_msg)
    
if __name__ == "__main__":
    main()
