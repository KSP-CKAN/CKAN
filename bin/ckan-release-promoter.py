#!/usr/bin/env python

from ckan_github_utils import *

import os, sys
import argparse
import urllib2

def main():
    parser = argparse.ArgumentParser(description='Promote nightly releases to production')

    parser.add_argument('--user', dest='user', action='store', help='Sets the GitHub user for the API', required=True)
    parser.add_argument('--token', dest='token', action='store', help='Sets the GitHub access token for the API', required=True)
    parser.add_argument('--repository', dest='repository', action='store', help='Sets the GitHub repository in which to make the release. Syntax: :owner/:repo', required=True)
    parser.add_argument('--jenkins-build', dest='jenkins_build', action='store', help='The URL to the jenkins build that will be promoted', required=True)
    parser.add_argument('--release-version', dest='release_version', action='store', help='Sets the CKAN release version', required=True)
    parser.add_argument('--name', dest='name', action='store', help='Sets the name of the release that will be created', required=True)
    parser.add_argument('--draft', dest='draft', action='store_true', help='Sets the release as draft', required=False)
    parser.add_argument('--prerelease', dest='prerelease', action='store_true', help='Sets the release as a pre-release', required=False)
    args = parser.parse_args()
    
    build_log = urllib2.urlopen(args.jenkins_build + '/consoleText').read()
    build_tag = None

    for line in build_log.split('\n'):
        line = line.strip()
        if line.startswith('>BUILD_TAG'):
            build_tag = line.split(' ')[1]
            break
    
    if build_tag == None:
        print 'Error: >BUILD_TAG not found in build log, bailing out..'
        sys.exit(1)
    
    print 'Found build tag: ' + build_tag
    
    core_hash = ''
    gui_hash = ''
    cmdline_hash = ''

    for item in build_tag.split('|'):
        if len(item) == 0:
            continue
        
        repo, commit_hash = item.split('+')
        if repo == 'CKAN-core':
            core_hash = commit_hash
        elif repo == 'CKAN-GUI':
            gui_hash = commit_hash
        elif repo == 'CKAN-cmdline':
            cmdline_hash = commit_hash

    print 'Reproducing build..'
    
    build_ckan(core_hash, gui_hash, cmdline_hash, args.release_version + '-0-g0000000')
    os.system('mono ckan.exe --version')
    
    release_diff = ''
    
    
    response = push_github_file(args.user, args.token, args.repository, 'build-tag', build_tag)
    if response.status_code < 400:
        print 'Build-tag file pushed to repository!'
    else:
        print 'There was an issue pushing the build-tag file! - %s' % response.text
        sys.exit(1)
        
    body_text = """Promoted from [%s](%s)
* CKAN-core - %s
* CKAN-GUI - %s
* CKAN-cmdline - %s
    
---
Changes since last version:
%s
""" % (args.jenkins_build, args.jenkins_build, core_hash, gui_hash, cmdline_hash, release_diff)
        
    response = make_github_release(args.user, args.token, args.repository, args.release_version, args.name, body_text, args.draft, args.prerelease)
    response_json = json.loads(response.text)

    if response.status_code == 201:
        print 'Release created successfully!'
    else:
        print 'There was an issue creating the release - %s' % response.text
        sys.exit(1)
     
    upload_url = response_json['upload_url']
    
    response = make_github_release_artifact(args.user, args.token, upload_url, 'ckan.exe')
    if response.status_code == 201:
        print 'Asset successfully uploaded'
    else:
        print 'There was an issue uploading your asset! - %s' % response.text
        sys.exit(1)

    print 'Done!'
    sys.exit(0)

if __name__ == "__main__":
    main()
