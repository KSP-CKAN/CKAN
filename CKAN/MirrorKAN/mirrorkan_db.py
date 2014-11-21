import json
import os
from dateutil.parser import parse

class Database:
    def __init__(self, path):
        self.path = path
        
        if os.path.exists(path):
            return
        
        with open(path, 'w') as db_file:
            db_file.write('{}')

    def add_mod(self, filename, lastModified):
        db = None
    
        with open(self.path, 'r') as db_file:
            db = json.load(db_file)
            db[filename] = lastModified
        
        if db is not None:
            with open(self.path, 'w') as db_file:
                json.dump(db, db_file)
    
    def is_newer(self, filename, lastModified):
        with open(self.path, 'r') as db_file:
            db = json.load(db_file)
            if filename not in db:
                return True
                
            lastModifiedCached = db[filename]
            dateCached = parse(lastModifiedCached)
            dateIncoming = parse(lastModified)
            
            if dateIncoming > dateCached:
                return True
            return False
        
        return True

    def get_lastmodified(self, filename):
        with open(self.path, 'r') as db_file:
            db = json.load(db_file)
            if filename not in db:
                return None
                
            return db[filename]
        return None
        
    def is_cached(self, filename):
        with open(self.path, 'r') as db_file:
            db = json.load(db_file)
            if filename not in db:
                return False
                
            return True
        return False
