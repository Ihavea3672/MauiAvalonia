using Avalonia;
using Microsoft.Maui.Handlers;
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

	/// <summary>
	/// Opens a new Avalonia window tied to the specified MAUI <paramref name="application"/>.
	/// </summary>
	/// <param name="application">The MAUI application requesting the window.</param>
	/// <param name="request">Optional window request metadata (persisted state, routing id, etc.).</param>
	void OpenWindow(Microsoft.Maui.IApplication application, OpenWindowRequest? request);
}
