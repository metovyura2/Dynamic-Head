# Dynamic-Head

> One-click switcher between two audio output devices in the Windows system tray.

<!-- Badges -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6.svg)](#requirements)
[![Build](https://github.com/metovyura2/Dynamic-Head/actions/workflows/build.yml/badge.svg)](https://github.com/metovyura2/Dynamic-Head/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/metovyura2/Dynamic-Head)](https://github.com/metovyura2/Dynamic-Head/releases)
[![GitHub stars](https://img.shields.io/github/stars/metovyura2/Dynamic-Head?style=social)](https://github.com/metovyura2/Dynamic-Head/stargazers)

**Dynamic-Head** is a tiny Windows tray utility that toggles the default audio
output device between two configured endpoints — for example **Speakers ⇄
Headphones** — with a single left-click. No more digging through
`Settings → System → Sound` every time you plug in a headset.

- 🇬🇧 **English** (this file)
- 🇷🇺 [Русская версия](README.ru.md)

<!--
  Add screenshots to docs/screenshots/ and reference them here, e.g.:
  ![Tray menu](docs/screenshots/tray-menu.png)
-->

## Why Dynamic-Head?

Windows has no built-in one-click way to switch the default playback device.
Dynamic-Head sits in the system tray and gives you exactly that: one click, one
toggle, done. It is lightweight (a single small executable), has no installer,
and stores everything in a plain JSON file next to the app.

## Features

- 🔘 **Single tray toggle** — left-click the tray icon to instantly switch the
  default playback device.
- 🖱️ **Context menu item** — "Switch to «…»" does the same thing from the menu.
- 🎨 **Two distinct icons** — blue (speakers) and green (headphones) so you can
  see the active device at a glance.
- ⚙️ **Settings window** — pick any two active playback devices and give them
  custom labels.
- 🚀 **Run at startup** — optional autostart entry in
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- 💾 **Portable config** — settings live in `config.json` next to the executable.
- 🔊 **All roles** — switching applies to Console, Multimedia, and
  Communications audio roles at once.
- 🛟 **Robust lookup** — if a device ID changes (driver/port change), the app
  falls back to matching by device name.

## Requirements

- **Windows 10 / 11**, x64.
- [**.NET 8 Desktop Runtime**](https://dotnet.microsoft.com/download/dotnet/8.0)
  — required to run the prebuilt release.
- For building from source: **.NET SDK 8**.

## Download & Install

1. Go to the [**Releases**](https://github.com/metovyura2/Dynamic-Head/releases)
   page and download the latest `DynamicHead-*-win-x64.zip`.
2. Unpack the archive to any folder (e.g. `C:\Tools\Dynamic-Head`).
3. Make sure the **.NET 8 Desktop Runtime** is installed.
4. Run `DynamicHead.exe`. On first launch the settings window opens
   automatically.

There is no installer — the app is portable. To remove it, exit from the tray
menu and delete the folder.

## Usage

1. Run `DynamicHead.exe`. On the first run the **settings window** appears.
2. Choose **Device 1** (e.g. speakers) and **Device 2** (e.g. headphones). You
   can edit the labels shown in the tray menu.
3. Click **Save**. You can close the window — the app keeps running in the tray.
4. **Left-click** the tray icon to toggle. **Right-click** for the menu:
   - `Switch to «…»` — change the active device;
   - `Settings…` — open the settings window (also via double-click);
   - `Refresh device list` — re-sync with the system;
   - `Run at startup` — enable/disable autostart;
   - `Exit` — quit the app.

## Configuration (`config.json`)

The file is created next to the executable on first save:

```json
{
  "DeviceAId": "{0.0.0.00000000}.{...}",
  "DeviceAName": "Speakers (Realtek High Definition Audio)",
  "DeviceBId": "{0.0.0.00000000}.{...}",
  "DeviceBName": "Headphones (USB Audio)",
  "LabelA": "Speakers",
  "LabelB": "Headphones",
  "RunAtStartup": false,
  "ActiveSlot": "A"
}
```

## Build from source

```powershell
# Using the helper script (recommended)
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1

# Or manually
dotnet restore .\src\DynamicHead\DynamicHead.csproj
dotnet build   .\src\DynamicHead\DynamicHead.csproj -c Release
dotnet publish .\src\DynamicHead\DynamicHead.csproj -c Release -r win-x64 --self-contained false -o .\dist
```

The result is `dist\DynamicHead.exe` (framework-dependent). It requires the
**.NET 8 Desktop Runtime**.

> The helper script searches for `dotnet` in `D:\dotnet` first and then in
> `PATH`. On CI (GitHub Actions) the SDK is provided by `actions/setup-dotnet`.

## Project structure

```
src/DynamicHead/Program.cs        entry point, single-instance guard
src/DynamicHead/AppConfig.cs      settings model and config.json
src/DynamicHead/AudioSwitcher.cs  device enumeration and default-device switching
src/DynamicHead/TrayApp.cs        tray icon, menu, autostart
src/DynamicHead/SettingsForm.cs   settings window
src/DynamicHead/assets/           state icons (app-speakers.ico, app-headphones.ico)
tools/build.ps1                   build & publish to dist
tools/gen-icons.ps1               icon generator (not needed for build)
```

## How it works

- Device enumeration and the current default endpoint are read through the
  Windows **Core Audio API** using [NAudio](https://github.com/naudio/NAudio).
- Switching the default output device is done through the undocumented
  `IPolicyConfig` COM interface (`PolicyConfigClient`), applied to all three
  audio roles.
- Settings are serialized with `System.Text.Json` to `config.json` next to the
  executable.

## FAQ

**Does it work with Bluetooth headphones?**
Yes — any active playback device that appears in the Windows sound settings can
be selected.

**Do I need administrator rights?**
No. The app writes only to `HKCU` and to its own folder.

**Are there global hotkeys?**
Not yet. Control is via tray clicks. Feel free to open a feature request.

**Where are my settings stored?**
In `config.json` next to `DynamicHead.exe`. Delete the file to reset.

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) and
the [Code of Conduct](CODE_OF_CONDUCT.md) first. For security issues, see
[SECURITY.md](SECURITY.md).

## License

This project is licensed under the [MIT License](LICENSE).

---

### Keywords

`audio switcher` · `sound switcher` · `default audio device` · `switch audio
output` · `change default playback device` · `system tray` · `tray app` ·
`windows` · `windows 11` · `dotnet` · `c#` · `csharp` · `winforms` · `naudio` ·
`core audio` · `desktop utility` · `portable` · `смена устройства вывода звука`
· `переключение звука` · `аудио переключатель`

