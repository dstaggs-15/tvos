param([string]$PublishPath="$PSScriptRoot\..\publish")
Stop-Process -Name TVOS -Force -ErrorAction SilentlyContinue
Copy-Item "$PublishPath\*" "C:\TV\app" -Recurse -Force
Copy-Item "$PSScriptRoot\..\web\*" "C:\TV\web" -Recurse -Force
Copy-Item "$PSScriptRoot\..\assets\*" "C:\TV\assets" -Recurse -Force
Start-Process "C:\TV\app\TVOS.exe"