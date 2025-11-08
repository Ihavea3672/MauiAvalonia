using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Avalonia.Dispatching;

internal sealed class AvaloniaDispatcherProvider : IDispatcherProvider
{
	private readonly ConditionalWeakTable<global::Avalonia.Threading.Dispatcher, AvaloniaDispatcher> _dispatchers = new();

	public IDispatcher? GetForCurrentThread()
	{
		var dispatcher = global::Avalonia.Threading.Dispatcher.UIThread;
		return _dispatchers.GetValue(dispatcher, static d => new AvaloniaDispatcher(d));
	}
}
