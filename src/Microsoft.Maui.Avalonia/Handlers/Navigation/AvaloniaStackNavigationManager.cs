using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Maui.Avalonia.Handlers;

namespace Microsoft.Maui.Avalonia.Navigation;

internal sealed class AvaloniaStackNavigationManager
{
	IReadOnlyList<IView> _currentStack = Array.Empty<IView>();
	IStackNavigation? _navigationView;
	ContentControl? _presenter;
	IMauiContext? _mauiContext;

	public void Connect(IStackNavigation navigationView, ContentControl presenter, IMauiContext? context)
	{
		_navigationView = navigationView;
		_presenter = presenter;
		_mauiContext = context;
	}

	public void Disconnect()
	{
		_navigationView = null;
		_presenter = null;
		_mauiContext = null;
	}

	public void NavigateTo(NavigationRequest request)
	{
		_currentStack = request.NavigationStack;
		if (_currentStack.Count == 0)
		{
			_presenter?.SetValue(ContentControl.ContentProperty, null);
			_navigationView?.NavigationFinished(_currentStack);
			return;
		}

		if (_mauiContext is null || _presenter is null)
			return;

		if (_currentStack[^1] is not IView view)
			return;

		var control = view.ToAvaloniaControl(_mauiContext);
		_presenter.Content = control;
		_navigationView?.NavigationFinished(_currentStack);
	}
}
