# BGFT OS

BGFT OS turns a Windows mini PC into a living-room streaming appliance. The TV can stay offline; the PC handles streaming and presents a purpose-built interface instead of a Windows desktop.

The original target is an Mini PC running Windows 11 with a dedicated standard `TV` account and a separate administrator account.

## What normal use looks like

Power on or wake the PC. Windows signs into the TV account. BGFT OS starts automatically and fills the screen. Pick Fubo, Netflix, Disney+, Prime Video, YouTube or Tubi. The service opens in a dedicated Chrome profile in native full-screen. Press Home and the streaming window closes and BGFT OS returns.

Nobody watching TV should need Command Prompt, PowerShell, the Windows desktop, a browser toolbar or a keyboard.

## Production interface

The Home screen is branded **BGFT OS** and uses a layered blue/violet living-room UI rather than a plain desktop-style grid. It includes a persistent BGFT OS mark, clock/date, large service cards, focus animation, seasonal color themes and a system panel.

Built-in service artwork is stored locally so Home does not need to contact a logo service. Custom services can attempt to discover and cache artwork from the service's own Open Graph image, Apple touch icon or favicon.

## Architecture

```
Windows 11 TV account
        |
        +-- BGFT OS (WinForms)
              |
              +-- WebView2 Home UI
              +-- local phone-remote server
              +-- service/settings manager
              +-- artwork cache
              +-- power/input control
              |
              +-- Chrome dedicated TV profile
                    +-- Fubo / Netflix / Disney+ / Prime / YouTube / Tubi
```

Home and Chrome are deliberately separate. A streaming website cannot replace or destroy Home. If the visible Chrome streaming window disappears, BGFT OS brings Home back.

## Repository

```
src/TVOS/                 C# application
web/launcher/             television UI
web/remote/               phone remote and app manager
assets/brand/             BGFT OS identity
assets/services/          local service artwork
assets/themes/            seasonal theme assets
config/                    default service/settings data
scripts/                   install/update/uninstall
.github/workflows/         production Windows build
```

Installed runtime data lives under `C:\TV`. Live configuration is kept separate from application files so updates do not intentionally overwrite the user's service list or settings.

## Controls

Keyboard controls remain available for development and generic HID remotes: arrows navigate, Enter selects and Home is registered globally. Home works while Chrome has focus.

The intended physical remote is the BOXPUT BPR1S Plus using its 2.4 GHz USB receiver. Exact button codes are not guessed in source. The actual receiver will be measured later and its HID events mapped onto the already-existing Home, Back, directional, OK, media and sleep actions.

TV volume and mute stay on the TV side. The PC is intended to remain at full volume.

## Phone remote

BGFT OS runs a small local HTTP server. The System panel shows the phone URL. The phone remote provides service shortcuts, D-pad, OK, Home, Back, Play/Pause, text entry, Sleep and app management.

A random control token is generated on first run. API requests require it. The remote is local-network only; BGFT OS has no cloud remote service.

For the final network layout, the TV PC should live on a TV/IoT VLAN with firewall rules that let the owner's phone reach the remote port without giving the TV PC broad access to trusted devices.

## Adding apps

The phone app manager accepts a service name and a full HTTP/HTTPS address. The service appears in the local configuration. BGFT OS can then inspect the service's own website for suitable artwork and cache it locally. If discovery fails, Home renders a clean text card instead of breaking.

Service passwords are never stored by BGFT OS. Authentication remains inside the dedicated Chrome profile.

## Themes

Theme mode supports `auto`, `default`, `halloween`, `thanksgiving` and `christmas`. Auto uses Halloween in October, Thanksgiving in November and Christmas in December. Seasonal themes change the presentation without changing apps or browser data.

## Power

The original HP supports Modern Standby and has already been verified to wake from a USB keyboard. BGFT OS can request sleep from the phone remote. The BOXPUT receiver's actual wake behavior will be tested when the hardware arrives.

Windows' plugged-in automatic sleep/display timers are expected to remain disabled so Windows does not interrupt playback.

## Build

Development/build requirements are Windows, .NET 10 and the NuGet WebView2 package. Runtime streaming also requires Chrome and the WebView2 Runtime.

```powershell
dotnet restore .\src\TVOS\TVOS.csproj
dotnet build .\src\TVOS\TVOS.csproj -c Release
dotnet publish .\src\TVOS\TVOS.csproj -c Release -r win-x64 --self-contained true -o .\publish
```

Every push to `main` also runs the **Production Build** GitHub Actions workflow. It compiles, publishes a self-contained Windows x64 application, assembles the web/config/assets/scripts, creates `BGFT-OS-win-x64.zip`, and uploads it as a workflow artifact.

## Install

After obtaining a published build, the install script places BGFT OS under `C:\TV` and creates a Startup shortcut for the current user.

Automatic Windows sign-in is intentionally not configured by the repository because it involves local account credentials. On the original HP, that TV-account auto-login is already configured separately.

## Privacy

BGFT OS contains no analytics, advertising SDK, cloud account, telemetry service or third-party logo API. The television itself can remain disconnected from the internet.

Streaming providers still receive the traffic required to use their websites, and Chrome/Windows retain their own privacy behavior. BGFT OS does not claim to make third-party services anonymous.

## Hardware-dependent finishing work

The production software can be built before the final hardware mapping, but several facts can only be established on the actual setup: BOXPUT HID codes, receiver wake behavior, IR learning, final Onn HDMI/4K/HDR/HDCP behavior and the playback resolution each streaming provider actually supplies on Windows.

Those items are intentionally isolated from the application design. They can be tuned after installation without rebuilding the product from scratch.
