using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Avalonia.Graphics;

internal static class AvaloniaStrokeExtensions
{
	public static void UpdateStroke(this Shape shape, IStroke stroke)
	{
		shape.Stroke = stroke.Stroke?.ToAvaloniaBrush();
		shape.StrokeThickness = stroke.StrokeThickness;
		shape.StrokeDashOffset = stroke.StrokeDashOffset;
		shape.StrokeDashArray = stroke.StrokeDashPattern is { Length: > 0 } pattern
			? new AvaloniaList<double>(pattern.Select(p => (double)p))
			: null;
		shape.StrokeLineCap = stroke.StrokeLineCap.ToAvalonia();
		shape.StrokeJoin = stroke.StrokeLineJoin.ToAvalonia();
	}

	public static PenLineCap ToAvalonia(this LineCap cap) =>
		cap switch
		{
			LineCap.Butt => PenLineCap.Flat,
			LineCap.Round => PenLineCap.Round,
			LineCap.Square => PenLineCap.Square,
			_ => PenLineCap.Flat
		};

	public static PenLineJoin ToAvalonia(this LineJoin join) =>
		join switch
		{
			LineJoin.Miter => PenLineJoin.Miter,
			LineJoin.Round => PenLineJoin.Round,
			LineJoin.Bevel => PenLineJoin.Bevel,
			_ => PenLineJoin.Miter
		};
}
