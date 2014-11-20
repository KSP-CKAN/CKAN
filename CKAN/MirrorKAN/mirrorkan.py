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

from mirrorkan_conf import *

def dldb_write(filename, lastModified):
    db = None
    
    with open('db.json', 'r') as db_file:
        db = json.load(db_file)
        db[filename] = lastModified
    
    if db is not None:
        with open('db.json', 'w') as db_file:
            json.dump(db, db_file)
    
def dldb_shoulddownload(filename, lastModified):
    with open('db.json', 'r') as db_file:
        db = json.load(db_file)
        if filename not in db:
            return True
            
        lastModifiedCached = db[filename]
        dateCached = parse(lastModifiedCached)
        dateIncoming = parse(lastModified)
        
        if dateIncoming > dateCached:
            return True
        
        return False
    
    return True

def zipdir(path, zip):
    for root, dirs, files in os.walk(path):
        for file in files:
            zip.write(os.path.join(root, file))

def dlfile(url, path, filename):
    # Open the url
    f = urlopen(url)
    
    shouldDownload = False
    
    if 'last-modified' in f.headers:
        last_modified = f.headers['last-modified']
        shouldDownload = dldb_shoulddownload(filename, last_modified)
        
        if shouldDownload:
            dldb_write(filename, last_modified)
        else:
            pass
    else:
        shouldDownload = True
        
    f.close()
    
    path = os.path.join(path, filename)
    
    if shouldDownload:
        os.system('wget -O ' + path + ' ' + url)
    else:
        print 'Latest version already in cache, skipping..'
    
    return path
   
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
    for f in file_list:
        fileName, fileExtension = os.path.splitext(f)
        if fileExtension == extension:
            result_list += [os.path.join(directory, f)]
    
    return result_list
    
def update(master_repo, root_path, mirror_path):
    print 'Cleaning up...',
    os.system('rm -R ' + LOCAL_CKAN_PATH + '/*')
    os.system('rm -R ' + MASTER_ROOT_PATH + '/*')
    print 'Done!'
    
    print 'Fetching remote master..',
    master_zip = dlfile(master_repo, '', 'master.zip')
    print 'Done!'
    
    with zipfile.ZipFile(master_zip, 'r') as zip_file:
        print 'Extracting master.zip..',
        zip_file.extractall(root_path)
        print 'Done!'
  
    ckan_files, ckan_json = parse_ckan_metadata_directory(os.path.join(root_path, 'CKAN-meta-master'))
    ckan_file_availability = {}
       
    for ckan_module in ckan_json:
        identifier = ckan_module[0]['identifier']
        version = ckan_module[0]['version']
        download_url = ckan_module[0]['download']
        
        ckan_file_availability[identifier] = 'OK!'
        
        filename = identifier + '-' + version + '.zip'
        download_file_url = LOCAL_URL_PREFIX + filename
        ckan_module[0]['download'] = download_file_url
            
        print 'Downloading "%s"' % download_url
        
        try:
            download_file = dlfile(download_url, FILE_MIRROR_PATH, filename)
        except HTTPError, e:
            ckan_file_availability[identifier] = 'HTTP Error: ' + str(e)
            print 'HTTPError: ' + str(e)
            continue
        except URLError, e:
            ckan_file_availability[identifier] = 'URL Error: ' + str(e)
            print 'URLError: ' + str(e)
            continue

        print 'Dumping json for ' + identifier

        with open(os.path.join(LOCAL_CKAN_PATH, os.path.basename(ckan_module[1])), 'w') as out_ckan:
            json.dump(ckan_module[0], out_ckan)
      
    # generate index.html
    if GENERATE_INDEX_HTML:
        index = '<html><head></head><body>'
        index += INDEX_HTML_HEADER + '<br/>&nbsp;<br/>'
        index += 'Last update: ' + str(datetime.datetime.now()) + '<br/>'
        index += 'Indexing ' + str(len(ckan_files)) + ' modules<br/>'
        index += 'Modules list:<br/>'
        
        for ckan_module in ckan_json:
            identifier = ckan_module[0]['identifier']
            version = ckan_module[0]['version']
            index += '&nbsp;' + identifier + ' - ' + version + ' - '
            index += 'status: ' + ckan_file_availability[identifier] + '<br/>'
        
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
