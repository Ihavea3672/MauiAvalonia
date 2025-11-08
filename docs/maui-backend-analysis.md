# MAUI Backend Architecture Analysis

## Why This Matters
Understanding how existing .NET MAUI backends (Android, iOS/macOS, Windows) are composed is essential before introducing a new Avalonia-based backend in this repository. The upstream repo organises backends through a consistent bootstrapping pattern, tight integration with `IMauiContext`, and handler partials compiled per target framework. Any new backend must either slot into those extension points or replicate them outside the main repo.

## Shared Building Blocks
- **Target framework switching** – `Directory.Build.props:191-207` builds MAUI for specific TFMs (`net{version}-android`, `-ios`, `-maccatalyst`, `-windows`, etc.). Adding a built-in backend typically means adding another `$(MauiPlatforms)` entry and corresponding SDK props.
- **Platform hosts** – Each backend exposes a host that derives from the respective native `Application` type and implements `IPlatformApplication` (Android: `MauiApplication`, iOS/macOS: `MauiUIApplicationDelegate`, Windows: `MauiWinUIApplication`). These hosts create the `MauiApp`, establish an `IMauiContext`, set the `Application` handler, and trigger platform-specific lifecycle events.
- **`IMauiContext` scopes** – `MauiContextExtensions.MakeApplicationScope/MakeWindowScope` (`extern/maui/src/Core/src/MauiContextExtensions.cs:38-74`) clone the service provider per application/window and inject platform objects (`Application`, `Activity`, `Window`, `NavigationRootManager`) so handlers can resolve them later.
- **Handler system** – `HandlerMauiAppBuilderExtensions.ConfigureMauiHandlers` (`src/Core/src/Hosting/HandlerMauiAppBuilderExtensions.cs`) registers handler mappings via DI. Handlers map virtual MAUI views to native views using property/command mappers and are marked `partial` so each platform-specific file can supply rendering details.
- **Navigation root plumbing** – Every backend installs a platform-specific root host (`NavigationRootManager`, `WindowRootView`, etc.) that owns the container hierarchy, toolbar/menu injection, and title bar integration. This plumbing sits in `src/Core/src/Platform/<platform>` and is added to the window scope via `MauiContextExtensions`.

## Android Backend Composition
1. **Host & lifecycle** – `MauiApplication` (`extern/maui/src/Core/src/Platform/Android/MauiApplication.cs:16-124`) inherits `Android.App.Application`, captures `IPlatformApplication.Current`, builds the `MauiApp`, calls `MakeApplicationScope(this)`, resolves `IApplication`, wires lifecycle events, and registers `ActivityLifecycleCallbacks`.
2. **Activity bootstrap** – `MauiAppCompatActivity` plus `ApplicationExtensions.CreatePlatformWindow` (`src/Core/src/Platform/Android/ApplicationExtensions.cs:33-70`) build a window scope from the root application context, invoke Android lifecycle events, create the MAUI `Window`, and attach it to the current `Activity`.
3. **Root view management** – `NavigationRootManager` (`src/Core/src/Platform/Android/Navigation/NavigationRootManager.cs`) inflates `navigationlayout`, swaps fragments for MAUI pages, wires drawer/flyout patterns, and coordinates toolbars and inset handling.
4. **Handlers** – Each handler defines a `PlatformView` alias for Android (e.g., `MaterialButton` in `src/Core/src/Handlers/Button/ButtonHandler.cs:1-66`) and an Android-specific partial class (e.g., `ButtonHandler.Android.cs`) that implements mapper callbacks using extension methods (`Platform/Android/*Extensions.cs`).
5. **Platform services** – Android-only services such as `LayoutInflater`, `FragmentManager`, `GlobalWindowInsetListener`, and `NavigationRootManager` are injected into the window scope to be consumed by handlers or controls.

## iOS & MacCatalyst Backend Composition
1. **Host & scenes** – `MauiUIApplicationDelegate` (`src/Core/src/Platform/iOS/MauiUIApplicationDelegate.cs:12-191`) handles both iOS and MacCatalyst. It builds the app in `WillFinishLaunching`, stores `_applicationContext`, resolves `IApplication`, sets its handler, and handles single-window (no scene) or multi-window flows via `MauiUISceneDelegate`.
2. **Window creation** – `ApplicationExtensions.CreatePlatformWindow` (`src/Core/src/Platform/iOS/ApplicationExtensions.cs:25-118`) looks at `UIApplication`, `UIScene`, or `NSUserActivity` state, creates a `UIWindow`, makes a window scope, and sets the MAUI window handler.
3. **Runtime services** – Lifecycle callbacks (`iOSLifecycle.*`) plus `WindowStateManager`, safe area helpers, and various `*Extensions` classes under `Platform/iOS` bridge MAUI abstractions to UIKit/AppKit concepts (UIView hosting, navigation controllers, scroll handling, context menus, etc.).
4. **Handlers** – Platform-specific partials provide UIKit implementations, using the same handler class compiled with `#if __IOS__ || MACCATALYST`. Many properties share code between iOS and MacCatalyst, which is why there is no separate Platform folder for macOS—they share the `Platform/iOS` subtree with conditional code paths.

## Windows Backend Composition
1. **Host** – `MauiWinUIApplication` (`src/Core/src/Platform/Windows/MauiWinUIApplication.cs:8-87`) inherits `Microsoft.UI.Xaml.Application`. `OnLaunched` guards against multiple launches, builds the app, creates an application scope, resolves `IApplication`, sets the handler, then calls `CreatePlatformWindow`.
2. **Window factory** – `Platform/Windows/ApplicationExtensions.cs:4-33` constructs `MauiWinUIWindow`, creates a window scope (`MakeWindowScope` injects a `NavigationRootManager` tied to the WinUI `Window`), creates the MAUI `Window`, and activates it. Lifecycle hooks dispatch `WindowsLifecycle` events.
3. **Root view & chrome** – `NavigationRootManager` (`Platform/Windows/NavigationRootManager.cs`) owns `WindowRootView`, `MauiToolbar`, `MenuBar`, and title bar customization. It reacts to window activation, manages drag rectangles, and hosts the MAUI content inside a `RootNavigationView`.
4. **Handlers & extensions** – Windows-specific partials (e.g., `ButtonHandler.Windows.cs`) invoke `Platform/Windows/*Extensions.cs` helpers that map MAUI properties to WinUI APIs (brushes, typography, title bars, etc.).

## Handler Pattern & Platform Views
- Handlers are partial classes compiled once per TFM. They select a native control via `using PlatformView = ...` inside the shared `.cs` file (`ButtonHandler.cs:1-11`). Platform-specific files implement mapper methods, lifecycle hooks, and native event wiring.
- Each handler references platform helper extensions (`UpdateBackground`, `UpdateFont`, etc.) housed under `Platform/<platform>`. This keeps property logic shareable across handlers.
- The handler registration pipeline expects unique handler types per platform. Reusing the existing handler classes outside the MAUI repo would require compiling `Microsoft.Maui.Core` for the new TFM so the partial classes can be extended, or supplying entirely new handler types that register themselves via `ConfigureMauiHandlers`.

## Implications for an Avalonia Backend
1. **Bootstrapping** – We need an Avalonia-specific host analogous to `MauiWinUIApplication`/`MauiApplication`. It must derive from an Avalonia `Application`/`ILifetime`, create the `MauiApp`, own `IMauiContext` scopes, and surface `IPlatformApplication`.
2. **Window scope services** – `MakeWindowScope` currently injects navigation managers only for Android and Windows (`MauiContextExtensions.cs:63-68`). Hosting MAUI inside Avalonia without touching upstream means creating equivalent services (root visual, toolbar/menu host, dispatcher, drag rectangles) inside this repo and registering them with the scoped DI container.
3. **Handlers** – Since handler partial classes can’t be extended from another assembly, we must either:  
   - Build a fork of `Microsoft.Maui.Core` with `#if AVALONIA` sections, or  
   - Provide new handler types (e.g., `AvaloniaButtonHandler : ViewHandler<IButton, Avalonia.Controls.Button>`) and override handler registration for Avalonia builds. The latter keeps upstream untouched but requires re‑implementing every platform mapper using Avalonia controls and styling APIs.
4. **Platform services parity** – Existing backends rely on services such as `IDispatcher`, `IFontManager`, `IAnimationManager`, safe area calculators, `IWindow`, `IMenuBar` integration, and physics/input translation. Avalonia equivalents must be implemented so MAUI abstractions behave as expected.
5. **Lifecycle & activation** – Android/iOS/Windows map their native lifecycle events into MAUI `LifecycleEvents`. An Avalonia backend must raise matching events (e.g., window Created/Activated/Closing, theme changes, clipboard, pointer gestures) so existing MAUI apps continue to work.
6. **Packaging** – If we keep this backend out of the official repo, we ship a companion NuGet (e.g., `Microsoft.Maui.Avalonia`) that references the upstream MAUI packages (netstandard TFMs) plus Avalonia, exposes a `UseAvaloniaAppHost()` helper, and contains all handler/host implementations described above.

These observations drive the detailed implementation plan in `docs/avalonia-backend-plan.md`.
