#!/bin/bash

if diff ./CKAN/CKAN.sln ./t/data/CKAN.sln >/dev/null ; then
  exit 0
else
  echo "The CKAN.sln project File has Changed, if this is deliberate"
  echo "copy the new file to:"
  echo "t/data/CKAN.sln"
  exit 1
fi
