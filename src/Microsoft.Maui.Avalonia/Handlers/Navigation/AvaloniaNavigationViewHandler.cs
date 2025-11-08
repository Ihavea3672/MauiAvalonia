using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Navigation;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public sealed class AvaloniaNavigationViewHandler : ViewHandler<IStackNavigationView, ContentControl>, INavigationViewHandler
{
static readonly IPropertyMapper<IStackNavigationView, INavigationViewHandler> _mapper = new PropertyMapper<IStackNavigationView, INavigationViewHandler>(ViewHandler.ViewMapper);
	static readonly CommandMapper<IStackNavigationView, INavigationViewHandler> _commandMapper = new(ViewCommandMapper)
	{
		[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
	};

	readonly AvaloniaStackNavigationManager _navigationManager = new();

	public AvaloniaNavigationViewHandler()
		: base(_mapper, _commandMapper)
	{
	}

	protected override ContentControl CreatePlatformView() => new ContentControl
	{
		HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Stretch,
		VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Stretch
	};

	protected override void ConnectHandler(ContentControl platformView)
	{
		base.ConnectHandler(platformView);
		_navigationManager.Connect(VirtualView, platformView, MauiContext);
	}

	protected override void DisconnectHandler(ContentControl platformView)
	{
		_navigationManager.Disconnect();
		base.DisconnectHandler(platformView);
	}

	static void RequestNavigation(INavigationViewHandler handler, IStackNavigation navigation, object? args)
	{
		if (handler is AvaloniaNavigationViewHandler avaloniaHandler && args is NavigationRequest request)
		{
			avaloniaHandler._navigationManager.NavigateTo(request);
		}
	}

	IStackNavigationView INavigationViewHandler.VirtualView => VirtualView;

	object INavigationViewHandler.PlatformView => PlatformView!;
}
