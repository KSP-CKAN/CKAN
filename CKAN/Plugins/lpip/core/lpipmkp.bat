@echo off
cd ..
title CKAN Local Package Installer
color 7e
cls
echo .
echo KSP Local Package Installing Plug-In
echo (c)2015 Firelands.
echo .
echo Local package creator.
echo .
ping 127.0.0.1 -n 1 -w 1000 >nul
echo Press any key.
echo.
pause>nul.
cls
echo Tell the name of the new package to create.
set /P mkpack=
cls
echo Thanks! Creating package...
core\zip.exe -j -r makemods\ready\%mkpack%.lpf makemods\bin\%mkpack%
echo -------
echo .
echo Read the log above. When done, press a key to exit.
pause>nul
exit