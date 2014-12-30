#!/usr/bin/env python

"""
This script can create GitHub tags/ releases and push build artifacts to them.
"""

GITHUB_API = 'https://api.github.com'

# ---* DO NOT EDIT BELOW THIS LINE *---

import os, sys
import argparse

import urllib
import requests
from urlparse import urljoin

import datetime
import base64

import json

def make_github_post_request(url_part, username, password, payload):
    url = urljoin(GITHUB_API, url_part)
    print '::make_github_post_request - %s' % url
    return requests.post(url, auth = (username, password), data = json.dumps(payload), verify=False)

def make_github_get_request(url_path, username, password, payload):
    url = urljoin(GITHUB_API, url_path)
    print '::make_github_get_request - %s' % url
    return requests.get(url, auth = (username, password), data = json.dumps(payload), verify=False)

def make_github_post_request_raw(url_part, username, password, payload, content_type):
    url = urljoin(GITHUB_API, url_part)
    print '::make_github_post_request_raw - %s' % url

    headers = { 'Content-Type': content_type }
    return requests.post(url, auth = (username, password), data = payload, verify=False, headers=headers)

def make_github_release(username, password, repo, tag_name, name, body, draft, prerelease):
    payload = {}
    payload['tag_name'] = tag_name
    payload['name'] = name
    payload['body'] = body
    payload['draft'] = draft
    payload['prerelease'] = prerelease
    return make_github_post_request('/repos/%s/releases' % repo, username, password, payload)

def make_github_release_artifact(username, password, upload_url, filepath, content_type = 'application/zip'):
    filename = os.path.basename(filepath)
    query = { 'name': filename }
    url = '%s?%s' % (upload_url[:-7], urllib.urlencode(query))
    payload = file(filepath, 'r').read()
    return make_github_post_request_raw(url, username, password, payload, content_type)

def get_github_file(username, password, repo, path):
    return make_github_get_request('/repos/%s/contents/%s' % (repo, path), username, password, {})

def push_github_file(username, password, repo, path, sha, content, branch='master'):
    payload = {}
    payload['path'] = path
    payload['message'] = 'Updating build-tag'
    payload['content'] = base64.b64encode(content)
    payload['sha'] = sha
    payload['branch'] = branch

def main():
    parser = argparse.ArgumentParser(description='Create GitHub releases and upload build artifacts')

    parser.add_argument('--user', dest='user', action='store', help='Sets the GitHub user for the API', required=True)
    parser.add_argument('--token', dest='token', action='store', help='Sets the GitHub access token for the API', required=True)
    parser.add_argument('--repository', dest='repository', action='store', help='Sets the GitHub repository in which to make the release. Syntax: :owner/:repo', required=True)
    parser.add_argument('--tag', dest='tag', action='store', help='Sets the name of the tag that will be created for the release', required=True)
    parser.add_argument('--name', dest='name', action='store', help='Sets the name of the release that will be created', required=True)
    parser.add_argument('--body', dest='body', action='store', help='Sets the body text of the release', required=True)
    parser.add_argument('--draft', dest='draft', action='store_true', help='Sets the release as draft', required=False)
    parser.add_argument('--prerelease', dest='prerelease', action='store_true', help='Sets the release as a pre-release', required=False)
    parser.add_argument('--push-build-tag-file', dest='build_tag_file', action='store_true', help='Pushes a special build-tag file to the repository', required=False)
    parser.add_argument('artifacts', metavar='file', type=str, nargs='+', help='build artifact')
    args = parser.parse_args()
    
    if len(sys.argv) == 1:
        parser.print_help()
        sys.exit(0)

    if args.build_tag_file:
        response = get_github_file(args.user, args.token, args.repository, 'build-tag')
        if response.status_code != 201 and response.status_code != 304:
            print 'There was an issue fetching the build-tag file! - %s' % response.text
            sys.exit(1)
        
        response_json = json.loads(response.text)
    
        response = push_github_file(args.user, args.token, args.repository, response_json['path'], response_json['sha'], str(datetime.datetime.now()))
        if response.status_code == 201:
            print 'Build-tag file pushed to repository!'
        else:
            print 'There was an issue pushing the build-tag file! - %s' % response.text
            sys.exit(1)

    response = make_github_release(args.user, args.token, args.repository, args.tag, args.name, args.body, args.draft, args.prerelease)
    response_json = json.loads(response.text)

    if response.status_code == 201:
        print 'Release created successfully!'
    else:
        print 'There was an issue creating the release - %s' % response.text
        sys.exit(1)
     
    upload_url = response_json['upload_url']
    
    for artifact in args.artifacts:
        response = make_github_release_artifact(args.user, args.token, upload_url, artifact)
        if response.status_code == 201:
            print 'Asset successfully uploaded'
        else:
            print 'There was an issue uploading your asset! - %s' % response.text
            sys.exit(1)

    print 'Done!'
    sys.exit(0)
    
if __name__ == "__main__":
    main()
