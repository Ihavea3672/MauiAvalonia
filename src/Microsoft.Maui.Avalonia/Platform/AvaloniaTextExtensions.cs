using System;
using Avalonia.Media;
using Microsoft.Maui;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;

namespace Microsoft.Maui.Avalonia.Platform;

internal static class AvaloniaTextExtensions
{
	public static AvaloniaTextAlignment ToAvaloniaHorizontalAlignment(this Microsoft.Maui.TextAlignment alignment) =>
		alignment switch
		{
			Microsoft.Maui.TextAlignment.Center => AvaloniaTextAlignment.Center,
			Microsoft.Maui.TextAlignment.End => AvaloniaTextAlignment.Right,
			_ => AvaloniaTextAlignment.Left
		};

	public static AvaloniaVerticalAlignment ToAvaloniaVerticalAlignment(this Microsoft.Maui.TextAlignment alignment) =>
		alignment switch
		{
			Microsoft.Maui.TextAlignment.Center => AvaloniaVerticalAlignment.Center,
			Microsoft.Maui.TextAlignment.End => AvaloniaVerticalAlignment.Bottom,
			_ => AvaloniaVerticalAlignment.Top
		};

	public static double ToAvaloniaLetterSpacing(this double characterSpacing)
	{
		if (double.IsNaN(characterSpacing) || Math.Abs(characterSpacing) < double.Epsilon)
			return 0;

		return characterSpacing / 1000d;
	}

	public static TextDecorationCollection? ToAvalonia(this TextDecorations decorations)
	{
		if (decorations == TextDecorations.None)
			return null;

		var collection = new TextDecorationCollection();

		if (decorations.HasFlag(TextDecorations.Underline))
		{
			collection.Add(new TextDecoration
			{
				Location = TextDecorationLocation.Underline
			});
		}

		if (decorations.HasFlag(TextDecorations.Strikethrough))
		{
			collection.Add(new TextDecoration
			{
				Location = TextDecorationLocation.Strikethrough
			});
		}

		return collection;
	}
}
