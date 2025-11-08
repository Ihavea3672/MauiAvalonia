using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaSwitchHandler : AvaloniaViewHandler<ISwitch, ToggleSwitch>, ISwitchHandler
{
	public static IPropertyMapper<ISwitch, AvaloniaSwitchHandler> Mapper = new PropertyMapper<ISwitch, AvaloniaSwitchHandler>(ViewHandler.ViewMapper)
	{
		[nameof(ISwitch.IsOn)] = MapIsOn,
		[nameof(ISwitch.TrackColor)] = MapTrackColor,
		[nameof(ISwitch.ThumbColor)] = MapThumbColor
	};

	public AvaloniaSwitchHandler()
		: base(Mapper)
	{
	}

	protected override ToggleSwitch CreatePlatformView() => new();

	protected override void ConnectHandler(ToggleSwitch platformView)
	{
		base.ConnectHandler(platformView);
		platformView.Checked += OnCheckedChanged;
		platformView.Unchecked += OnCheckedChanged;
	}

	protected override void DisconnectHandler(ToggleSwitch platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.Checked -= OnCheckedChanged;
		platformView.Unchecked -= OnCheckedChanged;
	}

	static void MapIsOn(AvaloniaSwitchHandler handler, ISwitch view)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.IsChecked = view.IsOn;
	}

	static void MapTrackColor(AvaloniaSwitchHandler handler, ISwitch view)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Background = view.TrackColor.ToAvaloniaBrush();
	}

	static void MapThumbColor(AvaloniaSwitchHandler handler, ISwitch view)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = view.ThumbColor.ToAvaloniaBrush();
	}

	void OnCheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (VirtualView is null || PlatformView is null)
			return;

		var value = PlatformView.IsChecked ?? false;
		if (VirtualView.IsOn != value)
			VirtualView.IsOn = value;
	}
}
