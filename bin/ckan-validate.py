#!/usr/bin/env python

import os, sys

import json
from jsonschema import validate, ValidationError

def main():
    if len(sys.argv) == 1:
        print 'Usage:'
        print sys.argv[0] + ' <.ckan files>'
        sys.exit(0)
        
    SCHEMA_PATH = "CKAN.schema"
    if not os.path.exists(SCHEMA_PATH):
        print 'Cannot find JSON schema at %s' % SCHEMA_PATH
        SCHEMA_PATH = "../" + SCHEMA_PATH
        if not os.path.exists(SCHEMA_PATH):
            print 'Cannot find JSON schema at %s' % SCHEMA_PATH
            sys.exit(1)
        
    schema = None
        
    with open(SCHEMA_PATH, 'r') as schema_file:
        schema = json.load(schema_file)
        
    if schema == None:
        print 'Could not parse JSON schema, exiting..'
        sys.exit(1)
        
    files = sys.argv[1:]
    error = 0
    
    for ckan_path in files:
        if not os.path.exists(ckan_path):
            print 'File "%s" does not exist, skipping..' % ckan_path
            continue
        
        with open(ckan_path, 'r') as ckan_file:
            print 'Validating %s..' % ckan_path,
            try:
                validate(json.load(ckan_file), schema)
            except ValidationError as e:
                print 'Failed! See below for error description.'
                print e
                error = 1
                continue
            except ValueError as e:
                print 'Failed! This error will be cryptic, but often a JSON or property error'
                print e
                error = 1
                continue
            print 'Success!'

    sys.exit(error)

if __name__ == "__main__":
    main()
