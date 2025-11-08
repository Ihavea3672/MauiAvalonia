using Avalonia;
using AvaloniaApplication = Avalonia.Application;

namespace Microsoft.Maui.Hosting;

/// <summary>
/// Provides a hook between the Avalonia lifetime model and the MAUI window system.
/// </summary>
public interface IAvaloniaWindowHost
{
	/// <summary>
	/// Attaches to the current Avalonia <paramref name="lifetimeOwner"/> and ensures at least one window
	/// is created for the supplied MAUI <paramref name="application"/>.
	/// </summary>
	void AttachLifetime(AvaloniaApplication lifetimeOwner, Microsoft.Maui.IApplication application, IMauiContext applicationContext);
}
