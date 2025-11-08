using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaGraphicsViewHandler : AvaloniaViewHandler<IGraphicsView, AvaloniaGraphicsView>
{
	public static IPropertyMapper<IGraphicsView, AvaloniaGraphicsViewHandler> Mapper = new PropertyMapper<IGraphicsView, AvaloniaGraphicsViewHandler>(ViewHandler.ViewMapper)
	{
		[nameof(IView.FlowDirection)] = MapFlowDirection,
		[nameof(IGraphicsView.Drawable)] = MapDrawable
	};

	public static CommandMapper<IGraphicsView, AvaloniaGraphicsViewHandler> CommandMapper = new(ViewCommandMapper)
	{
		[nameof(IGraphicsView.Invalidate)] = MapInvalidate
	};

	public AvaloniaGraphicsViewHandler()
		: base(Mapper, CommandMapper)
	{
	}

	protected override AvaloniaGraphicsView CreatePlatformView() => new();

	protected override void ConnectHandler(AvaloniaGraphicsView platformView)
	{
		base.ConnectHandler(platformView);
		if (VirtualView is not null)
			platformView.Connect(VirtualView);
	}

	protected override void DisconnectHandler(AvaloniaGraphicsView platformView)
	{
		platformView.Disconnect();
		base.DisconnectHandler(platformView);
	}

	public static void MapDrawable(AvaloniaGraphicsViewHandler handler, IGraphicsView graphicsView)
	{
		handler.PlatformView?.UpdateDrawable(graphicsView.Drawable);
	}

	public static void MapFlowDirection(AvaloniaGraphicsViewHandler handler, IGraphicsView graphicsView)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.FlowDirection = graphicsView.FlowDirection switch
		{
			FlowDirection.RightToLeft => global::Avalonia.Media.FlowDirection.RightToLeft,
			_ => global::Avalonia.Media.FlowDirection.LeftToRight
		};
	}

	public static void MapInvalidate(AvaloniaGraphicsViewHandler handler, IGraphicsView graphicsView, object? _)
	{
		handler.PlatformView?.InvalidateDrawable();
	}
}
