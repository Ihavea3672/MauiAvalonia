using Microsoft.Maui;
using AvaloniaFontStyle = Avalonia.Media.FontStyle;
using AvaloniaFontWeight = Avalonia.Media.FontWeight;
using MauiFontWeight = Microsoft.Maui.FontWeight;

namespace Microsoft.Maui.Avalonia.Graphics;

internal static class AvaloniaFontExtensions
{
	public static AvaloniaFontStyle ToAvaloniaFontStyle(this Font font) =>
		font.Slant switch
		{
			FontSlant.Italic => AvaloniaFontStyle.Italic,
			_ => AvaloniaFontStyle.Normal
		};

	public static AvaloniaFontWeight ToAvaloniaFontWeight(this Font font) =>
		font.Weight switch
		{
			MauiFontWeight.Thin => AvaloniaFontWeight.Thin,
			MauiFontWeight.Ultralight => AvaloniaFontWeight.ExtraLight,
			MauiFontWeight.Light => AvaloniaFontWeight.Light,
			MauiFontWeight.Regular => AvaloniaFontWeight.Normal,
			MauiFontWeight.Medium => AvaloniaFontWeight.Medium,
			MauiFontWeight.Semibold => AvaloniaFontWeight.SemiBold,
			MauiFontWeight.Bold => AvaloniaFontWeight.Bold,
			MauiFontWeight.Heavy => AvaloniaFontWeight.ExtraBold,
			MauiFontWeight.Black => AvaloniaFontWeight.Black,
			_ => AvaloniaFontWeight.Normal
		};
}
