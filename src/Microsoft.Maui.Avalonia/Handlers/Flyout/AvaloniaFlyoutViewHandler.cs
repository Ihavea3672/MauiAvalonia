using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Navigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public sealed class AvaloniaFlyoutViewHandler : ViewHandler<IFlyoutView, SplitView>, IFlyoutViewHandler
{
	static readonly IPropertyMapper<IFlyoutView, IFlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, IFlyoutViewHandler>(ViewHandler.ViewMapper)
	{
		[nameof(IFlyoutView.Flyout)] = MapFlyout,
		[nameof(IFlyoutView.Detail)] = MapDetail,
		[nameof(IFlyoutView.IsPresented)] = MapIsPresented,
		[nameof(IFlyoutView.FlyoutWidth)] = MapFlyoutWidth,
		[nameof(IFlyoutView.FlyoutBehavior)] = MapFlyoutBehavior,
		[nameof(IFlyoutView.IsGestureEnabled)] = MapIsGestureEnabled
	};

	readonly AvaloniaShellFlyoutPresenter _flyoutPresenter = new();
	readonly AvaloniaShellPresenter _shellPresenter = new();

	Shell? _trackedShell;
	IShellController? _controller;

	public AvaloniaFlyoutViewHandler()
		: base(Mapper)
	{
	}

	protected override SplitView CreatePlatformView() => new()
	{
		DisplayMode = SplitViewDisplayMode.Inline,
		IsPaneOpen = false,
		OpenPaneLength = 320
	};

	static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView view)
	{
		if (handler is not AvaloniaFlyoutViewHandler platformHandler || handler.MauiContext is null)
			return;

		platformHandler.EnsureShellSubscriptions(view);
		platformHandler.PlatformView.Pane = platformHandler.ResolveFlyoutPane(view);
	}

	static void MapDetail(IFlyoutViewHandler handler, IFlyoutView view)
	{
		if (handler is not AvaloniaFlyoutViewHandler platformHandler || handler.MauiContext is null)
			return;

		platformHandler.EnsureShellSubscriptions(view);

		if (view is Shell shell)
		{
			platformHandler._shellPresenter.Attach(handler.MauiContext, shell, platformHandler._controller);
			platformHandler.PlatformView.Content = platformHandler._shellPresenter;
			return;
		}

		var detailView = view.Detail ?? platformHandler.ResolveFallbackDetail(view);
		platformHandler.PlatformView.Content = detailView?.ToAvaloniaControl(handler.MauiContext);
	}

	static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView view)
	{
		if (handler is AvaloniaFlyoutViewHandler platformHandler)
			platformHandler.PlatformView.IsPaneOpen = view.IsPresented;
	}

	static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView view)
	{
		if (handler is AvaloniaFlyoutViewHandler platformHandler && view.FlyoutWidth > 0)
			platformHandler.PlatformView.OpenPaneLength = view.FlyoutWidth;
	}

	static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView view)
	{
		if (handler is AvaloniaFlyoutViewHandler platformHandler)
		{
			platformHandler.PlatformView.DisplayMode = view.FlyoutBehavior switch
			{
				FlyoutBehavior.Locked => SplitViewDisplayMode.Inline,
				FlyoutBehavior.Disabled => SplitViewDisplayMode.Overlay,
				_ => SplitViewDisplayMode.Overlay
			};
		}
	}

	static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView view)
	{
		// SplitView handles gestures internally.
	}

	void EnsureShellSubscriptions(IFlyoutView view)
	{
		if (_trackedShell is not null && ReferenceEquals(_trackedShell, view))
			return;

		ResetShellSubscriptions();

		if (view is not Shell shell)
			return;

		_trackedShell = shell;
		shell.PropertyChanged += OnShellPropertyChanged;

		if (shell is IShellController controller)
		{
			_controller = controller;
			controller.StructureChanged += OnShellStructureChanged;
			controller.FlyoutItemsChanged += OnShellStructureChanged;
		}

		_flyoutPresenter.AttachShell(shell, _controller);
	}

	void ResetShellSubscriptions()
	{
		if (_trackedShell is not null)
		{
			_trackedShell.PropertyChanged -= OnShellPropertyChanged;
			_trackedShell = null;
		}

		if (_controller is not null)
		{
			_controller.StructureChanged -= OnShellStructureChanged;
			_controller.FlyoutItemsChanged -= OnShellStructureChanged;
			_controller = null;
		}

		_flyoutPresenter.AttachShell(null, null);
		_shellPresenter.Detach();
	}

	void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (_trackedShell is null)
			return;

		if (e.PropertyName == nameof(Shell.CurrentItem) ||
			e.PropertyName == nameof(Shell.CurrentState) ||
			e.PropertyName == "CurrentPage")
		{
			MapDetail(this, _trackedShell);
			_flyoutPresenter.UpdateSelection();
		}
	}

	void OnShellStructureChanged(object? sender, EventArgs e)
	{
		if (_trackedShell is null)
		 return;

		MapDetail(this, _trackedShell);
		_flyoutPresenter.UpdateItems();
	}

	protected override void DisconnectHandler(SplitView platformView)
	{
		ResetShellSubscriptions();
		base.DisconnectHandler(platformView);
	}

	Control? ResolveFlyoutPane(IFlyoutView view)
	{
		if (MauiContext is null)
			return null;

		if (view.Flyout is IView flyoutView)
			return flyoutView.ToAvaloniaControl(MauiContext);

		if (_trackedShell is not null)
		{
			_flyoutPresenter.AttachShell(_trackedShell, _controller);
			return _flyoutPresenter;
		}

		return null;
	}

	IView? ResolveFallbackDetail(IFlyoutView view)
	{
		if (view is not Shell shell)
			return view.Detail;

		if (shell.CurrentPage is IView pageView)
		 return pageView;

		var shellContent = shell.CurrentItem?.CurrentItem?.CurrentItem
			?? shell.Items?.FirstOrDefault()?.Items?.FirstOrDefault()?.Items?.FirstOrDefault();

		if (shellContent is null)
			return null;

		if (shellContent is IShellContentController controller)
			return controller.GetOrCreateContent() as IView;

		return shellContent.Content as IView;
	}

	IFlyoutView IFlyoutViewHandler.VirtualView => VirtualView;

	object IFlyoutViewHandler.PlatformView => PlatformView!;
}
