# MAUI on Avalonia Sample

The `samples/MauiAvalonia.SampleApp` project demonstrates how the custom Avalonia backend boots a stock .NET MAUI application without touching `dotnet/maui`. The sample still multi-targets the standard mobile TFMs, but it now also exposes a desktop-friendly `net8.0` target that runs through `AvaloniaMauiApplication`.

## Prerequisites

- .NET 8.0 SDK (matching what `src/Microsoft.Maui.Avalonia` targets).
- MAUI workloads installed locally (`dotnet workload install maui`). This ensures the MAUI build targets and assets exist even though we host them inside Avalonia.
- (Optional) Any platform-specific SDKs if you also want to deploy to Android/iOS/macOS using the default MAUI backends.

## Restore & Build

```bash
# from the repo root
dotnet workload restore
dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

Building via `-f net8.0` ensures you compile the Avalonia desktop target (the other TFMs keep building exactly as before if you need them).

## Run the Avalonia host

```bash
dotnet run --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

This command:

1. Boots `AvaloniaProgram.Main`, which wires up `AppBuilder.Configure<AvaloniaHostApplication>()`.
2. `AvaloniaHostApplication` derives from `AvaloniaMauiApplication` and simply returns `MauiProgram.CreateMauiApp()` when asked to create the MAUI app.
3. `MauiProgram` calls `builder.UseMauiAvaloniaHost()` so the Avalonia-specific services/handlers light up.

You should see an Avalonia desktop window with the same content defined in `MainPage.xaml`.

## Troubleshooting

- If dotnet cannot find MAUI targets, rerun `dotnet workload install maui` (or `dotnet workload restore`) to pull them in for .NET 8.
- Use `dotnet run -f net8.0 -- --trace` to pass additional arguments through to Avalonia’s classic desktop lifetime (e.g., `--trace` for verbose logging).
- On Linux you may need to install the `libwebkit2gtk` dependencies that Avalonia relies on; see the [Avalonia documentation](https://docs.avaloniaui.net/) for distro-specific packages.
