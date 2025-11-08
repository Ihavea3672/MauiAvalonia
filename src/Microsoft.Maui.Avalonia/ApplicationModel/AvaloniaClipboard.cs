using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using AvaloniaClipboardService = Avalonia.Input.Platform.IClipboard;
using MauiClipboard = Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard;

namespace Microsoft.Maui.Avalonia.ApplicationModel;

internal sealed class AvaloniaClipboard : MauiClipboard
{
	readonly AvaloniaUiDispatcher _dispatcher = AvaloniaUiDispatcher.UIThread;

	public event EventHandler<EventArgs>? ClipboardContentChanged;

	public bool HasText => InvokeOnUi(() =>
	{
		var clipboard = GetClipboard();
		if (clipboard == null)
			return false;

		var text = clipboard.GetTextAsync().GetAwaiter().GetResult();
		return !string.IsNullOrEmpty(text);
	});

	public Task SetTextAsync(string? text) =>
		InvokeOnUiAsync(async () =>
		{
			var clipboard = GetClipboard();
			if (clipboard == null)
				return;

			await clipboard.SetTextAsync(text ?? string.Empty);
			ClipboardContentChanged?.Invoke(this, EventArgs.Empty);
		});

	public Task<string?> GetTextAsync() =>
		InvokeOnUiAsync(async () =>
		{
			var clipboard = GetClipboard();
			if (clipboard == null)
				return null;

			return await clipboard.GetTextAsync();
		});

	static TopLevel? GetActiveTopLevel()
	{
		if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
		{
			return desktopLifetime.Windows.FirstOrDefault(w => w.IsActive) ??
				desktopLifetime.Windows.FirstOrDefault() ??
				desktopLifetime.MainWindow;
		}

		if (global::Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
			return TopLevel.GetTopLevel(singleViewLifetime.MainView);

		return null;
	}

	AvaloniaClipboardService? GetClipboard() => GetActiveTopLevel()?.Clipboard;

	T InvokeOnUi<T>(Func<T> action)
	{
		if (_dispatcher.CheckAccess())
			return action();

		return _dispatcher.InvokeAsync(action).GetAwaiter().GetResult();
	}

	Task InvokeOnUiAsync(Func<Task> action)
	{
		if (_dispatcher.CheckAccess())
			return action();

		return _dispatcher.InvokeAsync(action);
	}

	Task<T> InvokeOnUiAsync<T>(Func<Task<T>> action)
	{
		if (_dispatcher.CheckAccess())
			return action();

		return _dispatcher.InvokeAsync(action);
	}

	public static void TryRegisterAsDefault(MauiClipboard implementation)
	{
		var method = typeof(Clipboard).GetMethod("SetDefault", BindingFlags.Static | BindingFlags.NonPublic);
		method?.Invoke(null, new object?[] { implementation });
	}
}
