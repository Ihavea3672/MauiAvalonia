using System;
using Avalonia.Threading;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Avalonia.Animations;

internal sealed class AvaloniaTicker : ITicker, IDisposable
{
	private readonly DispatcherTimer _timer;

	public AvaloniaTicker()
	{
		_timer = new DispatcherTimer(DispatcherPriority.Render);
		_timer.Tick += OnTick;
		UpdateInterval();
	}

	public bool IsRunning => _timer.IsEnabled;

	public bool SystemEnabled { get; private set; } = true;

	private int _maxFps = 60;
	public int MaxFps
	{
		get => _maxFps;
		set
		{
			_maxFps = Math.Max(1, value);
			UpdateInterval();
		}
	}

	public Action? Fire { get; set; }

	public void Start()
	{
		if (!_timer.IsEnabled)
			_timer.Start();
	}

	public void Stop()
	{
		if (_timer.IsEnabled)
			_timer.Stop();
	}

	void UpdateInterval()
	{
		var fps = Math.Max(1, MaxFps);
		_timer.Interval = TimeSpan.FromSeconds(1d / fps);
	}

	void OnTick(object? sender, EventArgs e)
	{
		if (!SystemEnabled)
		{
			Stop();
			Fire?.Invoke();
			return;
		}

		Fire?.Invoke();
	}

	public void Dispose()
	{
		_timer.Tick -= OnTick;
		_timer.Stop();
	}
}
