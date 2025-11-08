using Avalonia.Media;
using Microsoft.Maui.Graphics;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Avalonia.Graphics;

internal static class AvaloniaColorExtensions
{
	public static AvaloniaColor ToAvaloniaColor(this MauiColor color) =>
		AvaloniaColor.FromArgb(
			(byte)(color.Alpha * byte.MaxValue),
			(byte)(color.Red * byte.MaxValue),
			(byte)(color.Green * byte.MaxValue),
			(byte)(color.Blue * byte.MaxValue));

	public static IBrush ToAvaloniaBrush(this MauiColor color) =>
		new AvaloniaSolidColorBrush(color.ToAvaloniaColor());
}
