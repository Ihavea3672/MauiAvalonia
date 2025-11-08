using System;
using Avalonia.Controls;
using Microsoft.Maui;
using AvaloniaMenuItem = Avalonia.Controls.MenuItem;
using AvaloniaSeparator = Avalonia.Controls.Separator;

namespace Microsoft.Maui.Avalonia.Navigation;

internal static class AvaloniaMenuBuilder
{
	public static Control? BuildMenu(IMenuBar? menuBar, IMauiContext context)
	{
		_ = context ?? throw new ArgumentNullException(nameof(context));
		if (menuBar is null || menuBar.Count == 0)
			return null;

		var menu = new Menu
		{
			HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
			VerticalAlignment = AvaloniaVerticalAlignment.Stretch
		};

		foreach (var item in menuBar)
		{
			menu.Items.Add(CreateMenuBarItem(item));
		}

		return menu;
	}

	static AvaloniaMenuItem CreateMenuBarItem(IMenuBarItem item)
	{
		var menuItem = new AvaloniaMenuItem
		{
			Header = item.Text,
			IsEnabled = item.IsEnabled
		};

		foreach (var child in item)
		{
			menuItem.Items.Add(CreateMenuElement(child));
		}

		return menuItem;
	}

	static object CreateMenuElement(IMenuElement element)
	{
		if (element is IMenuFlyoutSeparator)
		{
			return new AvaloniaSeparator();
		}

		var menuItem = new AvaloniaMenuItem
		{
			Header = element.Text,
			IsEnabled = element.IsEnabled
		};

		menuItem.Click += (_, __) => element.Clicked();

		if (element is IMenuFlyoutSubItem subItem)
		{
			foreach (var child in subItem)
			{
				menuItem.Items.Add(CreateMenuElement(child));
			}
		}

		return menuItem;
	}
}
