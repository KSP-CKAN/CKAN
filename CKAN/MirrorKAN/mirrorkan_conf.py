# CKAN repo to clone
MASTER_REPO = "https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip"

# folder to store extracted ckan files
MASTER_ROOT_PATH = "master/"

# folder to store generated ckan files
LOCAL_CKAN_PATH = "ckan/"

# folder to store downloads and master.zip
FILE_MIRROR_PATH = "mirror/"

# server url - must point to the same location as FILE_MIRROR_PATH, trailing slash required!
LOCAL_URL_PREFIX = "http://128.199.55.88/"

# whether to generate an index.html in the mirror/ folder
GENERATE_INDEX_HTML = True

# the header text in index.html
INDEX_HTML_HEADER = "CKAN-meta mirror - DigitalOcean - Amsterdam"
