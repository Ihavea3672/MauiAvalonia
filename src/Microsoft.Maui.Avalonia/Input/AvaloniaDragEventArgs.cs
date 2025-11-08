using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Avalonia.Input;

internal sealed class AvaloniaDragEventArgs : DragEventArgs
{
	readonly Func<IElement?, Point?>? _positionProvider;

	public AvaloniaDragEventArgs(DataPackage dataPackage, Func<IElement?, Point?>? positionProvider)
		: base(dataPackage)
	{
		_positionProvider = positionProvider;
	}

	public override Point? GetPosition(Element? relativeTo) =>
		_positionProvider?.Invoke(relativeTo);
}

internal sealed class AvaloniaDropEventArgs : DropEventArgs
{
	readonly Func<IElement?, Point?>? _positionProvider;

	public AvaloniaDropEventArgs(DataPackageView view, Func<IElement?, Point?>? positionProvider)
		: base(view)
	{
		_positionProvider = positionProvider;
	}

	public override Point? GetPosition(Element? relativeTo) =>
		_positionProvider?.Invoke(relativeTo);
}
