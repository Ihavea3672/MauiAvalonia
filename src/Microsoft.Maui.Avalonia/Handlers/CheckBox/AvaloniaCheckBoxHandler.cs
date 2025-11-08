using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Handlers;
using AvaloniaCheckBox = Avalonia.Controls.CheckBox;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaCheckBoxHandler : AvaloniaViewHandler<ICheckBox, AvaloniaCheckBox>, ICheckBoxHandler
{
public static PropertyMapper<ICheckBox, AvaloniaCheckBoxHandler> Mapper = new PropertyMapper<ICheckBox, AvaloniaCheckBoxHandler>(ViewHandler.ViewMapper)
	{
		[nameof(ICheckBox.IsChecked)] = MapIsChecked,
		[nameof(ICheckBox.Foreground)] = MapForeground
	};

	public AvaloniaCheckBoxHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaCheckBox CreatePlatformView() => new();

	protected override void ConnectHandler(AvaloniaCheckBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.Checked += OnCheckedChanged;
		platformView.Unchecked += OnCheckedChanged;
		platformView.Indeterminate += OnCheckedChanged;
	}

	protected override void DisconnectHandler(AvaloniaCheckBox platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.Checked -= OnCheckedChanged;
		platformView.Unchecked -= OnCheckedChanged;
		platformView.Indeterminate -= OnCheckedChanged;
	}

	static void MapIsChecked(AvaloniaCheckBoxHandler handler, ICheckBox checkBox)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.IsChecked = checkBox.IsChecked;
	}

	static void MapForeground(AvaloniaCheckBoxHandler handler, ICheckBox checkBox)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = checkBox.Foreground?.ToAvaloniaBrush();
	}

	void OnCheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (VirtualView is null || PlatformView is null)
			return;

		var current = PlatformView.IsChecked ?? false;
		if (VirtualView.IsChecked != current)
			VirtualView.IsChecked = current;
	}
}
