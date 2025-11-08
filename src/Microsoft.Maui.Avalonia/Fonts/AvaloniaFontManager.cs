using System;
using System.Collections.Concurrent;
using Avalonia.Media;
using Microsoft.Maui.Avalonia.Graphics;

namespace Microsoft.Maui.Avalonia.Fonts;

/// <summary>
/// Basic font manager that bridges MAUI font abstractions to Avalonia font families.
/// </summary>
public sealed class AvaloniaFontManager : IFontManager, IAvaloniaFontManager
{
	const double DefaultSize = 14;

	readonly ConcurrentDictionary<string, FontFamily> _fontCache = new(StringComparer.OrdinalIgnoreCase);
	readonly IFontRegistrar _fontRegistrar;

	public AvaloniaFontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
	{
		_fontRegistrar = fontRegistrar;
	}

	public double DefaultFontSize => DefaultSize;

	public FontFamily DefaultFontFamily => global::Avalonia.Media.FontManager.Current.DefaultFontFamily;

	public FontFamily GetFontFamily(Font font)
	{
		if (font.IsDefault || string.IsNullOrWhiteSpace(font.Family))
			return DefaultFontFamily;

		return _fontCache.GetOrAdd(font.Family, CreateFontFamily);
	}

	public double GetFontSize(Font font, double defaultFontSize = 0) =>
		font.Size <= 0 || double.IsNaN(font.Size)
			? (defaultFontSize > 0 ? defaultFontSize : DefaultFontSize)
			: font.Size;

	FontFamily CreateFontFamily(string family)
	{
		var registered = _fontRegistrar.GetFont(family);

		if (!string.IsNullOrWhiteSpace(registered))
			return new FontFamily(registered);

		return new FontFamily(family);
	}
}
