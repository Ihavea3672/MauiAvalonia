using System;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Avalonia.Dispatching;

internal sealed class AvaloniaDispatcherTimer : IDispatcherTimer, IDisposable
{
	private readonly global::Avalonia.Threading.DispatcherTimer _timer;
	private bool _isRepeating = true;
	private bool _disposed;

	public AvaloniaDispatcherTimer(global::Avalonia.Threading.Dispatcher dispatcher)
	{
		_ = dispatcher;
		_timer = new global::Avalonia.Threading.DispatcherTimer(global::Avalonia.Threading.DispatcherPriority.Normal);
		_timer.Tick += OnTick;
		Interval = TimeSpan.FromMilliseconds(16);
	}

	public TimeSpan Interval
	{
		get => _timer.Interval;
		set => _timer.Interval = value;
	}

	public bool IsRepeating
	{
		get => _isRepeating;
		set => _isRepeating = value;
	}

	public bool IsRunning => _timer.IsEnabled;

	public event EventHandler? Tick;

	public void Start()
	{
		_timer.Start();
	}

	public void Stop()
	{
		_timer.Stop();
	}

	void OnTick(object? sender, EventArgs e)
	{
		Tick?.Invoke(this, EventArgs.Empty);

		if (!_isRepeating)
			Stop();
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_timer.Tick -= OnTick;
		_timer.Stop();
		_disposed = true;
	}
}
