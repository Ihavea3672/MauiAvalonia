using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Avalonia.Internal;

internal static class LifecycleInvoker
{
	public static void Invoke<TDelegate>(IServiceProvider? services, Action<TDelegate> invoker)
		where TDelegate : Delegate
	{
		if (services is null)
			return;

		var lifecycle = services.GetService<ILifecycleEventService>();
		if (lifecycle is null)
			return;

		var eventName = typeof(TDelegate).Name;
		foreach (var handler in lifecycle.GetEventDelegates<TDelegate>(eventName))
			invoker(handler);
	}
}
