using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Microsoft.Maui.Avalonia.Handlers;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Avalonia.Navigation;

internal sealed class AvaloniaStackNavigationManager
{
	static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(200);
	readonly Dictionary<IView, Control> _realizedViews = new();

	IReadOnlyList<IView> _currentStack = Array.Empty<IView>();
	IStackNavigation? _navigationView;
	ContentControl? _presenter;
	TransitioningContentControl? _transitionHost;
	AvaloniaGrid? _hostGrid;
	Panel? _modalLayer;
	IMauiContext? _mauiContext;
	IPageTransition _defaultTransition = new CrossFade(DefaultTransitionDuration);

	public void Connect(IStackNavigation navigationView, ContentControl presenter, IMauiContext? context)
	{
		_navigationView = navigationView;
		_presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
		_mauiContext = context;
		EnsureHost();
	}

	public void Disconnect()
	{
		ClearHostContent();
		_realizedViews.Clear();
		_currentStack = Array.Empty<IView>();
		_navigationView = null;
		_presenter = null;
		_transitionHost = null;
		_mauiContext = null;
	}

	public void NavigateTo(NavigationRequest request)
	{
		_currentStack = request.NavigationStack;
		EnsureHost();

		if (_currentStack.Count == 0)
		{
			ClearHostContent();
			_navigationView?.NavigationFinished(_currentStack);
			return;
		}

		if (_mauiContext is null)
			return;

		var baseView = ResolveBaseView(_currentStack, out var modalViews);
		if (baseView is not null)
		{
			var control = GetOrCreateControl(baseView);
			var animateBase = request.Animated && modalViews.Count == 0;
			ShowBaseControl(control, animateBase);
		}

		UpdateModalOverlays(modalViews);
		RecycleStaleViews(_currentStack);
		_navigationView?.NavigationFinished(_currentStack);
	}

	void EnsureHost()
	{
		if (_presenter is null)
			return;

		if (_hostGrid is null)
		{
			_transitionHost = new TransitioningContentControl
			{
				HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
				VerticalAlignment = AvaloniaVerticalAlignment.Stretch,
				PageTransition = _defaultTransition
			};

			_modalLayer = new AvaloniaGrid
			{
				HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
				VerticalAlignment = AvaloniaVerticalAlignment.Stretch,
				IsHitTestVisible = true
			};

			_hostGrid = new AvaloniaGrid();
			_hostGrid.Children.Add(_transitionHost);
			_hostGrid.Children.Add(_modalLayer);
		}

		_presenter.Content = _hostGrid;
	}

	void ClearHostContent()
	{
		if (_transitionHost is not null)
			_transitionHost.Content = null;

		_modalLayer?.Children.Clear();
		if (_presenter is not null && _hostGrid is null)
		{
			_presenter.SetValue(ContentControl.ContentProperty, null);
		}
	}

	Control GetOrCreateControl(IView view)
	{
		if (_realizedViews.TryGetValue(view, out var control))
			return control;

		if (_mauiContext is null)
			throw new InvalidOperationException("MAUI context is unavailable for navigation.");

		control = view.ToAvaloniaControl(_mauiContext)
			?? throw new InvalidOperationException($"Failed to create platform view for {view}.");

		_realizedViews[view] = control;
		return control;
	}

	void ShowBaseControl(Control control, bool animated)
	{
		if (_transitionHost is not null)
		{
			var transition = _transitionHost.PageTransition;

			if (!animated)
				_transitionHost.PageTransition = null;

			_transitionHost.Content = control;

			if (!animated)
				_transitionHost.PageTransition = transition;
		}
	}

	void RecycleStaleViews(IReadOnlyList<IView> liveStack)
	{
		if (_realizedViews.Count == 0)
			return;

		var liveSet = liveStack.ToHashSet();
		var stale = _realizedViews.Keys.Where(view => !liveSet.Contains(view)).ToList();

		foreach (var view in stale)
		{
			if (_realizedViews.TryGetValue(view, out var control))
			{
				if (control is IDisposable disposable)
					disposable.Dispose();
			}

			if (view is IElement element && element.Handler is IElementHandler handler)
			{
				handler.DisconnectHandler();
				element.Handler = null;
			}

			_realizedViews.Remove(view);
		}
	}

	static IView? ResolveBaseView(IReadOnlyList<IView> stack, out List<IView> modalViews)
	{
		modalViews = new List<IView>();
		IView? baseView = null;

		foreach (var view in stack)
		{
			if (IsModalView(view))
			{
				modalViews.Add(view);
			}
			else
			{
				baseView = view;
				modalViews.Clear();
			}
		}

		baseView ??= stack.LastOrDefault();
		return baseView;
	}

	static bool IsModalView(IView view) =>
		view is Page page && page.Navigation?.ModalStack?.Contains(page) == true;

	void UpdateModalOverlays(IReadOnlyList<IView> modalViews)
	{
		if (_modalLayer is null)
			return;

		_modalLayer.Children.Clear();

		if (modalViews.Count == 0)
			return;

		foreach (var modalView in modalViews)
		{
			var control = GetOrCreateControl(modalView);
			var overlay = new AvaloniaBorderControl
			{
				Background = new AvaloniaSolidColorBrush(AvaloniaColor.FromArgb(0x60, 0, 0, 0)),
				HorizontalAlignment = AvaloniaHorizontalAlignment.Stretch,
				VerticalAlignment = AvaloniaVerticalAlignment.Stretch,
				Child = control
			};
			_modalLayer.Children.Add(overlay);
		}
	}
}
