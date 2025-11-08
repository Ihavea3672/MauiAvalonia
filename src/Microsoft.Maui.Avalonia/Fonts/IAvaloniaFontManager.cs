using Avalonia.Media;

namespace Microsoft.Maui.Avalonia.Fonts;

internal interface IAvaloniaFontManager
{
	double DefaultFontSize { get; }

	FontFamily DefaultFontFamily { get; }

	FontFamily GetFontFamily(Font font);

	double GetFontSize(Font font, double defaultFontSize = 0);
}
