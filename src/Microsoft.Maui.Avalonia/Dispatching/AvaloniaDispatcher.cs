using System;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Avalonia.Dispatching;

internal sealed class AvaloniaDispatcher : IDispatcher
{
	private readonly global::Avalonia.Threading.Dispatcher _dispatcher;

	public AvaloniaDispatcher(global::Avalonia.Threading.Dispatcher dispatcher)
	{
		_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
	}

	public bool IsDispatchRequired => !_dispatcher.CheckAccess();

	public bool Dispatch(Action action)
	{
		if (action is null)
			return false;

		try
		{
			if (IsDispatchRequired)
				_ = _dispatcher.InvokeAsync(action);
			else
				action();

			return true;
		}
		catch
		{
			return false;
		}
	}

	public bool DispatchDelayed(TimeSpan delay, Action action)
	{
		if (action is null)
			return false;

		try
		{
			var timer = new global::Avalonia.Threading.DispatcherTimer(global::Avalonia.Threading.DispatcherPriority.Normal)
			{
				Interval = delay
			};

			void OnTick(object? sender, EventArgs args)
			{
				timer.Tick -= OnTick;
				timer.Stop();
				Dispatch(action);
			}

			timer.Tick += OnTick;
			timer.Start();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public IDispatcherTimer CreateTimer() => new AvaloniaDispatcherTimer(_dispatcher);
}
