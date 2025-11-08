using System.Collections.Generic;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Graphics;
using GraphicsRect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Avalonia.Handlers;

public sealed class AvaloniaLayoutPanel : Panel, ICrossPlatformLayoutBacking
{
	readonly List<IView> _logicalChildren = new();

	public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

	public void UpdateChildren(IMauiContext context, IReadOnlyList<IView> children)
	{
		Children.Clear();
		_logicalChildren.Clear();

		foreach (var child in children)
		{
			var control = child.ToAvaloniaControl(context);
			if (control is null)
				continue;

			_logicalChildren.Add(child);
			Children.Add(control);
		}
	}

	protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize)
	{
		if (CrossPlatformLayout is null)
			return base.MeasureOverride(availableSize);

		var desired = CrossPlatformLayout.CrossPlatformMeasure(availableSize.Width, availableSize.Height);
		foreach (var child in Children)
			child.Measure(availableSize);

		return desired.ToAvalonia();
	}

	protected override global::Avalonia.Size ArrangeOverride(global::Avalonia.Size finalSize)
	{
		if (CrossPlatformLayout is null)
			return base.ArrangeOverride(finalSize);

		var arranged = CrossPlatformLayout.CrossPlatformArrange(new GraphicsRect(0, 0, finalSize.Width, finalSize.Height));

		for (var index = 0; index < _logicalChildren.Count && index < Children.Count; index++)
		{
			var view = _logicalChildren[index];
			var control = Children[index];

			var frame = view.Frame;
			if (frame.Width <= 0 || frame.Height <= 0)
				continue;

			control.Arrange(frame.ToAvalonia());
		}

		return arranged.ToAvalonia();
	}
}
