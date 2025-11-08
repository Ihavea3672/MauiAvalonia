using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AvaloniaImage = Avalonia.Controls.Image;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaImageHandler : AvaloniaViewHandler<IImage, AvaloniaImage>, IImageHandler
{
	public static IPropertyMapper<IImage, AvaloniaImageHandler> Mapper = new PropertyMapper<IImage, AvaloniaImageHandler>(ViewHandler.ViewMapper)
	{
		[nameof(IImage.Aspect)] = MapAspect,
		[nameof(IImage.Source)] = MapSource,
		[nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying
	};

	static readonly CommandMapper<IImage, AvaloniaImageHandler> CommandMapper = new(ViewCommandMapper);

	ImageSourcePartLoader? _sourceLoader;
	CancellationTokenSource? _loadingCts;
	Bitmap? _currentBitmap;

	public AvaloniaImageHandler()
		: base(Mapper, CommandMapper)
	{
	}

	protected override AvaloniaImage CreatePlatformView() =>
		new()
		{
			Stretch = Aspect.Fill.ToAvalonia(),
			StretchDirection = global::Avalonia.Media.StretchDirection.Both
		};

	protected override void DisconnectHandler(AvaloniaImage platformView)
	{
		base.DisconnectHandler(platformView);
		CancelLoading();
		_ = SetPlatformImageAsync(null);
	}

	static void MapAspect(AvaloniaImageHandler handler, IImage image)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Stretch = image.Aspect.ToAvalonia();
	}

	static void MapIsAnimationPlaying(AvaloniaImageHandler handler, IImage image)
	{
		// Avalonia native image control does not yet support animated sources.
	}

	static void MapSource(AvaloniaImageHandler handler, IImage image)
	{
		_ = handler.UpdateImageSourceAsync();
	}

	async Task UpdateImageSourceAsync()
	{
		CancelLoading();

		if (MauiContext is null || PlatformView is null)
		{
			await SetPlatformImageAsync(null).ConfigureAwait(false);
			return;
		}

		var imageSource = VirtualView?.Source;
		if (imageSource is null)
		{
			await SetPlatformImageAsync(null).ConfigureAwait(false);
			VirtualView?.UpdateIsLoading(false);
			return;
		}

		var events = VirtualView as IImageSourcePartEvents;
		_loadingCts = new CancellationTokenSource();
		var token = _loadingCts.Token;

		try
		{
			events?.LoadingStarted();
			VirtualView?.UpdateIsLoading(true);

			var bitmap = await AvaloniaImageSourceLoader.LoadAsync(imageSource, MauiContext.Services, token).ConfigureAwait(false);

			if (token.IsCancellationRequested || !ReferenceEquals(imageSource, VirtualView?.Source))
			{
				bitmap?.Dispose();
				return;
			}

			await SetPlatformImageAsync(bitmap).ConfigureAwait(false);
			events?.LoadingCompleted(bitmap is not null);
		}
		catch (OperationCanceledException)
		{
			// no-op
		}
		catch (Exception ex)
		{
			await SetPlatformImageAsync(null).ConfigureAwait(false);
			events?.LoadingFailed(ex);
		}
		finally
		{
			if (!token.IsCancellationRequested && ReferenceEquals(imageSource, VirtualView?.Source))
				VirtualView?.UpdateIsLoading(false);
		}
	}

	async Task SetPlatformImageAsync(Bitmap? bitmap)
	{
		if (PlatformView is null)
		{
			bitmap?.Dispose();
			return;
		}

		await AvaloniaUiDispatcher.UIThread.InvokeAsync(() =>
		{
			var previous = _currentBitmap;
			_currentBitmap = bitmap;
			PlatformView.Source = bitmap;
			previous?.Dispose();
		});
	}

	void CancelLoading()
	{
		try
		{
			_loadingCts?.Cancel();
		}
		catch (ObjectDisposedException)
		{
		}
		finally
		{
			_loadingCts?.Dispose();
			_loadingCts = null;
		}
	}

	public ImageSourcePartLoader SourceLoader =>
		_sourceLoader ??= new ImageSourcePartLoader(new ImageSourceSetter(this));

	sealed class ImageSourceSetter : IImageSourcePartSetter
	{
		readonly AvaloniaImageHandler _handler;

		public ImageSourceSetter(AvaloniaImageHandler handler) =>
			_handler = handler;

		public IElementHandler? Handler => _handler;

		public IImageSourcePart? ImageSourcePart => _handler.VirtualView;

		public void SetImageSource(object? platformImage)
		{
			// ImageSourcePartLoader is not used on the Avalonia backend yet.
		}
	}
}
