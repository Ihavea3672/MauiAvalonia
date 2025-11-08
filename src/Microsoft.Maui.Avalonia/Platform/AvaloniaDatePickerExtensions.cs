using System;

namespace Microsoft.Maui.Avalonia.Platform;

internal static class AvaloniaDatePickerExtensions
{
	public static DateTimeOffset? ToDateTimeOffset(this DateTime? value)
	{
		if (value is null)
			return null;

		return value.Value.Kind switch
		{
			DateTimeKind.Utc => new DateTimeOffset(value.Value, TimeSpan.Zero),
			DateTimeKind.Unspecified => new DateTimeOffset(value.Value, DateTimeOffset.Now.Offset),
			_ => new DateTimeOffset(value.Value)
		};
	}

	public static DateTime? ToDateTime(this DateTimeOffset? value) =>
		value?.DateTime;
}
