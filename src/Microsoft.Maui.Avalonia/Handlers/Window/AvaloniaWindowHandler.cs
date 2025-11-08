using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Navigation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Graphics;
using MauiRect = Microsoft.Maui.Graphics.Rect;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaWindowHandler : ElementHandler<IWindow, AvaloniaWindowControl>
{
	public static IPropertyMapper<IWindow, AvaloniaWindowHandler> Mapper =
		new PropertyMapper<IWindow, AvaloniaWindowHandler>(ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
			[nameof(IWindow.Content)] = MapContent,
			[nameof(IWindow.X)] = MapX,
			[nameof(IWindow.Y)] = MapY,
			[nameof(IWindow.Width)] = MapWidth,
			[nameof(IWindow.Height)] = MapHeight,
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
			[nameof(IMenuBarElement.MenuBar)] = MapMenuBar,
			["TitleBar"] = MapTitleBar,
			["TitleBarDragRectangles"] = MapTitleBarDragRectangles
		};

	public AvaloniaWindowHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaWindowControl CreatePlatformElement()
	{
		if (MauiContext is MauiContext context &&
			context.Services.GetService(typeof(global::Avalonia.Controls.Window)) is global::Avalonia.Controls.Window window)
		{
			return window;
		}

		throw new InvalidOperationException("Avalonia Window was not registered with the MAUI context.");
	}

	protected override void ConnectHandler(AvaloniaWindowControl platformView)
	{
		VirtualView?.Created();
		platformView.Opened += OnOpened;
		platformView.Closed += OnClosed;
	}

	protected override void DisconnectHandler(AvaloniaWindowControl platformView)
	{
		platformView.Opened -= OnOpened;
		platformView.Closed -= OnClosed;
	}

	static void MapTitle(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Title = window.Title ?? string.Empty;
		GetNavigationRoot(handler)?.SetTitle(window.Title);
	}

	static void MapContent(AvaloniaWindowHandler handler, IWindow window)
	{
		var navigationRoot = GetNavigationRoot(handler);
		if (navigationRoot is null || handler.MauiContext is null)
			return;

		if (window.Content is IView view)
		{
			var control = view.ToAvaloniaControl(handler.MauiContext);
			navigationRoot.SetContent(control);
		}
		else
		{
			navigationRoot.SetPlaceholder("No content assigned to the window.");
		}
	}

	static void MapX(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.PlatformView is null || double.IsNaN(window.X))
			return;

		handler.PlatformView.Position = new PixelPoint((int)window.X, handler.PlatformView.Position.Y);
	}

	static void MapY(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.PlatformView is null || double.IsNaN(window.Y))
			return;

		handler.PlatformView.Position = new PixelPoint(handler.PlatformView.Position.X, (int)window.Y);
	}

	static void MapWidth(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.PlatformView is null || double.IsNaN(window.Width))
			return;

		handler.PlatformView.Width = window.Width;
	}

	static void MapHeight(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.PlatformView is null || double.IsNaN(window.Height))
			return;

		handler.PlatformView.Height = window.Height;
	}

	static void MapToolbar(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.MauiContext is null)
			return;

		var navigationRoot = GetNavigationRoot(handler);
		if (navigationRoot is null || window is not IToolbarElement toolbarElement)
			return;

		if (toolbarElement.Toolbar is null)
		{
			navigationRoot.SetToolbar(null);
			return;
		}

		var toolbarHandler = toolbarElement.Toolbar.ToHandler(handler.MauiContext);
		if (toolbarHandler.PlatformView is Control toolbarControl)
		{
			navigationRoot.SetToolbar(toolbarControl);
		}
	}

	static void MapMenuBar(AvaloniaWindowHandler handler, IWindow window)
	{
		var navigationRoot = GetNavigationRoot(handler);
		if (navigationRoot is null || handler.MauiContext is null)
			return;

		if (window is not IMenuBarElement menuElement || menuElement.MenuBar is null)
		{
			navigationRoot.SetMenu(null);
			return;
		}

		var menuControl = AvaloniaMenuBuilder.BuildMenu(menuElement.MenuBar, handler.MauiContext);
		navigationRoot.SetMenu(menuControl);
	}

	static void MapTitleBar(AvaloniaWindowHandler handler, IWindow window)
	{
		if (handler.MauiContext is null)
			return;

		var navigationRoot = GetNavigationRoot(handler);
		if (navigationRoot is null)
			return;

		var titleBarObject = TryGetTitleBarObject(window);
		if (titleBarObject is not IView titleBarView)
		{
			navigationRoot.SetTitleBar(null, Array.Empty<Control>());
			return;
		}

		var titleBarControl = titleBarView.ToAvaloniaControl(handler.MauiContext);
		var passthrough = new List<Control>();
		foreach (var element in EnumeratePassthroughViews(titleBarObject))
		{
			if (element?.ToHandler(handler.MauiContext).PlatformView is Control control)
			{
				passthrough.Add(control);
			}
		}

		navigationRoot.SetTitleBar(titleBarControl, passthrough);
	}

	static void MapTitleBarDragRectangles(AvaloniaWindowHandler handler, IWindow window)
	{
		var navigationRoot = GetNavigationRoot(handler);
		if (navigationRoot is null)
			return;

		navigationRoot.SetDragRectangles(TryGetTitleBarDragRectangles(window) ?? Array.Empty<MauiRect>());
	}

	static IAvaloniaNavigationRoot? GetNavigationRoot(AvaloniaWindowHandler handler) =>
		handler.MauiContext?.Services.GetService<IAvaloniaNavigationRoot>();

	static object? TryGetTitleBarObject(IWindow window)
	{
		var property = window.GetType().GetProperty("TitleBar");
		return property?.GetValue(window);
	}

	static MauiRect[]? TryGetTitleBarDragRectangles(IWindow window)
	{
		var property = window.GetType().GetProperty("TitleBarDragRectangles");
		return property?.GetValue(window) as MauiRect[];
	}

	static IEnumerable<IView?> EnumeratePassthroughViews(object titleBar)
	{
		var property = titleBar.GetType().GetProperty("PassthroughElements");
		if (property?.GetValue(titleBar) is IEnumerable<IView?> views)
			return views;

		return Array.Empty<IView?>();
	}

	void OnOpened(object? sender, System.EventArgs e) => VirtualView?.Activated();

	void OnClosed(object? sender, System.EventArgs e) => VirtualView?.Destroying();
}
