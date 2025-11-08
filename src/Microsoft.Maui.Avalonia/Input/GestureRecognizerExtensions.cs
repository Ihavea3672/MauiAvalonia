using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Avalonia.Input;

internal static class GestureRecognizerExtensions
{
	public static IEnumerable<T> GetGesturesFor<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null)
		where T : class, IGestureRecognizer
	{
		if (gestures is null)
			yield break;

		foreach (var gesture in gestures.OfType<T>())
		{
			if (predicate is null || predicate(gesture))
				yield return gesture;
		}
	}
}
