using System;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Graphics;
using Microsoft.Maui.Handlers;
using AvaloniaProgressBar = Avalonia.Controls.ProgressBar;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaProgressBarHandler : AvaloniaViewHandler<IProgress, AvaloniaProgressBar>, IProgressBarHandler
{
	public static PropertyMapper<IProgress, AvaloniaProgressBarHandler> Mapper = new(ViewMapper)
	{
		[nameof(IProgress.Progress)] = MapProgress,
		[nameof(IProgress.ProgressColor)] = MapProgressColor
	};

	public AvaloniaProgressBarHandler()
		: base(Mapper)
	{
	}

	protected override AvaloniaProgressBar CreatePlatformView() =>
		new()
		{
			Minimum = 0,
			Maximum = 1
		};

	static void MapProgress(AvaloniaProgressBarHandler handler, IProgress progress)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Value = Math.Clamp(progress.Progress, handler.PlatformView.Minimum, handler.PlatformView.Maximum);
	}

	static void MapProgressColor(AvaloniaProgressBarHandler handler, IProgress progress)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Foreground = progress.ProgressColor.ToAvaloniaBrush();
	}
}
