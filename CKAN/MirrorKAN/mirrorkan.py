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
    except HTTPError, e:
		return None
    except URLError, e:
		return None
        
    f.close()
    path = os.path.join(path, filename)
    os.system('wget -O ' + path + ' ' + url)
    return path
   
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
	print 'Cleaning up...',
	os.system('rm -R ' + FILE_MIRROR_PATH + '/*')
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
		
	print 'Looking for .ckan metadata files'
	ckan_files = find_files_with_extension(os.path.join(root_path, 'CKAN-meta-master'), '.ckan')
	print 'Found %i metadata files' % len(ckan_files)
	
	ckan_json = []
	
	print 'Parsing ' + str(len(ckan_files)) + ' modules'
	
	for ckan_file in ckan_files:
		print 'Parsing "%s"' % ckan_file
		ckan_module = parse_ckan_metadata(ckan_file)
		ckan_json += [[ckan_module, ckan_file]]
		
	# generate index.html
	if GENERATE_INDEX_HTML:
		index = '<html><head></head><body>'
		index += 'CKAN-meta mirror - DigitalOcean - Amsterdam<br/>'
		index += 'Last update: ' + str(datetime.datetime.now()) + '<br/>'
		index += 'Indexing ' + str(len(ckan_files)) + ' modules<br/>'
		index += 'Modules list:<br/>'
		
		for ckan_module in ckan_json:
			identifier = ckan_module[0]['identifier']
			version = ckan_module[0]['version']
			index += '&nbsp;' + identifier + ' - ' + version + '<br/>'
		
		index += '</body></html>'
			
		print 'Writing index.html'
		index_file = open(os.path.join(FILE_MIRROR_PATH, 'index.html'), 'w')
		index_file.write(index)
		index_file.close()
		
	for ckan_module in ckan_json:
		identifier = ckan_module[0]['identifier']
		version = ckan_module[0]['version']
		download_url = ckan_module[0]['download']
		
		filename = identifier + '-' + version + '.zip'
		download_file_url = LOCAL_URL_PREFIX + filename
		ckan_module[0]['download'] = download_file_url
			
		print 'Downloading "%s"' % download_url
		
		try:
			download_file = dlfile(download_url, FILE_MIRROR_PATH, filename)
		except:
			download_file = None
		
		if download_file == None:
			print 'Failed to download "%s", skipping..' % download_url
			continue
			
		with open(os.path.join(LOCAL_CKAN_PATH, os.path.basename(ckan_module[1])), 'w') as out_ckan:
			json.dump(ckan_module[0], out_ckan)
		
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
