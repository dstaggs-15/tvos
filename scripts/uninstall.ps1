$startup=[Environment]::GetFolderPath("Startup")
Remove-Item "$startup\TVOS.lnk" -Force -ErrorAction SilentlyContinue
Stop-Process -Name TVOS -Force -ErrorAction SilentlyContinue
Write-Host "Startup entry removed. C:\TV was kept so settings are not lost."