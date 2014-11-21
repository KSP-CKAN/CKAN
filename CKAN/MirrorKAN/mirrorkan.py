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

def dldb_create():
    if os.path.exists('db.json'):
        return
    
    with open('db.json', 'w') as db_file:
        db_file.write('{}');

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

def dldb_getlastmodified(filename):
    with open('db.json', 'r') as db_file:
        db = json.load(db_file)
        if filename not in db:
            return None
            
        return db[filename]

def zipdir(path, zip):
    for root, dirs, files in os.walk(path):
        for file in files:
            zip.write(os.path.join(root, file))

def dlfile(url, path, filename):
    print 'Downloading ' + url
    
    # Open the url
    f = urlopen(url)
    
    shouldDownload = False
    
    if 'last-modified' in f.headers:
        last_modified = f.headers['last-modified']
        print 'last-modified header time: ' + last_modified
        db_last_modified = dldb_getlastmodified(filename)
        if db_last_modified != None:
            print 'database last modified time: ' + db_last_modified
        
        shouldDownload = dldb_shoulddownload(filename, last_modified)
        dldb_write(filename, last_modified)
    else:
        print 'last-modified header not found'
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
    dldb_create()
    
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
    ckan_last_updated = {}
       
    for ckan_module in ckan_json:
        identifier = ckan_module[0]['identifier']
        version = ckan_module[0]['version']
        download_url = ckan_module[0]['download']
        mod_license = ckan_module[0]['license'] 
        
        ckan_file_availability[identifier] = 'OK!'
       
        filename = identifier + '-' + version + '.zip'
        download_file_url = LOCAL_URL_PREFIX + filename
        ckan_module[0]['download'] = download_file_url
        
        last_updated = dldb_getlastmodified(filename)
        if last_updated != None:
            ckan_last_updated[identifier] = last_updated
        else:
            ckan_last_updated[identifier] = 'last-modified header missing'
            
        if mod_license == 'restricted' or mod_license == 'unknown':
            ckan_file_availability[identifier] = 'Non-permissive license!'
            continue
       
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
        index += 'Last update: ' + str(datetime.datetime.now()) + '<br/>&nbsp;<br/>'
        index += '<a href="' + LOCAL_URL_PREFIX + 'master.zip">master.zip</a><br/>'
        index += '<a href="' + LOCAL_URL_PREFIX + 'log.txt">MirrorKAN log</a><br/>&nbsp;<br/>'
        
        index += 'Indexing ' + str(len(ckan_files)) + ' modules<br/>'
        index += 'Modules list:<br/>'
        
        for ckan_module in ckan_json:
            identifier = ckan_module[0]['identifier']
            version = ckan_module[0]['version']
            
            color = "#339900"
            if ckan_file_availability[identifier] != 'OK!':
                color = "#CC3300"
            
            index += '<font style="color: ' + color + ';">'
            
            index += '&nbsp;' + identifier + ' - ' + version + ' - '
            index += 'Status: ' + ckan_file_availability[identifier] + ' - '
            index += 'Last update: ' + ckan_last_updated[identifier] + '<br/>'
   
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
