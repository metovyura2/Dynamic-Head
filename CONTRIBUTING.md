# Contributing to Dynamic-Head

Thanks for taking the time to contribute! This document explains how to build
the project, the code style we follow, and how to submit changes.

## Ways to contribute

- Report bugs using the [bug report template](.github/ISSUE_TEMPLATE/bug_report.yml).
- Suggest features using the [feature request template](.github/ISSUE_TEMPLATE/feature_request.yml).
- Improve documentation, fix typos, or translate the README.
- Submit code changes via a pull request.

## Development environment

- **OS:** Windows 10 / 11, x64.
- **SDK:** [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).
- **IDE:** Visual Studio 2022, JetBrains Rider, or VS Code with the C# Dev Kit.

### Build

```powershell
# Using the helper script (recommended)
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1

# Or manually
dotnet restore .\src\DynamicHead\DynamicHead.csproj
dotnet build   .\src\DynamicHead\DynamicHead.csproj -c Release
dotnet publish .\src\DynamicHead\DynamicHead.csproj -c Release -r win-x64 --self-contained false -o .\dist
```

The published executable is `dist\DynamicHead.exe` and requires the
**.NET 8 Desktop Runtime** on the target machine.

## Code style

- Keep the existing structure and file responsibilities (see `AGENT.md` for the
  file map).
- **All code comments and UI strings are written in Russian.** This is a hard
  rule for this project.
- Do not add new dependencies beyond NAudio unless it is clearly justified and
  discussed in an issue first.
- Prefer small, focused classes with a single responsibility.
- Use nullable reference types (`Nullable` is enabled) and fix all warnings.
- Follow the existing naming conventions (`PascalCase` for types and members,
  `_camelCase` for private fields).

## Commit messages

- Write clear, imperative commit messages (e.g. `Add device fallback by name`).
- Reference the related issue when applicable (e.g. `Fix #12`).

## Pull request process

1. Fork the repository and create a topic branch from `main`.
2. Make your changes and verify the solution builds in `Release`.
3. Update `README.md` / `CHANGELOG.md` if behavior or features changed.
4. Open a pull request and fill in the template. Describe **what** changed and
   **why**, and include the steps to reproduce/verify.
5. A maintainer will review the change. Please be responsive to review comments.

## License

By contributing, you agree that your contributions are licensed under the
[MIT License](LICENSE).
