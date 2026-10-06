# TVOS

TVOS turns a small Windows PC into a simple living-room streaming box. The goal is simple: turn on the TV, see Home, pick a service, and watch. Nobody using it should need to know Windows is underneath.

The original machine is an HP Pro Mini 400 G9 (Core i5-13500T, 16 GB RAM, Intel UHD 770) running Windows 11 Pro. The Onn/Vizio television is intended to stay offline; the PC handles internet streaming.

## Daily use

The finished path is: **PC boots or wakes → Windows signs into the dedicated TV account → TVOS opens full-screen → choose a service → Chrome opens it full-screen → Home returns to TVOS.**

TV volume and mute stay on the television. PC audio should remain at 100%.

TVOS currently targets Fubo, Netflix, Disney+, Prime Video, YouTube and Tubi, with custom services supported through configuration and the phone remote.

## What is included

- Full-screen WebView2 Home screen.
- Chrome app-mode streaming using the existing dedicated `Profile 1` profile.
- Global Home key that closes visible Chrome streaming windows and restores Home.
- Recovery when the visible Chrome window disappears.
- Local-network phone remote with D-pad, OK, Home, Back, Play/Pause, text entry, direct service launch and Sleep.
- Phone-side service manager for adding/removing services without a keyboard on the TV PC.
- Automatic artwork discovery/cache foundation plus built-in fallback artwork.
- Default, Halloween, Thanksgiving and Christmas themes. Auto mode uses Halloween in October, Thanksgiving in November and Christmas in December.
- Startup installer, updater and uninstaller.
- GitHub Actions self-contained Windows x64 build.
- No TVOS analytics, advertising, cloud account or telemetry.

## Repository layout

```
src/TVOS/                 C# WinForms/WebView2 application
web/launcher/             Interface shown on the television
web/remote/               Local phone remote and app manager
config/                    Default service/settings files
assets/services/           Built-in service artwork
assets/themes/             Theme artwork
scripts/                   Install/update/uninstall scripts
.github/workflows/         Windows build workflow
```

Installed files live under `C:\TV`: `app`, `web`, `assets`, `config`, `cache\artwork`, and `logs`. Updates are designed not to overwrite the user's live `services.json` or `settings.json`.

## Why Chrome and WebView2 are separate

TVOS Home is its own WebView2 application. Streaming websites open in Chrome with `--app`, `--start-fullscreen` and `--hide-scrollbars`. This keeps browser controls out of sight while preserving Chrome's DRM/cookie behavior. If Chrome closes or crashes, Home can recover independently.

TVOS does not store streaming passwords. Logins remain in the dedicated Chrome profile.

## Controls

During development, Arrow keys move, Enter selects, and the physical keyboard Home key is registered globally. Home works even while Chrome has focus.

The planned physical remote is a BOXPUT BPR1S Plus with its 2.4 GHz USB receiver. Its exact HID codes are deliberately not guessed. Once the actual remote is available, its buttons will be measured and mapped onto the actions TVOS already provides. TV power/volume/mute can remain IR/TV-side.

## Phone remote

TVOS runs a small local HTTP server on port 8765 by default. Open the phone-remote address shown from the Home settings button while the phone is on the same LAN.

A random token is generated on first run and required by control endpoints. This is intended for a trusted home LAN. The final PC should still be placed on a TV/IoT VLAN with firewall rules allowing the owner's phone to reach the TVOS port without giving the TV PC broad access to sensitive LAN devices.

The phone remote can also type text into the active streaming page and add/remove streaming services.

## Services and artwork

Services are data, not hard-coded buttons. Each has an ID, name, URL, icon, enabled state and order.

For custom services, the artwork subsystem can inspect the website for Open Graph images, Apple touch icons and favicons and cache a usable image locally. If discovery fails, Home falls back to a text tile. Built-in services ship with local fallback artwork so Home never depends on a third-party logo API.

## Themes

Set `theme` in settings to `auto`, `default`, `halloween`, `thanksgiving` or `christmas`. Auto selects by month. Themes only affect presentation.

## Power

The target machine supports Modern Standby and has already been verified to wake from a USB keyboard. TVOS can request sleep from the phone remote. The final BOXPUT Power/wake behavior will be tested against the real receiver rather than assumed.

Windows' own plugged-in display/sleep timers are expected to be disabled so TVOS controls the appliance experience. Active streaming should never be interrupted by an aggressive idle timer.

## Build

Development requirements are Windows 11, .NET 10 SDK, WebView2 Runtime and Google Chrome.

```powershell
dotnet restore .\src\TVOS\TVOS.csproj
dotnet build .\src\TVOS\TVOS.csproj -c Release
dotnet publish .\src\TVOS\TVOS.csproj -c Release -r win-x64 --self-contained true -o .\publish
```

GitHub Actions performs the self-contained publish too.

## Install on the TV PC

After publishing, run:

```powershell
.\scripts\install.ps1
```

The installer copies TVOS to `C:\TV` and adds `TVOS.exe` to the current user's Startup folder. It intentionally does not store or configure Windows account credentials. The dedicated `TV` account's automatic sign-in remains a one-time Windows setup step.

Once installed, normal viewers should never need the command line.

## Privacy

The TV can remain disconnected from the internet. TVOS itself has no tracking SDK, analytics, ads or cloud control service. The phone remote stays local.

Streaming providers still receive the normal traffic required to use their websites, and Windows/Chrome have their own privacy settings. TVOS does not pretend to make those services anonymous; it avoids adding another unnecessary data-collection layer.

## What still requires the real hardware

Source code cannot truthfully finalize the BOXPUT HID button map, receiver wake behavior, Onn HDMI/4K/HDR/HDCP settings, IR learning, or the resolution a provider currently chooses to deliver through Windows web playback. Those are isolated so we can test them later without redesigning the application.

## Maintenance

Use the password-protected Windows administrator account for Windows/driver maintenance. Everyday viewing belongs in the standard `TV` account. Do not use that account for general web browsing, email or downloads.
