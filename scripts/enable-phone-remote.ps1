$ErrorActionPreference="Stop"
function IsAdmin{$i=[Security.Principal.WindowsIdentity]::GetCurrent();$p=New-Object Security.Principal.WindowsPrincipal($i);$p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)}
if(-not(IsAdmin)){Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"";exit}
if(-not(Test-Path "C:\TV\app\BGFTOS.exe")){throw "BGFTOS is not installed at C:\TV."}
Get-NetFirewallRule -DisplayName "BGFTOS Phone Remote" -ErrorAction SilentlyContinue|Remove-NetFirewallRule -ErrorAction SilentlyContinue
New-NetFirewallRule -DisplayName "BGFTOS Phone Remote" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8765 -Program "C:\TV\app\BGFTOS.exe" -RemoteAddress LocalSubnet -Profile Private|Out-Null
Write-Host "BGFTOS phone remote access is enabled for Private networks and the local subnet only." -ForegroundColor Green
Write-Host "If Windows currently labels this network Public, the rule will not open the remote here."
Start-Sleep 4