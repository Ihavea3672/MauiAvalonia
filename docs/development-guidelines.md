# MauiAvalonia Development Guidelines (Phase 1)

## Coding Standards
- Target `net8.0` for all backend libraries until the MAUI team finalizes a .NET 10 baseline; enable nullable reference types and `LangVersion=latest`.
- Default to MAUI coding style (PascalCase members, `var` for implicit locals when the type is obvious, `this.` only when required by the compiler). Avalonia-specific helpers should live under the `Microsoft.Maui.Avalonia` namespace to mirror upstream structure.
- Keep platform shims small and well-documented. If a method simply throws `NotImplementedException`, include a short comment describing which roadmap phase will fill it in.
- Add XML doc comments or summary remarks to any new public API so downstream consumers have IntelliSense guidance even before the feature is complete.

## Dependencies & Versions
- Use the checked-in submodules as the authoritative source of truth:
  - `extern/maui` supplies the current MAUI source for reference; NuGet packages referenced in our projects should match the commit/branch currently synced in the submodule.
  - `extern/Avalonia` provides source alignment. Until we consume the projects directly, pin NuGet packages (e.g., `Avalonia`, `Avalonia.Desktop`) to the exact tag mirrored by the submodule to avoid behavioral drift.
- Avoid scattering package versions across projects. Define shared versions in a future `Directory.Packages.props`; for Phase 1 the hard-coded versions in `Microsoft.Maui.Avalonia.csproj` set the baseline (MAUI 8.0.91, Avalonia 11.1.3).
- Do not take dependencies on platform-specific binaries unless they are required for Avalonia desktop runtimes. Keep anything mobile-specific inside the sample app.

## Versioning & Branching
- Follow MAUI’s release cadence: bump our backend version only after validating against the corresponding MAUI tag (e.g., `8.0.91`, `9.0.x`, etc.). Experimental prereleases should use semantic versions such as `0.1.0-alpha1`.
- Maintain long-lived branches per MAUI release (e.g., `maui-8.0`, `maui-9.0`). Work targeting a future MAUI build should land behind feature branches and merge only after syncing `extern/maui`.
- Every commit that updates either submodule must also update the documentation (plan + changelog) noting which upstream SHA we track.

## Testing Expectations
- Library projects must compile via `dotnet build` on macOS and Windows. Sample apps should restore successfully even if workloads for every platform are not installed.
- When adding host or handler functionality, include unit tests under `tests/` and connect them to the solution so CI can execute them headlessly. Avalonia provides a headless renderer; plan to adopt it in Phase 2+.
- Document any manual test steps in the sample app README until automated coverage is available.

These guidelines satisfy the “documentation” bullet in Phase 1 and will evolve as the backend implementation solidifies.
