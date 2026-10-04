# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-01-01

### Added

- System tray icon with a single toggle between two audio output devices.
- Left-click on the tray icon to instantly switch the default output device.
- Context menu item "Switch to ..." that mirrors the toggle action.
- Two distinct tray icons (blue speakers / green headphones) showing the active device at a glance.
- Settings window to pick any two active playback devices and give them custom labels.
- "Run at startup" option stored in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- Settings persisted to `config.json` next to the executable.
- Default-device switching applied to all audio roles (Console, Multimedia, Communications).
- Fallback device lookup by name when the stored device ID is no longer valid.

[Unreleased]: https://github.com/metovyura2/Dynamic-Head/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/metovyura2/Dynamic-Head/releases/tag/v1.0.0
