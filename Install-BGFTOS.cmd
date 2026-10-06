@echo off
title BGFTOS Installer
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\install.ps1"
if errorlevel 1 (echo.&echo BGFTOS installation did not complete.&pause)
