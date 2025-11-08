using System;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Avalonia.Handlers;

internal static class AvaloniaViewExtensions
{
	public static Control? ToAvaloniaControl(this IView view, IMauiContext context)
	{
		_ = view ?? throw new ArgumentNullException(nameof(view));
		_ = context ?? throw new ArgumentNullException(nameof(context));

		var platformView = view.ToPlatform(context);
		if (platformView is Control control)
			return control;

		throw new InvalidOperationException($"Handler for view '{view.GetType()}' did not provide an Avalonia control.");
	}
}
