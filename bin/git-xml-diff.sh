#!/bin/bash

# Find differences in string resources between differently formatted XML files

# DEPENDENCIES:
# sudo apt install xmlstarlet html-xml-utils libxml2-utils

FROM=${1:-master}
TO=${2:-HEAD}

function xnorm()
{
    # Strip comments
    # Remove boilerplate header elements
    # Standardize indentation
    cat | xmlstarlet canonic --without-comments - \
        | hxremove 'schema' , resheader \
        | xmllint --noblanks --format --encode UTF-8 -
}

for F in $(git diff --name-only $FROM $TO | grep resx)
do
    if ! OUT=$(diff -U 0 --label $FROM \
                         --label $TO \
                         <(git show $FROM:$F | xnorm) \
                         <(git show $TO:$F   | xnorm))
    then
        echo "${F}"
        echo "${OUT}"
        echo
    fi
done
