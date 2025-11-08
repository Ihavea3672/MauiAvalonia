using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaContentViewHandler : AvaloniaViewHandler<IContentView, ContentControl>
{
	public static PropertyMapper<IContentView, AvaloniaContentViewHandler> Mapper =
		new PropertyMapper<IContentView, AvaloniaContentViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IContentView.Content)] = MapContent,
			[nameof(IContentView.Padding)] = MapPadding
		};

	public AvaloniaContentViewHandler()
		: this(Mapper)
	{
	}

	protected AvaloniaContentViewHandler(IPropertyMapper mapper)
		: base(mapper)
	{
	}

	protected override ContentControl CreatePlatformView() =>
		new()
		{
			HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
			VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch
		};

	static void MapContent(AvaloniaContentViewHandler handler, IContentView view)
	{
		if (handler.MauiContext is null)
		{
			handler.PlatformView.Content = null;
			return;
		}

		var presented = view.PresentedContent;
		handler.PlatformView.Content = presented?.ToAvaloniaControl(handler.MauiContext);
	}

	static void MapPadding(AvaloniaContentViewHandler handler, IContentView view) =>
		handler.PlatformView.Padding = view.Padding.ToAvalonia();
}
