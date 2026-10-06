$ErrorActionPreference="Stop"
function IsAdmin{$i=[Security.Principal.WindowsIdentity]::GetCurrent();$p=New-Object Security.Principal.WindowsPrincipal($i);$p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)}
if(-not(IsAdmin)){Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"";exit}
Get-NetFirewallRule -DisplayName "BGFTOS Phone Remote" -ErrorAction SilentlyContinue|Remove-NetFirewallRule -ErrorAction SilentlyContinue
Write-Host "BGFTOS phone remote firewall access is disabled." -ForegroundColor Green
Start-Sleep 3