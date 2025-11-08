using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;

namespace Microsoft.Maui.Avalonia.Internal;

internal static class AvaloniaDiagnosticsHelper
{
#if DEBUG
	const bool DefaultEnabled = true;
#else
	const bool DefaultEnabled = false;
#endif

	public static void AttachIfEnabled(TopLevel? topLevel)
	{
		if (topLevel is null || !IsEnabled())
			return;

		try
		{
			topLevel.AttachDevTools();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[MauiAvalonia] Unable to attach Avalonia dev tools: {ex}");
		}
	}

	static bool IsEnabled()
	{
		var setting = Environment.GetEnvironmentVariable("MAUI_AVALONIA_DEVTOOLS");
		if (string.IsNullOrWhiteSpace(setting))
			return DefaultEnabled;

		return setting.Equals("1", StringComparison.OrdinalIgnoreCase) ||
			setting.Equals("true", StringComparison.OrdinalIgnoreCase) ||
			setting.Equals("yes", StringComparison.OrdinalIgnoreCase);
	}
}
