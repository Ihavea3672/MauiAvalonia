using System;

namespace Microsoft.Maui.LifecycleEvents;

/// <summary>
/// Adds support for registering Avalonia lifecycle events via <see cref="ILifecycleBuilder"/>.
/// </summary>
public static class AvaloniaLifecycleExtensions
{
	/// <summary>
	/// Registers Avalonia lifecycle callbacks.
	/// </summary>
	public static ILifecycleBuilder AddAvalonia(this ILifecycleBuilder builder, Action<IAvaloniaLifecycleBuilder>? configureDelegate)
	{
		var lifecycle = new LifecycleBuilder(builder);
		configureDelegate?.Invoke(lifecycle);
		return builder;
	}

	private sealed class LifecycleBuilder : IAvaloniaLifecycleBuilder
	{
		private readonly ILifecycleBuilder _builder;

		public LifecycleBuilder(ILifecycleBuilder builder)
		{
			_builder = builder;
		}

		public void AddEvent<TDelegate>(string eventName, TDelegate action) where TDelegate : Delegate =>
			_builder.AddEvent(eventName, action);
	}
}
