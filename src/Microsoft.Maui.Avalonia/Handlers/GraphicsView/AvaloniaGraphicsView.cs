using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;
using SkiaSharp;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaGraphicsView : Control
{
	readonly SkiaCanvas _skiaCanvas = new();
	readonly ScalingCanvas _scalingCanvas;
	readonly Dictionary<int, PointF> _activePointers = new();
	IGraphicsView? _graphicsView;
	IDrawable? _drawable;

	public AvaloniaGraphicsView()
	{
		ClipToBounds = true;
		_scalingCanvas = new ScalingCanvas(_skiaCanvas);
	}

	public void Connect(IGraphicsView graphicsView)
	{
		_graphicsView = graphicsView;
		UpdateDrawable(graphicsView.Drawable);
	}

	public void Disconnect()
	{
		_graphicsView = null;
		_activePointers.Clear();
		_drawable = null;
	}

	public void UpdateDrawable(IDrawable? drawable)
	{
		_drawable = drawable;
		InvalidateDrawable();
	}

	public void InvalidateDrawable()
	{
		if (AvaloniaUiDispatcher.UIThread.CheckAccess())
			InvalidateVisual();
		else
			AvaloniaUiDispatcher.UIThread.Post(InvalidateVisual);
	}

	public override void Render(DrawingContext context)
	{
		base.Render(context);

		if (_drawable is null)
			return;

		context.Custom(new SkiaDrawOperation(this));
	}

	void RenderDrawable(ImmediateDrawingContext context, global::Avalonia.Rect bounds)
	{
		var drawable = _drawable;
		if (drawable is null)
			return;

		var leaseFeature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) as ISkiaSharpApiLeaseFeature;
		if (leaseFeature is null)
			return;

		using var lease = leaseFeature.Lease();
		var skCanvas = lease?.SkCanvas;
		if (skCanvas is null)
			return;

		var width = (float)bounds.Width;
		var height = (float)bounds.Height;
		var scale = (float)(VisualRoot?.RenderScaling ?? 1d);
		if (scale <= 0)
			scale = 1;

		_skiaCanvas.Canvas = skCanvas;
		_skiaCanvas.SetDisplayScale(scale);

		var drawRect = new RectF(0, 0, width / scale, height / scale);

		skCanvas.Save();
		try
		{
			skCanvas.ClipRect(new SKRect(0, 0, width, height));
			skCanvas.Clear();

			_scalingCanvas.ResetState();
			_scalingCanvas.Scale(scale, scale);

			drawable.Draw(_scalingCanvas, drawRect);
		}
		finally
		{
			skCanvas.Restore();
		}
	}

	protected override void OnPointerEntered(global::Avalonia.Input.PointerEventArgs e)
	{
		base.OnPointerEntered(e);

		if (!CanProcessHover())
			return;

		var point = ToPointF(e.GetPosition(this));
		_graphicsView?.StartHoverInteraction(new[] { point });
	}

	protected override void OnPointerExited(global::Avalonia.Input.PointerEventArgs e)
	{
		base.OnPointerExited(e);

		if (!CanProcessHover())
			return;

		_graphicsView?.EndHoverInteraction();
	}

	protected override void OnPointerMoved(global::Avalonia.Input.PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		var point = ToPointF(e.GetPosition(this));
		if (_activePointers.ContainsKey(e.Pointer.Id))
		{
			if (!IsInteractionsEnabled(e))
				return;

			_activePointers[e.Pointer.Id] = point;
			var points = GetActivePointsSnapshot();
			_graphicsView?.DragInteraction(points);
			e.Handled = true;
		}
		else if (CanProcessHover())
		{
			_graphicsView?.MoveHoverInteraction(new[] { point });
		}
	}

	protected override void OnPointerPressed(global::Avalonia.Input.PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if (!IsInteractionsEnabled(e))
			return;

		var point = ToPointF(e.GetPosition(this));
		_activePointers[e.Pointer.Id] = point;
		e.Pointer.Capture(this);

		var points = GetActivePointsSnapshot();
		_graphicsView?.StartInteraction(points);
		e.Handled = true;
	}

	protected override void OnPointerReleased(global::Avalonia.Input.PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);

		if (!_activePointers.Remove(e.Pointer.Id))
			return;

		var point = ToPointF(e.GetPosition(this));
		e.Pointer.Capture(null);
		var contained = ContainsPoint(point);

		_graphicsView?.EndInteraction(new[] { point }, contained);
		e.Handled = true;
	}

	protected override void OnPointerCaptureLost(global::Avalonia.Input.PointerCaptureLostEventArgs e)
	{
		base.OnPointerCaptureLost(e);

		if (_activePointers.Count == 0)
			return;

		_activePointers.Clear();
		_graphicsView?.CancelInteraction();
	}

	bool CanProcessHover() =>
		_graphicsView is { IsEnabled: true };

	bool IsInteractionsEnabled(global::Avalonia.Input.PointerEventArgs e)
	{
		if (_graphicsView is not { IsEnabled: true })
			return false;

		if (e.Pointer.Type == PointerType.Mouse)
		{
			var props = e.GetCurrentPoint(this).Properties;
			return props.IsLeftButtonPressed;
		}

		return true;
	}

	PointF[] GetActivePointsSnapshot()
	{
		var snapshot = new PointF[_activePointers.Count];
		_activePointers.Values.CopyTo(snapshot, 0);
		return snapshot;
	}

	static PointF ToPointF(global::Avalonia.Point point) =>
		new((float)point.X, (float)point.Y);

	bool ContainsPoint(PointF point) =>
		point.X >= 0 &&
		point.Y >= 0 &&
		point.X <= Bounds.Width &&
		point.Y <= Bounds.Height;

	sealed class SkiaDrawOperation : ICustomDrawOperation
	{
		readonly AvaloniaGraphicsView _owner;
		readonly global::Avalonia.Rect _bounds;

		public SkiaDrawOperation(AvaloniaGraphicsView owner)
		{
			_owner = owner;
			_bounds = new global::Avalonia.Rect(owner.Bounds.Size);
		}

		public global::Avalonia.Rect Bounds => _bounds;

		public void Dispose()
		{
		}

		public bool HitTest(global::Avalonia.Point p) => _bounds.Contains(p);

		public void Render(ImmediateDrawingContext context) =>
			_owner.RenderDrawable(context, _bounds);

		public bool Equals(ICustomDrawOperation? other) =>
			other is SkiaDrawOperation operation && ReferenceEquals(operation._owner, _owner);
	}
}
