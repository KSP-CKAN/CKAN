#!/usr/bin/env python

GITHUB_API = 'https://api.github.com'

import os, sys
import urllib
import requests
import json
from urlparse import urljoin

def make_github_request(url_part, username, password, payload):
    url = urljoin(GITHUB_API, url_part)
    print '::make_github_request - %s' % url
    return requests.post(url, auth = (username, password), data = json.dumps(payload), verify=False)

def make_github_request_raw(url_part, username, password, payload, content_type):
    url = urljoin(GITHUB_API, url_part)
    print '::make_github_request_raw - %s' % url

    headers = { 'Content-Type': content_type }
    return requests.post(url, auth = (username, password), data = payload, verify=False, headers=headers)

def make_github_release(username, password, repo, tag_name, name, body, draft, prerelease):
    payload = {}
    payload['tag_name'] = tag_name
    payload['name'] = name
    payload['body'] = body
    payload['draft'] = draft
    payload['prerelease'] = prerelease
    return make_github_request('/repos/%s/releases' % repo, username, password, payload)

def make_github_release_artifact(username, password, upload_url, filepath, content_type = 'application/zip'):
    filename = os.path.basename(filepath)
    query = { 'name': filename }
    url = '%s?%s' % (upload_url[:-7], urllib.urlencode(query))
    payload = file(filepath, 'r').read()
    return make_github_request_raw(url, username, password, payload, content_type)

def main():
    if len(sys.argv) < 8:
        print 'Usage:'
        print sys.argv[0] + ' <user> <token> <repo> <tag> <name> <body> <file>'
        sys.exit(0)

    username = sys.argv[1]
    token = sys.argv[2]
    repo = sys.argv[3]
    tag = sys.argv[4]
    name = sys.argv[5]
    body = sys.argv[6]
    file_path = sys.argv[7]

    response = make_github_release(username, token, repo, tag, name, body, False, False)
    response_json = json.loads(response.text)

    if response.status_code == 201:
        print 'Release created successfully!'
    else:
        print 'There was an issue creating the release - %s' % response.text
        sys.exit(1)
     
    upload_url = response_json['upload_url']
 
    response = make_github_release_artifact(username, token, upload_url, file_path)
    if response.status_code == 201:
        print 'Asset successfully uploaded'
    else:
        print 'There was an issue uploading your asset!'
        sys.exit(1)

    sys.exit(0)
    
if __name__ == "__main__":
    main()
