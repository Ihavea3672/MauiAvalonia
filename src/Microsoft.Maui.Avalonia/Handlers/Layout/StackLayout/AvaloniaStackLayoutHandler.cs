using System;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaStackLayoutHandler : AvaloniaPanelLayoutHandler<IStackLayout, AvaloniaStackPanel>
{
	public static PropertyMapper<IStackLayout, AvaloniaStackLayoutHandler> Mapper =
		new PropertyMapper<IStackLayout, AvaloniaStackLayoutHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IStackLayout.Spacing)] = MapSpacing,
			["Orientation"] = MapOrientation
		};

	public AvaloniaStackLayoutHandler()
		: base(Mapper)
	{
	}

	static void MapSpacing(AvaloniaStackLayoutHandler handler, IStackLayout layout)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Spacing = layout.Spacing;
	}

	static void MapOrientation(AvaloniaStackLayoutHandler handler, IStackLayout layout)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Orientation = layout.GetStackOrientation();
	}

	protected override void OnChildrenUpdated()
	{
		base.OnChildrenUpdated();

		if (PlatformView is null || VirtualView is null)
			return;

		PlatformView.Spacing = VirtualView.Spacing;
		PlatformView.Orientation = VirtualView.GetStackOrientation();
	}
}

public sealed class AvaloniaStackPanel : StackPanel
{
}
