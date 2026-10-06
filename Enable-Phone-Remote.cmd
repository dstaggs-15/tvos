@echo off
title Enable BGFTOS Phone Remote
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\enable-phone-remote.ps1"
if errorlevel 1 pause
