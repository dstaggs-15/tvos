param([string]$PublishPath="$PSScriptRoot\..\publish")
$ErrorActionPreference="Stop"
$root="C:\TV"
if(-not(Test-Path $PublishPath)){throw "Publish folder not found. Run dotnet publish first."}
New-Item -ItemType Directory -Force -Path "$root\app","$root\web","$root\assets","$root\config","$root\cache\artwork","$root\logs"|Out-Null
Copy-Item "$PublishPath\*" "$root\app" -Recurse -Force
Copy-Item "$PSScriptRoot\..\web\*" "$root\web" -Recurse -Force
Copy-Item "$PSScriptRoot\..\assets\*" "$root\assets" -Recurse -Force
Copy-Item "$PSScriptRoot\..\config\*.default.json" "$root\config" -Force
if(-not(Test-Path "$root\config\services.json")){Copy-Item "$root\config\services.default.json" "$root\config\services.json"}
if(-not(Test-Path "$root\config\settings.json")){Copy-Item "$root\config\settings.default.json" "$root\config\settings.json"}
$startup=[Environment]::GetFolderPath("Startup");$ws=New-Object -ComObject WScript.Shell;$sc=$ws.CreateShortcut("$startup\TVOS.lnk");$sc.TargetPath="$root\app\TVOS.exe";$sc.WorkingDirectory="$root\app";$sc.Save()
Write-Host "TVOS installed. It will start automatically when this user signs in."