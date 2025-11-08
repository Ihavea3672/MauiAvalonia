using Microsoft.Maui;
using AvaloniaGridLength = global::Avalonia.Controls.GridLength;
using AvaloniaGridUnitType = global::Avalonia.Controls.GridUnitType;
using MauiGridUnitType = Microsoft.Maui.GridUnitType;

namespace Microsoft.Maui.Avalonia.Platform;

internal static class AvaloniaGridLengthExtensions
{
	public static AvaloniaGridLength ToAvalonia(this Microsoft.Maui.GridLength length) =>
		length.GridUnitType switch
		{
			MauiGridUnitType.Star => new AvaloniaGridLength(length.Value, AvaloniaGridUnitType.Star),
			MauiGridUnitType.Auto => AvaloniaGridLength.Auto,
			_ => new AvaloniaGridLength(length.Value, AvaloniaGridUnitType.Pixel)
		};
}
