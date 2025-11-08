using Avalonia;
using Microsoft.Maui;
using GraphicsRect = Microsoft.Maui.Graphics.Rect;
using GraphicsSize = Microsoft.Maui.Graphics.Size;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Avalonia.Internal;

internal static class AvaloniaConversionExtensions
{
	public static global::Avalonia.Thickness ToAvalonia(this Microsoft.Maui.Thickness source) =>
		new(source.Left, source.Top, source.Right, source.Bottom);

	public static global::Avalonia.Rect ToAvalonia(this GraphicsRect rect) =>
		new(rect.X, rect.Y, rect.Width, rect.Height);

	public static global::Avalonia.Size ToAvalonia(this GraphicsSize size) =>
		new(size.Width, size.Height);

	public static global::Avalonia.CornerRadius ToAvalonia(this Microsoft.Maui.CornerRadius radius) =>
		new(radius.TopLeft, radius.TopRight, radius.BottomRight, radius.BottomLeft);

	public static global::Avalonia.CornerRadius ToAvalonia(this int radius) =>
		new(radius);
}
