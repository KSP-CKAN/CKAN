# CKAN AutoUpdate

Auto-update tool for CKAN.

The core ckan.exe assembly checks for available updates and downloads them itself. However, a running executable cannot be overwritten on Windows. So to switch over to a newly downloaded release, ckan.exe also downloads AutoUpdate.exe and runs it. This tool then copies the new download over the original ckan.exe and runs it from that location.
