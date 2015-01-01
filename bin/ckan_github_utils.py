GITHUB_API = 'https://api.github.com'

# ---* DO NOT EDIT BELOW THIS LINE *---

import urllib
import requests
from urlparse import urljoin

import datetime
import base64

import json
import shutil

def run_git_clone(repo, commit_hash):
    if os.path.isdir(repo):
        shutil.rmtree(repo)
    
    cwd = os.getcwd()
    if os.system('git clone git@github.com:KSP-CKAN/%s' % repo) != 0:
        sys.exit(1)
        
    os.chdir(os.path.join(cwd, repo))
    if os.system('git checkout -f %s' % commit_hash) != 0:
        sys.exit(1)
        
    os.chdir(cwd)

def build_repo(repo):
    cwd = os.getcwd()
    os.chdir(os.path.join(cwd, repo))

    if os.system('sh build.sh') != 0:
        sys.exit(1)

    os.chdir(cwd)
    
def build_ckan(core_hash, gui_hash, cmdline_hash):
    print 'Building CKAN from the following commit hashes:'
    print 'CKAN-core: %s' % args.core_hash
    print 'CKAN-GUI: %s' % args.gui_hash
    print 'CKAN-cmdline: %s' % args.cmdline_hash
    
    run_git_clone('CKAN-core', args.core_hash)
    run_git_clone('CKAN-GUI', args.gui_hash)
    run_git_clone('CKAN-cmdline', args.cmdline_hash)
    
    build_repo('CKAN-core')
    build_repo('CKAN-GUI')
    build_repo('CKAN-cmdline')
    
    print 'Done!'

def make_github_post_request(url_part, username, password, payload):
    url = urljoin(GITHUB_API, url_part)
    print '::make_github_post_request - %s' % url
    return requests.post(url, auth = (username, password), data = json.dumps(payload), verify=False)

def make_github_get_request(url_path, username, password, payload):
    url = urljoin(GITHUB_API, url_path)
    print '::make_github_get_request - %s' % url
    return requests.get(url, auth = (username, password), data = json.dumps(payload), verify=False)

def make_github_put_request(url_path, username, password, payload):
    url = urljoin(GITHUB_API, url_path)
    print '::make_github_put_request - %s' % url
    return requests.put(url, auth = (username, password), data = json.dumps(payload), verify=False)

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
    return make_github_put_request('/repos/%s/contents/%s' % (repo, path), username, password, payload)
