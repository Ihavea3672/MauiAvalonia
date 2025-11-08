using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Handlers;
using AvaloniaSliderControl = Avalonia.Controls.Slider;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaSliderHandler : AvaloniaViewHandler<ISlider, AvaloniaSliderControl>, ISliderHandler
{
	public static IPropertyMapper<ISlider, AvaloniaSliderHandler> Mapper = new PropertyMapper<ISlider, AvaloniaSliderHandler>(ViewHandler.ViewMapper)
	{
		[nameof(IRange.Minimum)] = MapMinimum,
		[nameof(IRange.Maximum)] = MapMaximum,
		[nameof(IRange.Value)] = MapValue,
		[nameof(ISlider.MinimumTrackColor)] = MapMinimumTrackColor,
		[nameof(ISlider.MaximumTrackColor)] = MapMaximumTrackColor,
		[nameof(ISlider.ThumbColor)] = MapThumbColor
	};

	public AvaloniaSliderHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaSliderControl CreatePlatformView() =>
		new()
		{
			Minimum = 0,
			Maximum = 1,
			Value = 0
		};

	protected override void ConnectHandler(AvaloniaSliderControl platformView)
	{
		base.ConnectHandler(platformView);
		platformView.PropertyChanged += OnPropertyChanged;
		platformView.PointerPressed += OnPointerPressed;
		platformView.PointerReleased += OnPointerReleased;
		platformView.PointerCaptureLost += OnPointerCaptureLost;
	}

	protected override void DisconnectHandler(AvaloniaSliderControl platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.PropertyChanged -= OnPropertyChanged;
		platformView.PointerPressed -= OnPointerPressed;
		platformView.PointerReleased -= OnPointerReleased;
		platformView.PointerCaptureLost -= OnPointerCaptureLost;
	}

	static void MapMinimum(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Minimum = slider.Minimum;
		handler.PlatformView.Value = Math.Clamp(handler.PlatformView.Value, handler.PlatformView.Minimum, handler.PlatformView.Maximum);
	}

	static void MapMaximum(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Maximum = slider.Maximum;
		handler.PlatformView.Value = Math.Clamp(handler.PlatformView.Value, handler.PlatformView.Minimum, handler.PlatformView.Maximum);
	}

	static void MapValue(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Value = Math.Clamp(slider.Value, handler.PlatformView.Minimum, handler.PlatformView.Maximum);
	}

	static void MapMinimumTrackColor(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = slider.MinimumTrackColor.ToAvaloniaBrush();
	}

	static void MapMaximumTrackColor(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Background = slider.MaximumTrackColor.ToAvaloniaBrush();
	}

	static void MapThumbColor(AvaloniaSliderHandler handler, ISlider slider)
	{
		if (handler.PlatformView is null)
			return;

		// Avalonia's slider theme does not expose a direct thumb color brush. Use resources as a best-effort fallback.
		handler.PlatformView.Resources["SliderThumbFill"] = slider.ThumbColor.ToAvaloniaBrush();
	}

	void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (VirtualView is null || PlatformView is null)
			return;

		if (e.Property == RangeBase.ValueProperty)
		{
			var value = PlatformView.Value;
			if (!value.Equals(VirtualView.Value))
				VirtualView.Value = value;
		}
	}

	void OnPointerPressed(object? sender, PointerPressedEventArgs e) =>
		VirtualView?.DragStarted();

	void OnPointerReleased(object? sender, PointerReleasedEventArgs e) =>
		VirtualView?.DragCompleted();

	void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) =>
		VirtualView?.DragCompleted();
}
