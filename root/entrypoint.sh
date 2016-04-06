#!/bin/bash
source /root/.bashrc
ckan update
ckan scan
ckan upgrade --all
