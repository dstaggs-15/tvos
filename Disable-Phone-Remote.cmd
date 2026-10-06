@echo off
title Disable BGFTOS Phone Remote
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\disable-phone-remote.ps1"
if errorlevel 1 pause
