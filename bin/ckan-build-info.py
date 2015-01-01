#!/usr/bin/env python

import os, sys

BUILD_INFO_MESSAGE = """
>BUILD_TAG %s
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
        repo_name = repo[:]
        if repo[-1] == '.':
            repo_name = repo[:-1]
        os.chdir(os.path.join(cwd, repo_name))
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
                hashes[repositories[line_index]] = line.strip()
                urls[repositories[line_index]] = url_lines[line_index].strip()
                line_index += 1
                
    repo_msg = ''
    fetch_msg = ''
    build_tag = ''
    
    for repo, commit_hash in hashes.iteritems():
        repo_name = repo[:]
        if repo[-1] == '.':
            repo_name = repo[:-1]
    
        build_tag += '%s+%s|'
        repo_msg += '* %s - %s\n' % (repo_name, commit_hash)
        rev_parse = ''
        if repo[-1] == '.':
            rev_parse = 'git fetch --tags --progress %s +refs/pull/*:refs/remotes/origin/pr/*; ' % urls[repo]
        fetch_msg += 'git clone %s; cd %s; %sgit checkout -f %s; cd ..;\n' % (urls[repo], repo_name, rev_parse, commit_hash)
        
    print BUILD_INFO_MESSAGE % (build_tag, repo_msg, fetch_msg)
    
if __name__ == "__main__":
    main()
