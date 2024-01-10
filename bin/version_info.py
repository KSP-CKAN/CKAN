"""
Generate a JSON file containing the version and the latest changelog
"""

from pathlib import Path
import re
import json
from git import Repo

# Get the year and day of year from the latest commit
repo = Repo('.', search_parent_directories=True)
yyddd = repo.head.commit.committed_datetime.strftime(r'%g%j')
version = ''
changes = ''

with open(Path(repo.working_dir) / 'CHANGELOG.md',
          'rt', encoding='utf-8') as changelog:
    header_pattern = re.compile(r'^\s*\#\#\s+(v[0-9.]+)')
    # First header contains the current version
    for line in changelog:
        match = header_pattern.match(line)
        if match:
            version = match.group(1)
            break
    # Second header marks the end of the current changelog
    for line in changelog:
        if header_pattern.match(line):
            break
        changes += line

print(json.dumps({'version':   f'{version}.{yyddd}',
                  'changelog': changes.strip('\n')}, indent=4))
