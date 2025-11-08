using System;
using System.Globalization;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Fonts;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Avalonia.Platform;
using Microsoft.Maui.Handlers;
using AvaloniaDatePicker = Avalonia.Controls.DatePicker;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaDatePickerHandler : AvaloniaViewHandler<IDatePicker, AvaloniaDatePicker>, IDatePickerHandler
{
	public static IPropertyMapper<IDatePicker, AvaloniaDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, AvaloniaDatePickerHandler>(ViewHandler.ViewMapper)
	{
		[nameof(IDatePicker.Date)] = MapDate,
		[nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
		[nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
		[nameof(IDatePicker.Format)] = MapFormat,
		[nameof(ITextStyle.TextColor)] = MapTextColor,
		[nameof(ITextStyle.Font)] = MapFont
	};

	public AvaloniaDatePickerHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaDatePicker CreatePlatformView() => new();

	protected override void ConnectHandler(AvaloniaDatePicker platformView)
	{
		base.ConnectHandler(platformView);
		platformView.SelectedDateChanged += OnSelectedDateChanged;
	}

	protected override void DisconnectHandler(AvaloniaDatePicker platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.SelectedDateChanged -= OnSelectedDateChanged;
	}

	static void MapDate(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.SelectedDate = ((DateTime?)picker.Date).ToDateTimeOffset();
		EnsureSelectedDateWithinBounds(handler.PlatformView);
	}

	static void MapMinimumDate(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		var min = picker.MinimumDate;
		handler.PlatformView.MinYear = ((DateTime?)min).ToDateTimeOffset() ?? DateTimeOffset.MinValue;
		EnsureSelectedDateWithinBounds(handler.PlatformView);
	}

	static void MapMaximumDate(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		var max = picker.MaximumDate;
		handler.PlatformView.MaxYear = ((DateTime?)max).ToDateTimeOffset() ?? DateTimeOffset.MaxValue;
		EnsureSelectedDateWithinBounds(handler.PlatformView);
	}

	static void MapFormat(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		var format = string.IsNullOrWhiteSpace(picker.Format)
			? CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
			: picker.Format;

		handler.PlatformView.DayFormat = ExtractFormat(format, 'd', "%d");
		handler.PlatformView.MonthFormat = ExtractFormat(format, 'M', "MMM");
		handler.PlatformView.YearFormat = ExtractFormat(format, 'y', "yyyy");

		var lowered = format.ToLowerInvariant();
		handler.PlatformView.DayVisible = lowered.Contains('d');
		handler.PlatformView.MonthVisible = lowered.Contains('m');
		handler.PlatformView.YearVisible = lowered.Contains('y');
	}

	static void MapTextColor(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = picker.TextColor.ToAvaloniaBrush();
	}

	static void MapFont(AvaloniaDatePickerHandler handler, IDatePicker picker)
	{
		if (handler.PlatformView is null)
			return;

		var fontManager = handler.GetRequiredService<IAvaloniaFontManager>();
		handler.PlatformView.UpdateFont(picker, fontManager);
	}

	void OnSelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
	{
		if (VirtualView is null)
			return;

		var newDate = e.NewDate.ToDateTime();
		if (newDate.HasValue && VirtualView.Date != newDate.Value)
			VirtualView.Date = newDate.Value;
	}

	static string ExtractFormat(string format, char token, string fallback)
	{
		var index = format.IndexOf(token, StringComparison.OrdinalIgnoreCase);
		if (index < 0)
			return fallback;

		var count = 0;
		while (index + count < format.Length && char.ToLowerInvariant(format[index + count]) == char.ToLowerInvariant(token))
			count++;

		if (count == 0)
			return fallback;

		var formatToken = new string(char.ToLowerInvariant(token), count);
		if (token == 'd' || token == 'D')
			return count == 1 ? "%d" : formatToken;

		return formatToken;
	}

	static void EnsureSelectedDateWithinBounds(AvaloniaDatePicker picker)
	{
		if (picker.SelectedDate is null)
			return;

		if (picker.SelectedDate < picker.MinYear)
			picker.SelectedDate = picker.MinYear;
		else if (picker.SelectedDate > picker.MaxYear)
			picker.SelectedDate = picker.MaxYear;
	}
}
