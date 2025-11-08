using Avalonia.Media;
using Microsoft.Maui;
using AvaloniaStretch = Avalonia.Media.Stretch;

namespace Microsoft.Maui.Avalonia.Platform;

internal static class AvaloniaAspectExtensions
{
	public static AvaloniaStretch ToAvalonia(this Aspect aspect) =>
		aspect switch
		{
			Aspect.Fill => AvaloniaStretch.Fill,
			Aspect.AspectFill => AvaloniaStretch.UniformToFill,
			Aspect.Center => AvaloniaStretch.None,
			_ => AvaloniaStretch.Uniform
		};
}
