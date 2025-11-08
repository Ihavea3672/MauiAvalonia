using System;
using System.Collections.Concurrent;
using System.IO;

namespace Microsoft.Maui.Avalonia.Fonts;

/// <summary>
/// Persists embedded fonts to disk so Avalonia can reference them via absolute URIs.
/// </summary>
public sealed class AvaloniaEmbeddedFontLoader : IEmbeddedFontLoader
{
	readonly ConcurrentDictionary<string, string> _fontFiles = new(StringComparer.OrdinalIgnoreCase);
	readonly string _cacheDirectory;

	public AvaloniaEmbeddedFontLoader()
	{
		_cacheDirectory = Path.Combine(Path.GetTempPath(), "MauiAvaloniaFonts");
		Directory.CreateDirectory(_cacheDirectory);
	}

	public string? LoadFont(EmbeddedFont font)
	{
		if (font.ResourceStream is null || string.IsNullOrEmpty(font.FontName))
			return null;

		return _fontFiles.GetOrAdd(font.FontName, _ =>
		{
			var targetPath = Path.Combine(_cacheDirectory, $"{Guid.NewGuid()}-{font.FontName}");

			using (var targetStream = File.Create(targetPath))
			{
				font.ResourceStream.Position = 0;
				font.ResourceStream.CopyTo(targetStream);
			}

			var familyName = Path.GetFileNameWithoutExtension(font.FontName);
			var uri = new Uri(targetPath);
			return $"{uri.AbsoluteUri}#{familyName}";
		});
	}
}
