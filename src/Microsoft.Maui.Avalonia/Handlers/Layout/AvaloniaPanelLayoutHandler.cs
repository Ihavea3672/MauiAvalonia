using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public abstract class AvaloniaPanelLayoutHandler<TLayout, TPanel> : AvaloniaViewHandler<TLayout, TPanel>, ILayoutHandler
	where TLayout : class, ILayout
	where TPanel : Panel, new()
{
	protected AvaloniaPanelLayoutHandler(IPropertyMapper mapper)
		: base(mapper)
	{
	}

	protected override TPanel CreatePlatformView() => new();

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);
		SyncChildren();
	}

	public void Add(IView child) => SyncChildren();

	public void Remove(IView child) => SyncChildren();

	public void Clear() => PlatformView?.Children.Clear();

	public void Insert(int index, IView view) => SyncChildren();

	public void Update(int index, IView view) => SyncChildren();

	public void UpdateZIndex(IView view) => SyncChildren();

	protected override void ConnectHandler(TPanel platformView)
	{
		base.ConnectHandler(platformView);
		SyncChildren();
	}

	protected virtual void OnChildrenUpdated()
	{
	}

	protected virtual void AddChildControl(IView view, Control control) =>
		PlatformView?.Children.Add(control);

	void SyncChildren()
	{
		if (MauiContext is null || PlatformView is null || VirtualView is null)
			return;

		var snapshot = new List<IView>(VirtualView.Count);
		for (var i = 0; i < VirtualView.Count; i++)
			snapshot.Add(VirtualView[i]);

		snapshot.Sort((left, right) => left.ZIndex.CompareTo(right.ZIndex));

		PlatformView.Children.Clear();

		foreach (var child in snapshot)
		{
			var control = child.ToAvaloniaControl(MauiContext);
			if (control is null)
				continue;

			AddChildControl(child, control);
		}

		OnChildrenUpdated();
	}

	ILayout ILayoutHandler.VirtualView => (ILayout)VirtualView!;

	object ILayoutHandler.PlatformView => PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} is not available.");
}
