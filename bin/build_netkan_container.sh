#!/bin/bash
cp -v Dockerfile.netkan _build/repack/$BUILD_CONFIGURATION/.
(cd _build/repack/$BUILD_CONFIGURATION/ && docker build . -f Dockerfile.netkan -t kspckan/inflator)
docker tag "kspckan/inflator" "kspckan/inflator:latest"
docker push "kspckan/inflator:latest"
