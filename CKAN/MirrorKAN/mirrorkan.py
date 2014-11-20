#!/usr/bin/env python

import os, sys
from os import listdir
from os.path import isfile, join
import tempfile
from urllib2 import urlopen, URLError, HTTPError
import zipfile
import json
import datetime

from mirrorkan_conf import *

def zipdir(path, zip):
    for root, dirs, files in os.walk(path):
        for file in files:
            zip.write(os.path.join(root, file))

def dlfile(url, path, filename):
    # Open the url
    try:
        f = urlopen(url)

        # Open our local file for writing
        with open(os.path.join(path, filename), "wb") as local_file:
            local_file.write(f.read())

    #handle errors
    except HTTPError, e:
		return None
    except URLError, e:
		return None
        
    return local_file.name
   
def parse_ckan_metadata(filename):
	data = None
	
	with open(filename) as json_file:
		data = json.load(json_file)
		
	return data
    
def find_files_with_extension(directory, extension):
	result_list = []
	
	file_list = [ f for f in listdir(directory) if isfile(join(directory, f)) ]
	for f in file_list:
		fileName, fileExtension = os.path.splitext(f)
		if fileExtension == extension:
			result_list += [os.path.join(directory, f)]
	
	return result_list
	
def update(master_repo, root_path, mirror_path):
	print 'Fetching remote master..',
	master_zip = dlfile(master_repo, '', 'master.zip')
	print 'Done!'
	
	with zipfile.ZipFile(master_zip, 'r') as zip_file:
		print 'Extracting master.zip..',
		zip_file.extractall(root_path)
		print 'Done!'
		
	print 'Looking for .ckan metadata files'
	ckan_files = find_files_with_extension(os.path.join(root_path, 'CKAN-meta-master'), '.ckan')
	print 'Found %i metadata files' % len(ckan_files)
	
	ckan_json = []
	
	print 'Parsing ' + str(len(ckan_files)) + ' modules'
	
	for ckan_file in ckan_files:
		print 'Parsing "%s"' % ckan_file
		ckan_module = parse_ckan_metadata(ckan_file)
		ckan_json += [[ckan_module, ckan_file]]
		
	for ckan_module in ckan_json:
		identifier = ckan_module[0]['identifier']
		version = ckan_module[0]['version']
		download_url = ckan_module[0]['download']
		
		filename = identifier + '-' + version + '.zip'
		download_file_url = LOCAL_URL_PREFIX + filename
		ckan_module[0]['download'] = download_file_url
		
		with open(os.path.join(LOCAL_CKAN_PATH, os.path.basename(ckan_module[1])), 'w') as out_ckan:
			json.dump(ckan_module[0], out_ckan)
			
		print 'Downloading "%s"' % download_url
		
		try:
			download_file = dlfile(download_url, FILE_MIRROR_PATH, filename)
		except:
			download_file = None
		
		if download_file == None:
			print 'Failed to download "%s", skipping..' % download_url
			continue
		
	# zip up all generated files 
	print 'Creating new master.zip'
	zipf = zipfile.ZipFile(os.path.join(FILE_MIRROR_PATH, 'master.zip'), 'w')
	zipdir(LOCAL_CKAN_PATH, zipf)
	zipf.close()
	
	# generate index.html
	if GENERATE_INDEX_HTML:
		index = ''
		index += 'CKAN-meta mirror - DigitalOcean - Amsterdam\n'
		index += 'Last update: ' + str(datetime.datetime.now()) + '\n'
		index += 'Indexing ' + str(len(ckan_files)) + ' modules\n'
		index += 'Modules list:\n'
		
		for ckan_module in ckan_json:
			identifier = ckan_module[0]['identifier']
			version = ckan_module[0]['version']
			index += '\t' + identifier + ' - ' + version + '\n'
			
		print 'Writing index.html'
		index_file = open(os.path.join(FILE_MIRROR_PATH, 'index.html'), 'w')
		index_file.write(index)
		index_file.close()
	
	print 'Done!'

def main():
	print 'Using "%s" as a remote' % MASTER_REPO
	print 'Master root is "%s"' % MASTER_ROOT_PATH
	print

	update(MASTER_REPO, MASTER_ROOT_PATH, FILE_MIRROR_PATH)

if __name__ == "__main__":
	main()
