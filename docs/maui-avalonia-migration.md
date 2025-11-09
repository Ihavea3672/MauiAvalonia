# Migrating an existing MAUI app to Avalonia

This repo ships a companion NuGet package (`Microsoft.Maui.Avalonia`) that wires MAUI into an Avalonia `Application`. You can migrate any existing MAUI project to run on Avalonia (desktop preview) by following these steps.

## 1. Add the Avalonia host package

Update your MAUI project to reference the latest preview NuGet:

```bash
dotnet add package Microsoft.Maui.Avalonia --version 0.1.0-nightly.123
```

Replace the version with the nightly build you want from `.github/workflows/nightly-release.yml` or use the stable `0.1.0` package once it is published. For version details and the publishing cadence see [docs/versioning.md](versioning.md).

## 2. Wire the Avalonia host

In your `MauiProgram.cs`, call `.UseMauiAvaloniaHost()` when building the MAUI app:

```csharp
builder.UseMauiApp<App>()
       .UseMauiAvaloniaHost();
```

Next, add the Avalonia entry points shown in the template:

* `AvaloniaHostApplication : AvaloniaMauiApplication` that overrides `CreateMauiApp` and returns `MauiProgram.CreateMauiApp()`.
* `AvaloniaProgram` that configures `AppBuilder`, calls `.UsePlatformDetect()`, and starts the classic desktop lifetime.

Both classes should be compiled for `NET8_0` only (wrap them in `#if NET8_0` guards).

## 3. Keep UI code portable

The remaining MAUI pages and shells stay the same—templates can help you bootstrap a starter shell and `MainPage`. The new `dotnet new maui-avalonia` template in the `templates/` folder mirrors this minimal setup. Use it as a reference or run:

```bash
dotnet new maui-avalonia -n MyAvaloniaApp
```

This scaffolds a ready-to-run project that already wires the Avalonia host, so you can compare it with your existing project to spot missing resources or event handlers.

## 4. Run and validate

Build and run using the standard `dotnet build`/`dotnet run` commands. Avalonia will launch a desktop window that hosts your MAUI UI. Use the sample matrix (`samples/MauiAvalonia.SampleApp`) as a regression suite to ensure handlers behave on Windows/macOS/Linux.

Once you have your Avalonia configuration working, copy the entry-point files and any resource dictionaries from the template or sample app into your project. Adjust layout code as needed; the existing MAUI handler layer will drive Avalonia controls under the hood.
