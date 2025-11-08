# Running the Avalonia sample app

This repository already wires the MAUI sample to the Avalonia backend, so once the .NET workloads are restored you can launch the desktop build directly from the command line.

## Prerequisites
- .NET 8 SDK (10.0.100-rc.2 or later). Because it is still a preview, the SDK prints `NETSDK1057`; that is expected.
- The `maui` workload restored locally (`dotnet workload restore` from the repo root).
- macOS or Windows desktop with GUI access (Avalonia needs a windowing system; headless shells exit immediately after startup).

## Build
Run this once to compile the backend library and the sample:

```bash
dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

## Run

```bash
dotnet run --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

The command hosts the MAUI app inside `AvaloniaHostApplication`:
- `samples/MauiAvalonia.SampleApp/AvaloniaProgram.cs` starts `AvaloniaHostApplication` via `AppBuilder`.
- `MauiProgram` calls `.UseMauiAvaloniaHost()`, which registers the Avalonia-specific handlers and services.
- `AvaloniaWindowHost` now registers the `IWindow` instance inside each MAUI context, so the window handler can populate the `AvaloniaNavigationRoot` with real MAUI controls instead of the placeholder banner.

On successful launch you should see the standard MAUI template UI (dotnet bot image, hello world labels, and the counter button). Closing the window stops the `dotnet run` process.

### Optional: Avalonia diagnostics

Set `MAUI_AVALONIA_DEVTOOLS=1` before launching to automatically attach Avalonia’s diagnostics window (a.k.a. DevTools):

```bash
MAUI_AVALONIA_DEVTOOLS=1 dotnet run --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

In Debug builds the tools attach by default; set the variable to `0` if you want to suppress them.

## Troubleshooting
- If the command exits immediately without opening a window, confirm you are running on a desktop session; headless shells (CI or SSH without GUI forwarding) will start and shut down right away.
- When iterating on handlers, use `dotnet run … 2>&1 | rg AvaloniaWindowHandler` to confirm the window handler maps the MAUI content—you should see a log line similar to `Mapping content view: MauiAvalonia.SampleApp.AppShell`.
- If MAUI workloads were not restored, rerun `dotnet workload restore` and build again.
