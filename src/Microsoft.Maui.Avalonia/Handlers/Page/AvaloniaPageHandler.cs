using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaPageHandler : AvaloniaContentViewHandler
{
	public static new IPropertyMapper<IContentView, AvaloniaPageHandler> Mapper =
		new PropertyMapper<IContentView, AvaloniaPageHandler>(AvaloniaContentViewHandler.Mapper)
		{
			[nameof(ITitledElement.Title)] = MapTitle
		};

	public AvaloniaPageHandler()
		: base(Mapper)
	{
	}

	static void MapTitle(AvaloniaPageHandler handler, IContentView page)
	{
		if (handler.MauiContext is null || page is not ITitledElement titled)
			return;

		var window = handler.MauiContext.Services.GetService<IWindow>();
		if (window is null || !ReferenceEquals(window.Content, page))
			return;

		if (window.Handler is AvaloniaWindowHandler avaloniaWindowHandler &&
			avaloniaWindowHandler.PlatformView is AvaloniaWindow platformWindow)
		{
			platformWindow.Title = titled.Title ?? string.Empty;
		}
	}
}
