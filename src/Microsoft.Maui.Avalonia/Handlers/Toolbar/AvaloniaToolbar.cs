using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiColors = Microsoft.Maui.Graphics.Colors;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AvaloniaImage = Avalonia.Controls.Image;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace Microsoft.Maui.Avalonia.Handlers;

internal sealed class AvaloniaToolbar : AvaloniaGrid
{
	readonly AvaloniaButton _backButton;
	readonly StackPanel _titleStack;
	readonly AvaloniaTextBlock _titleText;
	readonly AvaloniaImage _titleIcon;
	readonly ContentControl _titleViewHost;
	readonly StackPanel _itemsHost;
	readonly AvaloniaBorder _itemsBorder;
	readonly Dictionary<ToolbarItem, AvaloniaButton> _itemButtons = new();
	readonly AvaloniaThickness _buttonMargin = new(4, 0, 0, 0);
	IMauiContext? _context;
	Control? _defaultTitleView;

	public AvaloniaToolbar()
	{
		RowDefinitions = new RowDefinitions("Auto");
		ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto");
		Height = 44;
		Background = new AvaloniaSolidColorBrush(AvaloniaColor.FromRgb(245, 245, 245));
		VerticalAlignment = AvaloniaVerticalAlignment.Stretch;
		HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch;
		Margin = new AvaloniaThickness(8, 0, 8, 0);

		_backButton = new AvaloniaButton
		{
			Content = "←",
			VerticalAlignment = AvaloniaVerticalAlignment.Center,
			HorizontalAlignment = AvaloniaHorizontalAlignment.Left,
			Margin = new AvaloniaThickness(0, 0, 8, 0),
			IsVisible = false
		};
		_backButton.Click += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
		_backButton.SetValue(AvaloniaGrid.ColumnProperty, 0);

		_titleIcon = new AvaloniaImage
		{
			Width = 16,
			Height = 16,
			Margin = new AvaloniaThickness(0, 0, 6, 0),
			IsVisible = false
		};

		_titleText = new AvaloniaTextBlock
		{
			VerticalAlignment = AvaloniaVerticalAlignment.Center,
			FontSize = 16,
			TextTrimming = TextTrimming.CharacterEllipsis
		};

		_titleStack = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			VerticalAlignment = AvaloniaVerticalAlignment.Center,
			Spacing = 2
		};
		_titleStack.Children.Add(_titleIcon);
		_titleStack.Children.Add(_titleText);

		_titleViewHost = new ContentControl
		{
			Content = _titleStack,
			HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
			VerticalAlignment = AvaloniaVerticalAlignment.Center
		};
		_defaultTitleView = _titleStack;
		_titleViewHost.SetValue(AvaloniaGrid.ColumnProperty, 1);

		_itemsHost = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
			VerticalAlignment = AvaloniaVerticalAlignment.Center,
			Spacing = 4
		};

		_itemsBorder = new AvaloniaBorder
		{
			Child = _itemsHost,
			HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
			VerticalAlignment = AvaloniaVerticalAlignment.Center
		};
		_itemsBorder.SetValue(AvaloniaGrid.ColumnProperty, 2);

		Children.Add(_backButton);
		Children.Add(_titleViewHost);
		Children.Add(_itemsBorder);
	}

	public event EventHandler? BackRequested;

	public void SetContext(IMauiContext? context)
	{
		_context = context;
	}

	public void UpdateTitle(IToolbar toolbar)
	{
		_titleText.Text = toolbar.Title ?? string.Empty;
	}

	public void UpdateTitleView(Toolbar toolbar)
	{
		if (toolbar.TitleView is IView titleView && _context != null)
		{
			_titleViewHost.Content = titleView.ToAvaloniaControl(_context);
		}
		else
		{
			_titleViewHost.Content = _defaultTitleView;
		}
	}

	public void UpdateTitleIcon(Toolbar toolbar)
	{
		if (toolbar.TitleIcon != null)
		{
			LoadImage(toolbar.TitleIcon, image =>
			{
				_titleIcon.Source = image;
				_titleIcon.IsVisible = image != null;
			});
		}
		else
		{
			_titleIcon.Source = null;
			_titleIcon.IsVisible = false;
		}
	}

	public void UpdateBarBackground(Toolbar toolbar)
	{
		if (toolbar.BarBackground is Microsoft.Maui.Controls.SolidColorBrush solidBrush)
		{
			Background = new AvaloniaSolidColorBrush(solidBrush.Color.ToAvaloniaColor());
		}
		else
		{
			Background = new AvaloniaSolidColorBrush(AvaloniaColor.FromRgb(245, 245, 245));
		}
	}

	public void UpdateBarTextColor(Toolbar toolbar)
	{
		if (toolbar.BarTextColor != MauiColors.Transparent)
			_titleText.Foreground = new AvaloniaSolidColorBrush(toolbar.BarTextColor.ToAvaloniaColor());
	}

	public void UpdateIconColor(Toolbar toolbar)
	{
		if (toolbar.IconColor != MauiColors.Transparent)
			_backButton.Foreground = new AvaloniaSolidColorBrush(toolbar.IconColor.ToAvaloniaColor());
	}

	public void UpdateBackButton(Toolbar toolbar)
	{
		_backButton.IsVisible = toolbar.BackButtonVisible;
		_backButton.IsEnabled = toolbar.BackButtonEnabled;
	}

	public void UpdateToolbarItems(Toolbar toolbar)
	{
		_itemsHost.Children.Clear();
		_itemButtons.Clear();

		if (toolbar.ToolbarItems == null)
			return;

		foreach (var item in toolbar.ToolbarItems)
		{
			var button = CreateToolbarButton(item);
			_itemsHost.Children.Add(button);
			_itemButtons[item] = button;
		}
	}

	AvaloniaButton CreateToolbarButton(ToolbarItem item)
	{
		var button = new AvaloniaButton
		{
			Content = item.Text,
			Margin = _buttonMargin,
			HorizontalAlignment = AvaloniaHorizontalAlignment.Right,
			VerticalAlignment = AvaloniaVerticalAlignment.Center,
			MinHeight = 30
		};

		button.Command = new ToolbarItemCommand(item);

		if (item.IconImageSource != null)
		{
			LoadImage(item.IconImageSource, image =>
			{
				if (image is null)
					return;

				button.Content = new AvaloniaImage
				{
					Source = image,
					Width = 18,
					Height = 18
				};
			});
		}

		return button;
	}

	void LoadImage(ImageSource? source, Action<Bitmap?> onLoaded)
	{
		if (source == null || _context == null)
		{
			onLoaded(null);
			return;
		}

		_ = AvaloniaImageSourceLoader.LoadAsync(source, _context.Services, default)
			.ContinueWith(task =>
			{
				var image = task.IsCompletedSuccessfully ? task.Result : null;
				AvaloniaUiDispatcher.UIThread.Post(() => onLoaded(image));
			});
	}

	sealed class ToolbarItemCommand : ICommand
	{
		readonly ToolbarItem _item;

		public ToolbarItemCommand(ToolbarItem item)
		{
			_item = item;
			_item.PropertyChanged += OnPropertyChanged;
		}

		public bool CanExecute(object? parameter) => _item.IsEnabled;

		public event EventHandler? CanExecuteChanged;

		public void Execute(object? parameter)
		{
			if (_item is IMenuItemController controller)
			{
				controller.Activate();
			}
			else
			{
				_item.Command?.Execute(_item.CommandParameter);
			}
		}

		void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(_item.IsEnabled))
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
