# Microsoft.Maui.Avalonia (experimental)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.1-blueviolet.svg)](https://avaloniaui.net/)
[![Status](https://img.shields.io/badge/state-preview-orange.svg)](#status)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

> This repository hosts an experimental backend that runs .NET MAUI applications on top of the Avalonia UI framework without modifying the upstream `dotnet/maui` sources.

## Status

- ✅ Application bootstrap (`AvaloniaMauiApplication`), window hosting, toolbar/menu chrome, and core controls (Button, Label, Entry, etc.) are implemented.
- ✅ Shell/Flyout navigation works, and the Avalonia diagnostics window can be attached automatically.
- ⚠️ Many MAUI handlers and Essentials APIs are still under construction (see [Phase 6 – parity plan](docs/avalonia-backend-plan.md#phase-6--parity-gap-inventory--execution-plan)).
- 💡 Expect frequent breaking changes while the backend evolves; do not use in production yet.

## Getting Started

### Prerequisites

- .NET 8 SDK (10.0.100-rc.2 or newer recommended).
- MAUI workloads restored locally: `dotnet workload restore`.
- Desktop OS with GUI session (macOS or Windows; Avalonia needs an actual windowing system).

### Build

```bash
git clone https://github.com/wieslawsoltes/MauiAvalonia.git
cd MauiAvalonia
dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

### Run with Hot Reload

```bash
dotnet watch run --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

Hot reload automatically applies XAML/C# edits; press `Ctrl+R` in the watch session to restart the app manually.

### Attach Avalonia DevTools

```bash
MAUI_AVALONIA_DEVTOOLS=1 dotnet run --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

The diagnostics window opens alongside the MAUI UI. Set the environment variable to `0` to disable.

## Repository Layout

```
docs/                         # Design notes and parity plan
samples/MauiAvalonia.SampleApp# MAUI template app bootstrapped via Avalonia
src/Microsoft.Maui.Avalonia/   # Backend handlers, services, hosting glue
extern/                        # Pinned submodules (Avalonia + dotnet/maui)
```

Key entry points:
- `src/Microsoft.Maui.Avalonia/MauiAvaloniaHostBuilderExtensions.cs` – `.UseMauiAvaloniaHost()` registrations.
- `src/Microsoft.Maui.Avalonia/AvaloniaMauiApplication.cs` – Avalonia application host that wires MAUI lifetimes.
- `docs/avalonia-backend-plan.md` – multi-phase plan plus open parity gaps.

## Contributing

1. Fork the repo and create a feature branch.
2. Keep changes scoped and update docs/tests when applicable.
3. Run `dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0` before submitting a PR.
4. Describe the parity gap or bug you are addressing and link to any relevant plan item.

Experimental work is welcome, but please call out any new limitations or TODOs in the pull request description.

## License

MIT License — see [LICENSE](LICENSE) for details. Avalonia and .NET MAUI remain under their respective licenses.
