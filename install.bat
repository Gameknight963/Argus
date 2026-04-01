@echo off
sc create Argus binPath= "%~dp0Argus.exe" start= auto
sc description Argus "Watches for DWM crash loops and kills suspect processes"
sc start Argus
echo Done
pause