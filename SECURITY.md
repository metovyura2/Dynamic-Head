# Security Policy

## Supported Versions

Only the latest release of Dynamic-Head receives security updates.

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security issue, please **do not** open a public issue.
Instead, report it privately:

1. Go to the repository's **Security** tab.
2. Click **Report a vulnerability** to open a private advisory.

Please include:

- A description of the issue and its impact.
- Steps to reproduce (or a proof of concept).
- The affected version and your Windows build.

You can expect an initial response within a few days. Once the issue is
confirmed and fixed, the advisory will be published and credit will be given to
the reporter (unless you prefer to stay anonymous).

## Scope

Dynamic-Head is a local desktop utility. It:

- Reads the list of audio playback devices via the Windows Core Audio API.
- Changes the default playback device via the `IPolicyConfig` COM interface.
- Optionally registers itself in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- Stores settings in a plain `config.json` file next to the executable.

It does not collect telemetry and does not communicate over the network.
