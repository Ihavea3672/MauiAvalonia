using System;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Avalonia.Devices;

#pragma warning disable CS0067
internal sealed class AvaloniaDeviceDisplay : IDeviceDisplay, IDisposable
{
	private bool _keepScreenOn;
	private readonly DisplayInfo _defaultInfo = new(1920, 1080, 1, DisplayOrientation.Landscape, DisplayRotation.Rotation0);

	public AvaloniaDeviceDisplay()
	{
	}

	public bool KeepScreenOn
	{
		get => _keepScreenOn;
		set => _keepScreenOn = value;
	}

	public DisplayInfo MainDisplayInfo => _defaultInfo;

	public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged;

	public void Dispose()
	{
	}
}
#pragma warning restore CS0067
