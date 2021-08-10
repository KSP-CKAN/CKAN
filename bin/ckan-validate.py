#!/usr/bin/env python3

from os.path import exists
from sys import argv, exit

from json import load
from jsonschema import validate, ValidationError

def get_schema(schema_path = "CKAN.schema"):
    if not exists(schema_path):
        schema_path = f"../{schema_path}"
        if not exists(schema_path):
            return not print(f"Cannot find JSON schema in the usual places")
    
    with open(schema_path, 'r') as schema_file:
        schema = load(schema_file)
        if schema == None:
            return print("Could not parse JSON schema, exiting..")
        else:
            return schema

def ckan_validates(ckan_paths):
    for ckan_path in ckan_paths:
        if exists(ckan_path):
            with open(ckan_path, 'r') as ckan_file:
                print(f"Validating {ckan_path}..")
                try:
                    validate(load(ckan_file), schema)
                    yield not print(f"Success!")
                except ValidationError as error:
                    yield print(
                        "Failed! See below for error description.\n", error)
                except ValueError as error:
                    yield print(
                        "Failed! This error will be cryptic,\n",
                        "but often a JSOqN or property error", error)
        else:
            print(f"File '{ckan_path}' does not exist, skipping..")

def main(ckan_paths):
    schema = get_schema()
    if not schema: return 1
    return all(ckan_validates(ckan_paths, schema))

if __name__ == "__main__":
    if len(argv) == 1: exit(print(f"Usage: {argv[0]} <.ckan files>"))
    else: exit(main(argv[1:]))



