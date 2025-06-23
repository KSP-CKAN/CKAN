#!/bin/bash

# Extract all resource names from all resx files
# Exclude resources with GUI property names
# Sort and uniqify
# Exclude resources used in *.cs files like:
#    - Properties.Resources.ResourceName
#    - Description = "ResourceName" (for Display attribute)
#    - Name = "ResourceName" (for Display attribute)
# Print remaining resource names

find . -name '*.resx' -exec xq -x '//data/@name' '{}' ';' \
    | grep -v '\.' \
    | sort -u \
    | (while read R
    do
        if ! grep -rq --include='*.cs' "Properties\.Resources\.$R\|\(Name\|Description\) *= *\"$R\"" .
        then
            echo "$R"
        fi
    done)
