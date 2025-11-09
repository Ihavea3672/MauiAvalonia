using Avalonia;
using Avalonia.Controls;
using AvaloniaCornerRadius = global::Avalonia.CornerRadius;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public sealed class AvaloniaBorderHandler : AvaloniaViewHandler<IBorderView, AvaloniaBorderControl>
{
	public static readonly IPropertyMapper<IBorderView, AvaloniaBorderHandler> Mapper =
		new PropertyMapper<IBorderView, AvaloniaBorderHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
			[nameof(IContentView.Padding)] = MapPadding,
			[nameof(IBorderStroke.Stroke)] = MapStroke,
			[nameof(IBorderStroke.StrokeThickness)] = MapStroke,
			[nameof(IBorderStroke.Shape)] = MapStrokeShape
		};

	public AvaloniaBorderHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaBorderControl CreatePlatformView() =>
		new()
		{
			HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
			VerticalAlignment = AvaloniaVerticalAlignment.Stretch
		};

	static void MapContent(AvaloniaBorderHandler handler, IBorderView view)
	{
		if (handler.PlatformView is null)
			return;

		if (handler.MauiContext is null)
		{
			handler.PlatformView.Child = null;
			return;
		}

		var content = view.PresentedContent;
		handler.PlatformView.Child = content?.ToAvaloniaControl(handler.MauiContext);
	}

	static void MapPadding(AvaloniaBorderHandler handler, IBorderView view)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Padding = view.Padding.ToAvalonia();
	}

	static void MapStroke(AvaloniaBorderHandler handler, IBorderView view)
	{
		if (handler.PlatformView is null)
			return;

		if (view is not IBorderStroke stroke)
			return;

		handler.PlatformView.BorderBrush = stroke.Stroke?.ToAvaloniaBrush();
		handler.PlatformView.BorderThickness = new AvaloniaThickness(stroke.StrokeThickness);
		handler.UpdateCornerRadius(view);
	}

	static void MapStrokeShape(AvaloniaBorderHandler handler, IBorderView view) =>
		handler.UpdateCornerRadius(view);

	void UpdateCornerRadius(IBorderView view)
	{
		if (PlatformView is null)
			return;

		PlatformView.CornerRadius = ResolveCornerRadius(view);
	}

	static AvaloniaCornerRadius ResolveCornerRadius(IBorderView view)
	{
		if (view is not IBorderStroke stroke)
			return default;

		var shape = stroke.Shape;
		if (shape is RoundRectangle rounded)
		{
			return rounded.CornerRadius.ToAvalonia();
		}

		return default;
	}
}
