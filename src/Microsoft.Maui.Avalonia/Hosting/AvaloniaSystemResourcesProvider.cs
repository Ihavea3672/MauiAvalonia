using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Avalonia.Hosting;

/// <summary>
/// Supplies a minimal resource dictionary so MAUI's <see cref="Application"/> constructor
/// can complete without platform-specific resource providers.
/// </summary>
internal sealed class AvaloniaSystemResourcesProvider : ISystemResourcesProvider
{
	public IResourceDictionary GetSystemResources() => new ResourceDictionary();
}
