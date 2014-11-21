#!/usr/bin/env python

import os, sys
from os import listdir
from os.path import isfile, join
import tempfile
from urllib2 import urlopen, URLError, HTTPError
import zipfile
import json
import datetime
from dateutil.parser import parse
from email.Utils import formatdate

from mirrorkan_conf import *
from mirrorkan_db import *

db = Database('db.json')

def zipdir(path, zip):
    for root, dirs, files in os.walk(path):
        for file in files:
            zip.write(os.path.join(root, file))

def download_file(url, path, filename):
    os.system('wget -O "' + os.path.join(path, filename) + '" "' + url + '"')
    
DLRESULT_SUCCESS = 1
DLRESULT_CACHED = 2
DLRESULT_HTTP_ERROR_CACHED = 3
DLRESULT_HTTP_ERROR_NOT_CACHED = 4

def download_mod(url, path, filename):
    print 'Downloading ' + url
    
    f = None
    
    error = None
    
    is_cached = db.is_cached(filename)
    
    # Open the url
    try:
        f = urlopen(url)
    except HTTPError, e:
        error = e
        print str(e) + ': ' + url
    except URLError, e:
        error = e
        print str(e) + ': ' + url
    
    if error is not None:
        if is_cached:
            return DLRESULT_HTTP_ERROR_CACHED
        else:
            return DLRESULT_HTTP_ERROR_NOT_CACHED
    
    should_download = False
    
    if 'last-modified' in f.headers:
        last_modified = f.headers['last-modified']
        print 'last-modified header time: ' + last_modified
        
        db_last_modified = db.get_lastmodified(filename)
        if db_last_modified != None:
            print 'database last modified time: ' + db_last_modified
        
        if not db.is_newer(filename, last_modified):
            return DLRESULT_CACHED
        
        db.add_mod(filename, last_modified)
    elif is_cached:
        print 'last-modified header not found'
        return DLRESULT_CACHED
     
    download_file(url, path, filename)
    
    return DLRESULT_SUCCESS
   
def parse_ckan_metadata(filename):
    data = None
    
    with open(filename) as json_file:
        data = json.load(json_file)
        
    return data
    
def parse_ckan_metadata_directory(path):
    print 'Looking for .ckan metadata files in ' + path
    ckan_files = find_files_with_extension(path, '.ckan')
    print 'Found %i metadata files' % len(ckan_files)
    
    ckan_json = []
    
    for ckan_file in ckan_files:
        print 'Parsing "%s"' % ckan_file
        ckan_module = parse_ckan_metadata(ckan_file)
        ckan_json += [[ckan_module, ckan_file]]
        
    return (ckan_files, ckan_json)
    
def find_files_with_extension(directory, extension):
    result_list = []
    
    file_list = [ f for f in listdir(directory) if isfile(join(directory, f)) ]
    file_list.sort()
    
    for f in file_list:
        fileName, fileExtension = os.path.splitext(f)
        if fileExtension == extension:
            result_list += [os.path.join(directory, f)]
    
    return result_list
    
def clean_up():
    print 'Cleaning up...',
    os.system('rm -R ' + LOCAL_CKAN_PATH + '/*')
    os.system('rm -R ' + MASTER_ROOT_PATH + '/*')
    print 'Done!'
    
def fetch_and_extract_master(master_repo, root_path):
    print 'Fetching remote master..',
    download_file(master_repo, '', 'master.zip')
    print 'Done!'
    
    with zipfile.ZipFile('master.zip', 'r') as zip_file:
        print 'Extracting master.zip..',
        zip_file.extractall(root_path)
        print 'Done!'

def dump_all_modules(ckan_files, ckan_json):
    ckan_mod_file_status = {}
    ckan_mod_status = {}
    ckan_last_updated = {}
    
    for ckan_module in ckan_json:
        identifier = ckan_module[0]['identifier']
        version = ckan_module[0]['version']
        download_url = ckan_module[0]['download']
        mod_license = ckan_module[0]['license'] 
        
        filename = identifier + '-' + version + '.zip'
        
        ckan_mod_status[filename] = ''
       
        download_file_url = LOCAL_URL_PREFIX + filename
        
        last_updated = db.get_lastmodified(filename)
        if last_updated is not None:
            ckan_last_updated[filename] = last_updated
        else:
            ckan_last_updated[filename] = 'last-modified header missing'
            
        if mod_license is 'restricted' or mod_license is 'unknown':
            ckan_module[0]['download'] = download_file_url
        else:
            file_status = download_mod(download_url, FILE_MIRROR_PATH, filename)
            ckan_mod_file_status[filename] = file_status
            
            if file_status is DLRESULT_SUCCESS:
                print 'Success!'
                ckan_mod_status[filename] = 'Just updated'
            elif file_status is DLRESULT_CACHED:
                ckan_mod_status[filename] = 'Cached, no updates'
                print 'Cached'
            elif file_status is DLRESULT_HTTP_ERROR_CACHED:
                ckan_mod_status[filename] = 'Cached, http error'
                print 'HTTP Error (Cached)'
            elif file_status is DLRESULT_HTTP_ERROR_NOT_CACHED:
                print 'HTTP Error (Not cached)'
                ckan_mod_status[filename] = 'Not cached, http error'
        
        print 'Dumping json for ' + identifier

        with open(os.path.join(LOCAL_CKAN_PATH, os.path.basename(ckan_module[1])), 'w') as out_ckan:
            json.dump(ckan_module[0], out_ckan)

    return (ckan_mod_file_status, ckan_mod_status, ckan_last_updated)

def update(master_repo, root_path, mirror_path):    
    clean_up()
    fetch_and_extract_master(master_repo, root_path)
  
    ckan_files, ckan_json = parse_ckan_metadata_directory(os.path.join(root_path, 'CKAN-meta-master'))
    ckan_mod_file_status, ckan_mod_status, ckan_last_updated = dump_all_modules(ckan_files, ckan_json)
      
    # generate index.html
    if GENERATE_INDEX_HTML:
        mods_ok = 0
        mods_error = 0
        
        for ckan_module in ckan_json:
            identifier = ckan_module[0]['identifier']
            version = ckan_module[0]['version']
            filename = identifier + '-' + version + '.zip'
            
            if ckan_mod_file_status[filename] is not DLRESULT_HTTP_ERROR_NOT_CACHED:
                mods_ok += 1
            else:
                mods_error += 1
        
        index = '<html><head></head><body>'
        index += INDEX_HTML_HEADER + '<br/>&nbsp;<br/>'
        index += 'Last update: ' + str(datetime.datetime.now()) + '<br/>&nbsp;<br/>'
        index += '<a href="' + LOCAL_URL_PREFIX + 'master.zip">master.zip</a><br/>'
        index += '<a href="' + LOCAL_URL_PREFIX + 'log.txt">MirrorKAN log</a><br/>&nbsp;<br/>'
        
        index += 'Indexing ' + str(len(ckan_files)) + ' modules - '
        index += '<font style="color: #339900; font-weight: bold;">'
        index += str(mods_ok) + ' ok'
        index += '</font>'
        index += ', <font style="color: #CC3300; font-weight: bold;">'
        index += str(mods_error) + ' failed'
        index += '</font>'
        index += '<br/>&nbsp;<br/>'
        index += 'Modules list:<br/>'
        
        for ckan_module in ckan_json:
            identifier = ckan_module[0]['identifier']
            version = ckan_module[0]['version']
            filename = identifier + '-' + version + '.zip'
            
            style = "color: #339900;"
            if ckan_mod_file_status[filename] is DLRESULT_HTTP_ERROR_NOT_CACHED:
                style = "color: #CC3300; font-weight: bold;"
            elif ckan_mod_file_status[filename] is DLRESULT_HTTP_ERROR_CACHED:
                style = "color: #FFD700; font-weight: bold;"
            
            index += '<font style="' + style + '">'
            
            index += '&nbsp;' + identifier + ' - ' + version + ' - '
            index += 'Status: ' + ckan_mod_status[filename] + '(' + ckan_mod_file_status[filename] + ') - '
            index += 'Last update: ' + ckan_last_updated[filename] + '<br/>'
            
            index += '</font>'
        
        index += '</body></html>'

        print 'Writing index.html'
        index_file = open(os.path.join(FILE_MIRROR_PATH, 'index.html'), 'w')
        index_file.write(index)
        index_file.close()
    
    # zip up all generated files 
    print 'Creating new master.zip'
    zipf = zipfile.ZipFile(os.path.join(FILE_MIRROR_PATH, 'master.zip'), 'w')
    zipdir(LOCAL_CKAN_PATH, zipf)
    zipf.close()
    
    print 'Done!'

def main():
    print 'Using "%s" as a remote' % MASTER_REPO
    print 'Master root is "%s"' % MASTER_ROOT_PATH
    print

    update(MASTER_REPO, MASTER_ROOT_PATH, FILE_MIRROR_PATH)

if __name__ == "__main__":
    main()
