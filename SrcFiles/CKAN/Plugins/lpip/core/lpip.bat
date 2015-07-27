@echo off
cd ..
title CKAN Local Package Installer
color 7e
cls
echo .
echo KSP Local Package Installing Plug-In
echo (c)2015 Firelands.
echo .
echo Local package installer
ping 127.0.0.1 /w 500 /n 1 >nul
echo .
echo Press any key to start installation.
pause >nul
goto begin
:begin
cls
echo You only need to tell the name of the package to install below.
set /P inspack=
cls
echo Thanks! Installing package %inspack%.
cd ..
cd ..
cd ..
CKAN\Plugins\lpip\core\unzip.exe CKAN\Plugins\lpip\packages\%inspack%.lpf -d GameData\%inspack% > CKAN\Plugins\lpip\lpip-log.txt
set logtime=%time%
set logdate=%date%
echo .
echo Done installing!
echo .
cd CKAN
cd Plugins
cd lpip
echo Press anything to continue.
pause >nul
goto final
:final
cls
echo Type R to retry, L to show log or anything else to exit.
set /P retry=
if /I %retry%==R goto begin
if /I %retry%==L goto log
goto exit
:log
cls
echo Showing log from %logdate%, %logtime%:
echo .
echo --------------
type lpip-log.txt
echo --------------
echo .
echo Press any key to return to retry screen.
pause >nul
goto final
:exit
cls
echo Exiting...
goto d
:d