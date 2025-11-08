using System.Linq;
using Avalonia;
using Avalonia.Media;
using Microsoft.Maui.Graphics;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using AvaloniaLinearGradientBrush = Avalonia.Media.LinearGradientBrush;
using AvaloniaRadialGradientBrush = Avalonia.Media.RadialGradientBrush;
using AvaloniaGradientStop = Avalonia.Media.GradientStop;
using AvaloniaGradientStops = Avalonia.Media.GradientStops;

namespace Microsoft.Maui.Avalonia.Graphics;

internal static class AvaloniaPaintExtensions
{
	public static IBrush? ToAvaloniaBrush(this Paint? paint)
	{
		switch (paint)
		{
			case null:
				return null;
			case SolidPaint solid:
				return solid.Color.ToAvaloniaBrush();
			case LinearGradientPaint linear:
				return CreateLinearGradientBrush(linear);
			case RadialGradientPaint radial:
				return CreateRadialGradientBrush(radial);
			default:
				return null;
		}
	}

	static AvaloniaLinearGradientBrush CreateLinearGradientBrush(LinearGradientPaint paint)
	{
		var stops = new AvaloniaGradientStops();

		if (paint.GradientStops?.Any() == true)
		{
			foreach (var stop in paint.GradientStops)
			{
				stops.Add(new AvaloniaGradientStop(stop.Color.ToAvaloniaColor(), stop.Offset));
			}
		}
		else
		{
			stops.Add(new AvaloniaGradientStop(MauiColors.Transparent.ToAvaloniaColor(), 0));
			stops.Add(new AvaloniaGradientStop(MauiColors.Transparent.ToAvaloniaColor(), 1));
		}

		var brush = new AvaloniaLinearGradientBrush
		{
			GradientStops = stops,
			StartPoint = new RelativePoint(paint.StartPoint.X, paint.StartPoint.Y, RelativeUnit.Relative),
			EndPoint = new RelativePoint(paint.EndPoint.X, paint.EndPoint.Y, RelativeUnit.Relative)
		};

		return brush;
	}

	static AvaloniaRadialGradientBrush CreateRadialGradientBrush(RadialGradientPaint paint)
	{
		var stops = new AvaloniaGradientStops();

		if (paint.GradientStops?.Any() == true)
		{
			foreach (var stop in paint.GradientStops)
				stops.Add(new AvaloniaGradientStop(stop.Color.ToAvaloniaColor(), stop.Offset));
		}
		else
		{
			stops.Add(new AvaloniaGradientStop(MauiColors.Transparent.ToAvaloniaColor(), 0));
			stops.Add(new AvaloniaGradientStop(MauiColors.Transparent.ToAvaloniaColor(), 1));
		}

		var brush = new AvaloniaRadialGradientBrush
		{
			GradientStops = stops,
			Center = new RelativePoint(paint.Center.X, paint.Center.Y, RelativeUnit.Relative),
			GradientOrigin = new RelativePoint(paint.Center.X, paint.Center.Y, RelativeUnit.Relative),
			RadiusX = new RelativeScalar(paint.Radius, RelativeUnit.Relative),
			RadiusY = new RelativeScalar(paint.Radius, RelativeUnit.Relative)
		};

		return brush;
	}
}
