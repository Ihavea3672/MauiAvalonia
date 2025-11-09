# Versioning & Servicing Policy

This project ships an experimental preview of `Microsoft.Maui.Avalonia`. We keep the package alive through fast, automated tooling so contributors can grab nightly builds, while meaningful version bumps and servicing releases remain under human control.

## Versioning strategy

- **Base version:** `0.1.0` represents the current preview stream. When we are ready for a stable release we bump this base version and adjust the workflows/docs together.
- **Nightly builds:** The nightly release workflow (`.github/workflows/nightly-release.yml`) produces `0.1.0-nightly.{run_number}` packages. Every run reinstates the `BASE_VERSION` from the workflow environment, increments the suffix with GitHub’s `run_number`, and packs the library without rebuilding the solution twice.
- **Package metadata:** The nightly job writes packages into the `artifacts` folder, uploads them as workflow artifacts, and pushes them to NuGet only when the repository has a `NUGET_API_KEY` secret. The secret should point to a NuGet API key with push rights for `Microsoft.Maui.Avalonia`.

## Nightly release workflow

- Runs on a schedule (`0 3 * * *` UTC) and can be triggered manually via `workflow_dispatch`.
- Steps:
  1. Install the .NET SDK + MAUI workload.
  2. Restore, build, and test the solution on `ubuntu-latest`.
  3. Pack `src/Microsoft.Maui.Avalonia` using the generated nightly version string.
  4. Upload the `.nupkg` as a GitHub artifact for manual validation.
  5. Push to NuGet with `dotnet nuget push` when `NUGET_API_KEY` is defined (the step is skipped otherwise).

Nightly packages can be consumed via `dotnet add package Microsoft.Maui.Avalonia --version 0.1.0-nightly.{run_number}`. A GitHub artifact download is also available from the workflow run page if pushing to NuGet is not desired.

## Servicing policy

- **Hotfix path:** Critical or regression fixes should ship on `main` and update the nightly stream immediately; include a changelog entry and verify that the nightly workflow succeeds before backporting.
- **Patch releases:** When a stable release is prepared, bump `BASE_VERSION` in the release workflow and coordinate a manual `dotnet pack` + `dotnet nuget push` with the desired `PackageVersion`. Release candidates should follow the `MAJOR.MINOR.X` pattern that matches the bumped `BASE_VERSION`.
- **Support window:** We do not promise long-term support for nightly builds—they are best-effort snapshots meant for validation. Consider a stable release (with a proper semantic version) for production usage, and document any API changes in `docs/avalonia-backend-plan.md`.

## Manual NuGet pushes

If you need to push a preview or stable package outside the nightly cadence:

1. Update `BASE_VERSION`/`PackageVersion` to the desired semantic version.
2. Run `dotnet workload install maui`, `dotnet restore`, `dotnet build`, and `dotnet pack` with `-p:PackageVersion=<version>` and `-o artifacts`.
3. Use `dotnet nuget push artifacts/*.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json --skip-duplicate`.
4. Tag the commit and annotate the release in GitHub.

Document the release in the plan once the package is published so the next maintainer knows which versions are live.
