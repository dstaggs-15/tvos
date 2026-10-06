$ErrorActionPreference="Stop"
function IsAdmin{$i=[Security.Principal.WindowsIdentity]::GetCurrent();$p=New-Object Security.Principal.WindowsPrincipal($i);$p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)}
if(-not(IsAdmin)){Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"";exit}
$packageRoot=(Resolve-Path(Join-Path $PSScriptRoot "..")).Path;$appSource=Join-Path $packageRoot "app"
if(-not(Test-Path(Join-Path $appSource "BGFTOS.exe"))){throw "BGFTOS.exe is missing. Extract the completed Production Build ZIP before installing."}
$consoleUser=(Get-CimInstance Win32_ComputerSystem).UserName;if([string]::IsNullOrWhiteSpace($consoleUser)){throw "No interactive Windows user is signed in."}
$userName=$consoleUser.Split("\")[-1];$sid=(New-Object Security.Principal.NTAccount($consoleUser)).Translate([Security.Principal.SecurityIdentifier]).Value
$profile=(Get-CimInstance Win32_UserProfile|Where-Object SID -eq $sid|Select-Object -First 1).LocalPath;if(!$profile){throw "Could not find the Windows profile for $consoleUser."}
$root="C:\TV";Write-Host "Installing BGFTOS for $consoleUser..." -ForegroundColor Cyan
Stop-Process -Name BGFTOS -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path "$root\app","$root\web","$root\assets","$root\config","$root\cache\artwork","$root\cache\webview","$root\logs"|Out-Null
Copy-Item "$appSource\*" "$root\app" -Recurse -Force;Copy-Item "$packageRoot\web\*" "$root\web" -Recurse -Force;Copy-Item "$packageRoot\assets\*" "$root\assets" -Recurse -Force;Copy-Item "$packageRoot\config\*.default.json" "$root\config" -Force
if(-not(Test-Path "$root\config\services.json")){Copy-Item "$root\config\services.default.json" "$root\config\services.json"}
if(-not(Test-Path "$root\config\settings.json")){Copy-Item "$root\config\settings.default.json" "$root\config\settings.json"}
$acl=Get-Acl $root;$rule=New-Object Security.AccessControl.FileSystemAccessRule($consoleUser,"Modify","ContainerInherit,ObjectInherit","None","Allow");$acl.SetAccessRule($rule);Set-Acl $root $acl
$startup=Join-Path $profile "AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup";New-Item -ItemType Directory -Force -Path $startup|Out-Null
$ws=New-Object -ComObject WScript.Shell;$sc=$ws.CreateShortcut((Join-Path $startup "BGFTOS.lnk"));$sc.TargetPath="$root\app\BGFTOS.exe";$sc.WorkingDirectory="$root\app";$sc.Save()
Get-NetFirewallRule -DisplayName "BGFTOS Phone Remote" -ErrorAction SilentlyContinue|Remove-NetFirewallRule -ErrorAction SilentlyContinue
if(-not(Test-Path "C:\Program Files\Google\Chrome\Application\chrome.exe")){Write-Warning "Chrome was not found. Install Google Chrome before streaming."}
$wv=Get-ChildItem "C:\Program Files (x86)\Microsoft\EdgeWebView\Application" -Directory -ErrorAction SilentlyContinue|Select-Object -First 1;if(-not$wv){Write-Warning "WebView2 Runtime was not detected. BGFTOS needs it for Home."}
Start-Process "$root\app\BGFTOS.exe"
Write-Host "Installed to C:\TV. BGFTOS will start automatically whenever $userName signs in." -ForegroundColor Green
Write-Host "Phone remote firewall access is DISABLED by default." -ForegroundColor Yellow
Write-Host "When this PC is on your trusted home network, run Enable-Phone-Remote.cmd from the BGFTOS package."
Start-Sleep 4