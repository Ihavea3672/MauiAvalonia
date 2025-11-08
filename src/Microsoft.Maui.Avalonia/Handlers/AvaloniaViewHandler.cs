using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Avalonia.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public abstract class AvaloniaViewHandler<TView, TPlatformView> : ViewHandler<TView, TPlatformView>
	where TView : class, IView
	where TPlatformView : global::Avalonia.Controls.Control
{
	AvaloniaInputAdapter? _inputAdapter;

	protected AvaloniaViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
		: base(mapper, commandMapper)
	{
		AvaloniaViewHandlerMapper.EnsureInitialized();
	}

	public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
		if (PlatformView is null)
			return Size.Zero;

		var width = double.IsInfinity(widthConstraint) ? double.PositiveInfinity : Math.Max(0, widthConstraint);
		var height = double.IsInfinity(heightConstraint) ? double.PositiveInfinity : Math.Max(0, heightConstraint);
		var available = new global::Avalonia.Size(width, height);

		PlatformView.Measure(available);
		var desired = PlatformView.DesiredSize;
		return new Size(desired.Width, desired.Height);
	}

	public override void PlatformArrange(Rect frame)
	{
		if (PlatformView is null)
			return;

		var rect = new global::Avalonia.Rect(frame.X, frame.Y, frame.Width, frame.Height);
		PlatformView.Arrange(rect);
		PlatformView.InvalidateVisual();
		this.Invoke(nameof(IView.Frame), frame);
	}

	protected override void SetupContainer()
	{
	}

	protected override void RemoveContainer()
	{
	}

	protected override void ConnectHandler(TPlatformView platformView)
	{
		base.ConnectHandler(platformView);
		_inputAdapter = AvaloniaInputAdapter.Attach(VirtualView, platformView);
	}

	protected override void DisconnectHandler(TPlatformView platformView)
	{
		_inputAdapter?.Dispose();
		_inputAdapter = null;
		base.DisconnectHandler(platformView);
	}

	protected TService GetRequiredService<TService>()
		where TService : notnull
	{
		var services = MauiContext?.Services
			?? throw new InvalidOperationException("MAUI context is not available.");

		return services.GetRequiredService<TService>();
	}
}
