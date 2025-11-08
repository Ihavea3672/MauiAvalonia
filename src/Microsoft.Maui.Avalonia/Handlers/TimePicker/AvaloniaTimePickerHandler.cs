using System;
using System.Globalization;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Fonts;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Avalonia.Platform;
using Microsoft.Maui.Handlers;
using AvaloniaTimePickerControl = Avalonia.Controls.TimePicker;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaTimePickerHandler : AvaloniaViewHandler<ITimePicker, AvaloniaTimePickerControl>, ITimePickerHandler
{
	public static IPropertyMapper<ITimePicker, AvaloniaTimePickerHandler> Mapper = new PropertyMapper<ITimePicker, AvaloniaTimePickerHandler>(ViewHandler.ViewMapper)
	{
		[nameof(ITimePicker.Time)] = MapTime,
		[nameof(ITimePicker.Format)] = MapFormat,
		[nameof(ITextStyle.TextColor)] = MapTextColor,
		[nameof(ITextStyle.Font)] = MapFont
	};

	public AvaloniaTimePickerHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaTimePickerControl CreatePlatformView() => new();

	protected override void ConnectHandler(AvaloniaTimePickerControl platformView)
	{
		base.ConnectHandler(platformView);
		platformView.SelectedTimeChanged += OnSelectedTimeChanged;
	}

	protected override void DisconnectHandler(AvaloniaTimePickerControl platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.SelectedTimeChanged -= OnSelectedTimeChanged;
	}

	static void MapTime(AvaloniaTimePickerHandler handler, ITimePicker timePicker)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.SelectedTime = timePicker.Time;
	}

	static void MapFormat(AvaloniaTimePickerHandler handler, ITimePicker timePicker)
	{
		if (handler.PlatformView is null)
			return;

		var format = string.IsNullOrWhiteSpace(timePicker.Format)
			? CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
			: timePicker.Format;

		var is24Hour = format.IndexOf('H') >= 0;
		handler.PlatformView.ClockIdentifier = is24Hour ? "24HourClock" : "12HourClock";
	}

	static void MapTextColor(AvaloniaTimePickerHandler handler, ITimePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = picker.TextColor.ToAvaloniaBrush();
	}

	static void MapFont(AvaloniaTimePickerHandler handler, ITimePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		var fontManager = handler.GetRequiredService<IAvaloniaFontManager>();
		handler.PlatformView.UpdateFont(picker, fontManager);
	}

	void OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
	{
		if (VirtualView is null)
			return;

		if (e.NewTime is TimeSpan newTime && VirtualView.Time != newTime)
			VirtualView.Time = newTime;
	}
}
