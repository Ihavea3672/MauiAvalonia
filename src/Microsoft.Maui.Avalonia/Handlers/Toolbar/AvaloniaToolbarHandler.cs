using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public sealed class AvaloniaToolbarHandler : ToolbarHandler
{
	public AvaloniaToolbarHandler()
		: base(ToolbarHandler.Mapper, ToolbarHandler.CommandMapper)
	{
	}

	protected override object CreatePlatformElement() => new AvaloniaToolbar();

	protected override void ConnectHandler(object platformView)
	{
		base.ConnectHandler(platformView);

		if (platformView is AvaloniaToolbar toolbar)
		{
			toolbar.SetContext(MauiContext);
			toolbar.BackRequested += OnBackRequested;
		}
	}

	protected override void DisconnectHandler(object platformView)
	{
		if (platformView is AvaloniaToolbar toolbar)
		{
			toolbar.BackRequested -= OnBackRequested;
		}

		base.DisconnectHandler(platformView);
	}

	void OnBackRequested(object? sender, System.EventArgs e)
	{
		var window = MauiContext?.Services.GetService(typeof(IWindow)) as IWindow;
		window?.BackButtonClicked();
	}
}
