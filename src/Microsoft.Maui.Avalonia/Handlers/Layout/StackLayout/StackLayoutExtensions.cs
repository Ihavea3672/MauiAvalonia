using System;
using System.Reflection;
using Avalonia.Layout;
using Microsoft.Maui;

namespace Microsoft.Maui.Avalonia.Handlers;

internal static class StackLayoutExtensions
{
	public static Orientation GetStackOrientation(this IStackLayout layout)
	{
		if (layout is null)
			return Orientation.Vertical;

		// StackLayout exposes an Orientation property in Microsoft.Maui.Controls.
		// VerticalStackLayout/HorizontalStackLayout types do not expose the property,
		// so we infer the orientation from the concrete type.
		var orientationProperty = layout.GetType().GetRuntimeProperty("Orientation");
		if (orientationProperty is not null)
		{
			var value = orientationProperty.GetValue(layout);
			if (value is not null && string.Equals(value.ToString(), "Horizontal", StringComparison.OrdinalIgnoreCase))
				return Orientation.Horizontal;

			if (value is not null && string.Equals(value.ToString(), "Vertical", StringComparison.OrdinalIgnoreCase))
				return Orientation.Vertical;
		}

		var typeName = layout.GetType().Name;
		if (typeName.Contains("Horizontal", StringComparison.OrdinalIgnoreCase))
			return Orientation.Horizontal;

		return Orientation.Vertical;
	}
}
