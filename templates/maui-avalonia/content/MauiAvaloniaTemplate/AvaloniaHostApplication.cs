using Microsoft.Maui;

#if NET8_0
namespace MauiAvaloniaTemplate;

internal sealed class AvaloniaHostApplication : AvaloniaMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
#endif
