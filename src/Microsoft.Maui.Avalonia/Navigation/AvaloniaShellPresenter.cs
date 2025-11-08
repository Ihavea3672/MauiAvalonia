using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Handlers;
using Microsoft.Maui.Avalonia.Navigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AvaloniaSelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Avalonia.Navigation;

internal sealed class AvaloniaShellPresenter : UserControl
{
	readonly AvaloniaGrid _root;
	readonly TabStrip _tabStrip;
	readonly ContentControl _contentHost;
	readonly List<ShellTabItem> _tabItems = new();
	Shell? _shell;
	IShellController? _controller;
	IMauiContext? _context;
	IAvaloniaNavigationRoot? _navigationRoot;
	Toolbar? _currentToolbar;
	IToolbarHandler? _currentToolbarHandler;
	bool _suppressSelection;

	public AvaloniaShellPresenter()
	{
		_tabStrip = new TabStrip
		{
			HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
			VerticalAlignment = AvaloniaVerticalAlignment.Stretch,
			ItemTemplate = new FuncDataTemplate<ShellTabItem>((item, _) =>
				new TextBlock
				{
					Margin = new AvaloniaThickness(12, 4),
					VerticalAlignment = AvaloniaVerticalAlignment.Center,
					Text = item?.Title ?? string.Empty
				}, true)
		};
		_tabStrip.SelectionChanged += OnTabSelectionChanged;

		_contentHost = new ContentControl
		{
			HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
			VerticalAlignment = AvaloniaVerticalAlignment.Stretch
		};

		_root = new AvaloniaGrid
		{
			RowDefinitions = new global::Avalonia.Controls.RowDefinitions("Auto,*")
		};
		_tabStrip.SetValue(AvaloniaGrid.RowProperty, 0);
		_contentHost.SetValue(AvaloniaGrid.RowProperty, 1);
		_root.Children.Add(_tabStrip);
		_root.Children.Add(_contentHost);

		Content = _root;
		UpdateTabStripVisibility();
	}

	public void Attach(IMauiContext context, Shell shell, IShellController? controller)
	{
		if (_shell == shell && ReferenceEquals(_context, context))
			return;

			Detach();

			_context = context ?? throw new ArgumentNullException(nameof(context));
			_navigationRoot = context.Services.GetService<IAvaloniaNavigationRoot>();
			_shell = shell ?? throw new ArgumentNullException(nameof(shell));
			_controller = controller;

		_shell.PropertyChanged += OnShellPropertyChanged;
		if (_controller is not null)
		{
			_controller.StructureChanged += OnShellStructureChanged;
			_controller.FlyoutItemsChanged += OnShellStructureChanged;
		}

		UpdateTabs();
		UpdateSelection();
		UpdateDetail();
	}

	public void Detach()
	{
		if (_shell is not null)
		{
			_shell.PropertyChanged -= OnShellPropertyChanged;
			_shell = null;
		}

		if (_controller is not null)
		{
			_controller.StructureChanged -= OnShellStructureChanged;
			_controller.FlyoutItemsChanged -= OnShellStructureChanged;
			_controller = null;
		}

		_context = null;
		_navigationRoot = null;
		ReleaseToolbar();
		_tabItems.Clear();
		_tabStrip.ItemsSource = null;
		UpdateTabStripVisibility();
		_contentHost.Content = null;
	}

	void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (_shell is null)
			return;

		if (e.PropertyName == nameof(Shell.CurrentItem) ||
			e.PropertyName == nameof(Shell.CurrentState) ||
			e.PropertyName == "CurrentPage")
		{
			UpdateSelection();
			UpdateDetail();
		}
	}

	void OnShellStructureChanged(object? sender, EventArgs e)
	{
		UpdateTabs();
		UpdateSelection();
		UpdateDetail();
	}

	void UpdateTabs()
	{
		if (_shell is null)
			return;

		_tabItems.Clear();

		var shellItems = _controller?.GetItems() ?? _shell.Items;
		if (shellItems is not null)
		{
			foreach (var shellItem in shellItems)
			{
				if (shellItem is not null)
					_tabItems.Add(new ShellTabItem(shellItem));
			}
		}

		_tabStrip.ItemsSource = _tabItems.ToArray();
		UpdateTabStripVisibility();
	}

	void UpdateTabStripVisibility() =>
		_tabStrip.IsVisible = _tabItems.Count > 1;

	void UpdateSelection()
	{
		if (_shell is null)
			return;

		ShellTabItem? selected = null;
		if (_shell.CurrentItem is ShellItem currentItem)
			selected = _tabItems.FirstOrDefault(tab => ReferenceEquals(tab.ShellItem, currentItem));

		try
		{
			_suppressSelection = true;
			_tabStrip.SelectedItem = selected;
		}
		finally
		{
			_suppressSelection = false;
		}
	}

	void UpdateDetail()
	{
		if (_shell is null || _context is null)
		{
			_contentHost.Content = null;
			UpdateChrome(null);
			return;
		}

		var view = ResolveShellDetail(_shell);
		var control = view?.ToAvaloniaControl(_context);
		_contentHost.Content = control;
		UpdateChrome(view);
	}

	async void OnTabSelectionChanged(object? sender, AvaloniaSelectionChangedEventArgs e)
	{
		if (_suppressSelection || _controller is null)
			return;

		if (_tabStrip.SelectedItem is not ShellTabItem tabItem)
			return;

		try
		{
			await _controller.OnFlyoutItemSelectedAsync(tabItem.ShellItem).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[AvaloniaShellPresenter] Shell tab navigation failed: {ex}");
		}
	}

	static IView? ResolveShellDetail(Shell shell)
	{
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

	void UpdateChrome(IView? currentView)
	{
		UpdateTitle();
		UpdateToolbar(currentView);
	}

	void UpdateTitle()
	{
		if (_navigationRoot is null)
			return;

		var title = ResolveTitle();
		_navigationRoot.SetTitle(title);
	}

	string? ResolveTitle()
	{
		if (_shell?.CurrentPage is Page page && !string.IsNullOrWhiteSpace(page.Title))
			return page.Title;

		if (_shell?.CurrentItem?.CurrentItem?.CurrentItem is ShellContent shellContent && !string.IsNullOrWhiteSpace(shellContent.Title))
			return shellContent.Title;

		if (_shell?.CurrentItem?.CurrentItem is ShellSection section && !string.IsNullOrWhiteSpace(section.Title))
			return section.Title;

		if (_shell?.CurrentItem is ShellItem item && !string.IsNullOrWhiteSpace(item.Title))
			return item.Title;

		return _shell?.Title;
	}

	void UpdateToolbar(IView? view)
	{
		if (_navigationRoot is null)
			return;

		var toolbarElement = ResolveToolbarElement(view);
		if (toolbarElement?.Toolbar is not Toolbar toolbar || _context is null)
		{
			_navigationRoot.SetToolbar(null);
			return;
		}

		var handler = EnsureToolbarHandler(toolbar);
		if (handler?.PlatformView is Control control)
		{
			_navigationRoot.SetToolbar(control);
		}
		else
		{
			_navigationRoot.SetToolbar(null);
		}
	}

	IToolbarElement? ResolveToolbarElement(IView? view)
	{
		if (view is IToolbarElement toolbarElement)
			return toolbarElement;

		if (view is VisualElement element && element.Parent is IToolbarElement parentToolbarElement)
			return parentToolbarElement;

		return _shell;
	}

	IToolbarHandler? EnsureToolbarHandler(Toolbar toolbar)
	{
		if (_context is null)
			return null;

		if (!ReferenceEquals(_currentToolbar, toolbar))
		{
			ReleaseToolbar();
			toolbar.Handler?.DisconnectHandler();
			toolbar.ToHandler(_context);
			_currentToolbar = toolbar;
			_currentToolbarHandler = toolbar.Handler as IToolbarHandler;
		}

		return _currentToolbarHandler;
	}

	void ReleaseToolbar()
	{
		if (_currentToolbarHandler is IElementHandler handler)
			handler.DisconnectHandler();

		_currentToolbarHandler = null;
		_currentToolbar = null;
	}

	sealed class ShellTabItem
	{
		public ShellTabItem(ShellItem shellItem) =>
			ShellItem = shellItem ?? throw new ArgumentNullException(nameof(shellItem));

		public ShellItem ShellItem { get; }

		public string Title =>
			!string.IsNullOrWhiteSpace(ShellItem.Title) ? ShellItem.Title :
			!string.IsNullOrWhiteSpace(ShellItem.Route) ? ShellItem.Route :
			ShellItem.ToString() ?? string.Empty;
	}
}
