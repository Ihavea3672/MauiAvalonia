using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using MauiControls = global::Microsoft.Maui.Controls;
using AvaloniaVisualTreeAttachmentEventArgs = global::Avalonia.VisualTreeAttachmentEventArgs;

namespace Microsoft.Maui.Avalonia.Handlers;

using AvaloniaSelectionChangedEventArgs = global::Avalonia.Controls.SelectionChangedEventArgs;
using AvaloniaSelectionMode = global::Avalonia.Controls.SelectionMode;
using MauiSelectionMode = global::Microsoft.Maui.Controls.SelectionMode;

public class AvaloniaCollectionViewHandler : ViewHandler<MauiControls.CollectionView, ListBox>
{
	public static readonly IPropertyMapper<MauiControls.CollectionView, AvaloniaCollectionViewHandler> Mapper =
		new PropertyMapper<MauiControls.CollectionView, AvaloniaCollectionViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(MauiControls.ItemsView.ItemsSource)] = MapItemsSource,
			[nameof(MauiControls.ItemsView.ItemTemplate)] = MapItemTemplate,
			[nameof(MauiControls.SelectableItemsView.SelectionMode)] = MapSelectionMode,
			[nameof(MauiControls.SelectableItemsView.SelectedItem)] = MapSelectedItem
		};

	public AvaloniaCollectionViewHandler()
		: base(Mapper)
	{
	}

	bool _suppressSelectionUpdates;

	protected override ListBox CreatePlatformView() => new();

	protected override void ConnectHandler(ListBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.SelectionChanged += OnSelectionChanged;
	}

	protected override void DisconnectHandler(ListBox platformView)
	{
		platformView.SelectionChanged -= OnSelectionChanged;
		base.DisconnectHandler(platformView);
	}

	static void MapItemsSource(AvaloniaCollectionViewHandler handler, MauiControls.CollectionView view) =>
		handler.UpdateItemsSource();

	static void MapItemTemplate(AvaloniaCollectionViewHandler handler, MauiControls.CollectionView view) =>
		handler.UpdateItemTemplate();

	static void MapSelectionMode(AvaloniaCollectionViewHandler handler, MauiControls.CollectionView view) =>
		handler.UpdateSelectionMode();

	static void MapSelectedItem(AvaloniaCollectionViewHandler handler, MauiControls.CollectionView view) =>
		handler.UpdateSelectedItem();

	void UpdateItemsSource()
	{
		if (PlatformView is null || VirtualView is null)
			return;

		if (MauiContext is not null)
			UpdateItemTemplate();

		PlatformView.ItemsSource = VirtualView.ItemsSource ?? Array.Empty<object>();
		UpdateSelectionMode();
		UpdateSelectedItem();
	}

	void UpdateItemTemplate()
	{
		if (PlatformView is null || VirtualView is null || MauiContext is null)
			return;

		PlatformView.DataTemplates.Clear();
		PlatformView.DataTemplates.Add(new MauiCollectionViewTemplate(VirtualView, MauiContext));
	}

	void UpdateSelectionMode()
	{
		if (PlatformView is null || VirtualView is null)
			return;

		PlatformView.SelectionMode = VirtualView.SelectionMode switch
		{
			MauiSelectionMode.Multiple => AvaloniaSelectionMode.Multiple,
			MauiSelectionMode.Single => AvaloniaSelectionMode.Single,
			_ => AvaloniaSelectionMode.Single
		};
	}

	void UpdateSelectedItem()
	{
		if (PlatformView is null || VirtualView is null)
			return;

		try
		{
			_suppressSelectionUpdates = true;
			PlatformView.SelectedItem = VirtualView.SelectedItem;
		}
		finally
		{
			_suppressSelectionUpdates = false;
		}
	}

	void OnSelectionChanged(object? sender, AvaloniaSelectionChangedEventArgs e)
	{
		if (VirtualView is null || _suppressSelectionUpdates)
			return;

		try
		{
			_suppressSelectionUpdates = true;

		if (VirtualView.SelectionMode == MauiSelectionMode.None)
		{
			if (PlatformView?.SelectedItem is not null)
				PlatformView.SelectedItem = null;
			return;
		}

		if (VirtualView.SelectionMode == MauiSelectionMode.Multiple)
		{
			var selection = PlatformView?.SelectedItems?.Cast<object>().ToList() ?? new List<object>();
			VirtualView.UpdateSelectedItems(selection);
		}
		else
			{
				VirtualView.SelectedItem = PlatformView?.SelectedItem;
			}
		}
		finally
		{
			_suppressSelectionUpdates = false;
		}
	}
}

sealed class MauiCollectionViewTemplate : IDataTemplate
{
	readonly MauiControls.CollectionView _collectionView;
	readonly IMauiContext _context;

	public MauiCollectionViewTemplate(MauiControls.CollectionView collectionView, IMauiContext context)
	{
		_collectionView = collectionView;
		_context = context;
	}

	public bool Match(object? data) => true;

	public Control Build(object? param)
	{
		var view = CreateView(param);
		if (view is null)
			return new ContentControl();

		view.BindingContext = param;
		return new MauiCollectionViewItem(view, _context);
	}

	MauiControls.View? CreateView(object? item)
	{
		if (_collectionView.ItemTemplate is MauiControls.DataTemplate template)
		{
			var content = template.CreateContent();
			if (content is MauiControls.View view)
				return view;

			if (content is MauiControls.ViewCell cell)
				return cell.View;
		}

		return new MauiControls.Label
		{
			Text = item?.ToString() ?? string.Empty
		};
	}
}

sealed class MauiCollectionViewItem : ContentControl
{
	readonly IView _view;

	public MauiCollectionViewItem(IView view, IMauiContext context)
	{
		_view = view;
		Content = view.ToAvaloniaControl(context);
	}

	protected override void OnDetachedFromVisualTree(AvaloniaVisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		_view.Handler?.DisconnectHandler();
	}
}
