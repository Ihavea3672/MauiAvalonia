using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaLayoutHandler : AvaloniaPanelLayoutHandler<ILayout, AvaloniaLayoutPanel>
{
	public static IPropertyMapper<ILayout, AvaloniaLayoutHandler> Mapper =
		new PropertyMapper<ILayout, AvaloniaLayoutHandler>(ViewHandler.ViewMapper);

	public AvaloniaLayoutHandler()
		: base(Mapper)
	{
	}

	protected override void ConnectHandler(AvaloniaLayoutPanel platformView)
	{
		base.ConnectHandler(platformView);
		platformView.CrossPlatformLayout = VirtualView;
	}

	protected override void AddChildControl(IView view, Control control)
	{
		// The cross-platform layout panel manages its own visual children.
	}

	protected override void OnChildrenUpdated()
	{
		if (PlatformView is null || VirtualView is null || MauiContext is null)
			return;

		var snapshot = new List<IView>(VirtualView.Count);
		for (var i = 0; i < VirtualView.Count; i++)
			snapshot.Add(VirtualView[i]);

		snapshot.Sort((left, right) => left.ZIndex.CompareTo(right.ZIndex));
		PlatformView.UpdateChildren(MauiContext, snapshot);
	}
}
