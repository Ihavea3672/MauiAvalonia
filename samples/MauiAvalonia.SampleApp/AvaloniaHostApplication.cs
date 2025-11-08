#if NET8_0
using Microsoft.Maui;

namespace MauiAvalonia.SampleApp;

/// <summary>
/// Avalonia entry point that boots the MAUI app through <see cref="AvaloniaMauiApplication"/>.
/// </summary>
public sealed class AvaloniaHostApplication : AvaloniaMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
#endif
