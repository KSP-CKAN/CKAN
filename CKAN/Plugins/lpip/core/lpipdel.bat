@echo off
cd ..
title CKAN Local Package Installer
color 7e
cls
echo CKAN LPIP - Package Killer
echo
echo Say the name of a package to
echo delete.
set /P delpack=
cls
echo Thanks! Deleting...
del packages\%delpack%.lpf
cls
echo Done!
pause>nul
goto exit
:exit