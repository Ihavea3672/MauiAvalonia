# Avalonia Desktop Build Fix Plan (net8.0 target)

The current `dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0` attempt fails because `src/Microsoft.Maui.Avalonia` has only been tested against `net8.0` inside the MAUI package environment. When compiling the repo’s own sources, hundreds of errors appear. They cluster into the following buckets. The items below describe the concrete work required to address each bucket, including the file(s) involved and the intended fix.

## 1. Namespace & API drift between MAUI vs Avalonia types

Errors: `CS0104` on `Window`, `Border`, `ContentPresenter`, `SolidColorBrush`, `Color`, `Grid`, `Rect`, `HorizontalAlignment`, `VerticalAlignment`, `Thickness`, `Dispatcher`, etc.

Plan:
1. Update every file that mixes Avalonia + MAUI namespaces so the types are fully-qualified or alias-imported. Files include:
   - `src/Microsoft.Maui.Avalonia/ApplicationModel/AvaloniaClipboard.cs`
   - `.../ApplicationModel/AvaloniaSemanticScreenReader.cs`
   - `.../Graphics/AvaloniaColorExtensions.cs`
   - `.../Handlers/*` (Button, Entry, DatePicker, etc.)
   - `.../Navigation/*.cs`
   - `.../Platform/AvaloniaControlExtensions.cs` and related extension files.
2. Standardize alias usage (`using AvaloniaWindow = global::Avalonia.Controls.Window;` etc.) to match what already exists in `AvaloniaToolbar.cs`. This eliminates ambiguous references.

## 2. Missing members in net8 MAUI interfaces

Errors: `IWindow.TitleBar`, `IWindow.TitleBarDragRectangles`, `IToolbar.ToHandler`, `IView.BindingContext`, `FocusManager.Instance`, etc. These members only exist in MAUI 9+ or Controls namespace extensions.

Plan:
1. Implement conditional compilation so the Avalonia backend targets the `net8.0` API surface:
   - Guard TitleBar-related logic with `#if NET9_0_OR_GREATER` (or similar) in `Handlers/Window/AvaloniaWindowHandler.cs` and `Navigation/AvaloniaNavigationRoot.cs`. Provide fallback no-ops for net8 builds.
   - Replace `FocusManager.Instance` usage with net8-friendly alternatives (`Microsoft.Maui.Controls.Internals.FocusManager` or manual state tracking) under `#if` blocks.
   - Replace `IToolbar.ToHandler` usage with direct `MauiContext.Services.GetRequiredService<IMauiHandlersServiceProvider>().CreateHandler()` calls, which exist in net8.
   - Avoid `IView.BindingContext` and use `IElement` or `BindableObject` casts where necessary.

## 3. Avalonia APIs renamed/absent in 11.x

Errors: `TextInputOptions.SetShowSuggestions`, `DragDrop.DoDragDropAsync`, `TextBox.SelectionChanged`, `ListBox.HorizontalContentAlignment`, `IBitmap`, etc.

Plan:
1. Audit each missing member against Avalonia 11.1.3:
   - Replace legacy API calls with the modern equivalents (`TextInputOptions.SetAutoCapitalization` etc. exist, but `SetShowSuggestions` moved to `TextInputOptions.SetIsSuggestionEnabled`). Verify exact signatures.
   - For drag/drop: use `DragDrop.DoDragDrop` (sync) or `DragDrop.DoDragDropAsync` inside `Avalonia.Input.DragDrop`. Ensure the correct namespace is imported; if API truly missing in 11.1, implement using `DragDrop.DoDragDrop(…)` and wrap `Task.Run` for async semantics.
   - `TextBox.SelectionChanged` events were renamed to `SelectionChanged` on `SelectableTextBox`. Use `TextBox.SelectionChanged` equivalent by subscribing to `TextBox.GetObservable(TextBox.SelectionStartProperty)` etc.
   - `ListBox.HorizontalContentAlignment` moved under `Control.HorizontalContentAlignment`. Update code to set `listBox.HorizontalContentAlignment` compiled in `Avalonia.Controls.Primitives`.
   - Access Avalonia `IBitmap` via `Avalonia.Media.Imaging.IBitmap` (it is internal). Switch to `Avalonia.Media.Imaging.Bitmap` or `IImage` surfaces that are public.
2. Create helper wrappers in `Platform/` to abstract these differences so handler code stays clean.

## 4. MAUI property mapper initialization failures

Errors: `Cannot create an instance of the abstract type or interface 'IPropertyMapper<...>'` due to `new(IPropertyMapper)` being invalid when MAUI 8 uses different constructors.

Plan:
1. Update each handler to follow the MAUI 8 pattern:
   - Instead of `public static IPropertyMapper<IButton, AvaloniaButtonHandler> Mapper = new(ViewMapper) { ... }`, use `public static IPropertyMapper<IButton, AvaloniaButtonHandler> Mapper = new PropertyMapper<IButton, AvaloniaButtonHandler>(ViewMapper)`.
   - Ensure `using Microsoft.Maui.Handlers;` is in scope so `PropertyMapper` resolves.
2. Repeat for every handler file flagged in the error list (Button, Entry, DatePicker, Editor, Label, CheckBox, Switch, Slider, ScrollView, CollectionView, GraphicsView, Flyout, Navigation, TimePicker, etc.).

## 5. Missing extension methods inside MAUI Controls namespace

Errors: `PointerGestureRecognizer.SendPointerEntered`, `DragGestureRecognizer.SendDragStarting`, `DropGestureRecognizer.SendDrop`, `IGestureRecognizer.GetGesturesFor`, etc.

Plan:
1. Add references to `Microsoft.Maui.Controls.Internals` or import the relevant helpers from upstream MAUI (e.g., `GestureManager` extension methods). Since we can’t modify `Microsoft.Maui.Controls`, we need to re-implement the required helper methods locally:
   - Create `internal static class GestureRecognizerExtensions` in `src/Microsoft.Maui.Avalonia/Input` that copies the logic from MAUI’s `GestureElementExtensions` (since we already reference `Microsoft.Maui.Controls`). This class should provide `GetGesturesFor<T>`, `SendPointerEntered`, etc., bridging through `IGestureController` APIs that exist in net8.
2. Replace direct calls with these helper methods.

## 6. Clipboard and Semantic Screen Reader compatibility

Errors: `IClipboard.TryGetTextAsync`, `Application` ambiguity, `AutomationProperties` differences, `AutomationElementIdentifiers.HelpTextProperty`, `FocusManager.Instance`.

Plan:
1. For `AvaloniaClipboard`:
   - Use `ClipboardExtensions.TryGetTextAsync` (available via `Avalonia.Input.Platform`). This returns `(bool success, string? text)`; adapt the logic accordingly.
   - Fully qualify `Avalonia.Application` to avoid conflicts.
   - Replace `_dispatcher.InvokeAsync(action).Task.Unwrap()` pattern with `await _dispatcher.InvokeAsync(action).GetTask()` since dispatcher returns `DispatcherOperation<T>`.
2. For `AvaloniaSemanticScreenReader`:
   - Fully qualify `Avalonia.Automation.AutomationProperties` vs `Microsoft.Maui.Controls.AutomationProperties`. Prefer Avalonia versions only.
   - Use `AvaloniaLocator.Current.GetService<IFocusManager>()` to fetch focus info instead of `FocusManager.Instance`.
   - `AutomationElementIdentifiers` is under `Avalonia.Automation.Peers`. Ensure the proper namespace is imported and that we only use members available in Avalonia 11 (HelpTextProperty exists there).

## 7. NavigationRoot, WindowHost, MenuBuilder alignment

Errors: numerous `Thickness`, `HorizontalAlignment`, `Grid`, `Color` conversions under `Navigation/` and `Hosting/`.

Plan:
1. Introduce alias statements at the top of each file (e.g., `using AvaloniaThickness = Avalonia.Thickness; using MauiColor = Microsoft.Maui.Graphics.Color;`). Replace property assignments accordingly.
2. Move repeated conversions into helper methods (e.g., `static AvaloniaThickness ToAvaloniaThickness(Thickness value)` inside `AvaloniaControlExtensions`).
3. For `AvaloniaWindowHost`, convert MAUI `HorizontalAlignment`/`VerticalAlignment` to Avalonia equivalents inside the layout helper, not inline.

## 8. Graphics & Grid helper gaps

Errors: `GridLength.ToAvalonia`, `ScrollBarVisibilityExtensions`, `AvaloniaGridPanel.RowSpacing` not found, `ImmediateDrawingContext.TryGetFeature<T>` not generic.

Plan:
1. Recreate the extension helpers we expected from upstream (e.g., `GridLengthExtensions` should declare the `GridUnitType` enum we need). `Avalonia`’s API uses `GridLengthUnitType.Pixel`, `Star`, etc. Map them correctly.
2. Replace `TryGetFeature<T>` with non-generic calls: `context.TryGetFeature(typeof(IDrawingContextFeature))` and cast.
3. Implement row/column spacing via attached properties or custom panel logic; Avalonia’s `Grid` doesn’t have those properties.

## 9. Handler service lookups

Errors: `AvaloniaButtonHandler` missing `GetRequiredService`, etc.

Plan:
1. Add the `IServiceProvider` helper to `AvaloniaViewHandler<TView, TPlatformView>` so derived classes can call `GetRequiredService<T>()`. This can be a protected method that forwards to `MauiContext?.Services`.
2. Update each handler to call the new helper rather than `this.GetRequiredService`.

## 10. Sample build pipeline

Once the above fixes land, rerun:

```bash
dotnet workload restore
dotnet build samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
dotnet run  --project samples/MauiAvalonia.SampleApp/MauiAvalonia.SampleApp.csproj -f net8.0
```

Document any remaining warnings (e.g., CS8766 nullability) and decide whether to suppress or adjust the handler signatures.

---

Tracking these tasks in this document ensures we do not lose sight of any error class. Tackle them in the order above so that foundational namespace/API fixes unblock the remaining issues.
