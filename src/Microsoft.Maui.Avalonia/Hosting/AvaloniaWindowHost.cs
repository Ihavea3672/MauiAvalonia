using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Handlers;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Avalonia.Navigation;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using AvaloniaApplication = Avalonia.Application;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace Microsoft.Maui.Hosting;

/// <summary>
/// Window host that wires the Avalonia desktop lifetime into MAUI. It creates an Avalonia
/// window for each MAUI <see cref="IWindow"/> and keeps the lifecycle events in sync.
/// </summary>
internal sealed class AvaloniaWindowHost : IAvaloniaWindowHost
{
	public void AttachLifetime(AvaloniaApplication lifetimeOwner, IApplication mauiApp, IMauiContext applicationContext)
	{
		var services = applicationContext.Services;

		if (lifetimeOwner.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.Startup += (sender, args) =>
			{
				LifecycleInvoker.Invoke<AvaloniaLifecycle.OnStartup>(services, del => del(lifetimeOwner, args));
				desktop.MainWindow ??= CreatePlaceholderWindow(lifetimeOwner, mauiApp, applicationContext, services);
			};

			desktop.Exit += (sender, args) =>
			{
				LifecycleInvoker.Invoke<AvaloniaLifecycle.OnExit>(services, del => del(lifetimeOwner, args));
				foreach (var window in desktop.Windows)
					window.Close();
			};

			if (desktop.MainWindow is null)
				desktop.MainWindow = CreatePlaceholderWindow(lifetimeOwner, mauiApp, applicationContext, services);
		}
		else if (lifetimeOwner.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
		{
			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnStartup>(
				services,
				del => del(lifetimeOwner, new ControlledApplicationLifetimeStartupEventArgs(Array.Empty<string>())));

			singleView.MainView ??= new TextBlock
			{
				Text = "Avalonia backend initialization is still in progress.",
				HorizontalAlignment = AvaloniaHorizontalAlignment.Center,
				VerticalAlignment = AvaloniaVerticalAlignment.Center
			};
		}
	}

	private static AvaloniaWindow CreatePlaceholderWindow(AvaloniaApplication lifetimeOwner, IApplication mauiApp, IMauiContext applicationContext, IServiceProvider services)
	{
		var avaloniaWindow = new AvaloniaWindow
		{
			Width = 1024,
			Height = 768,
			Title = "MAUI on Avalonia (preview)"
		};

		var windowScope = applicationContext.Services.CreateScope();
		var windowContext = new MauiContext(windowScope.ServiceProvider);
		MauiServiceUtilities.InitializeScopedServices(windowScope.ServiceProvider);

		var activationState = new ActivationState(windowContext);
		var window = mauiApp.CreateWindow(activationState);
		MauiContextAccessor.TryAddSpecific(windowContext, window);
		MauiContextAccessor.TryAddWeakSpecific(windowContext, avaloniaWindow);

		try
		{
			avaloniaWindow.SetWindowHandler(window, windowContext);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[AvaloniaWindowHost] Window handler wiring failed: {ex}");
		}

		var navigationRoot = windowScope.ServiceProvider.GetService<IAvaloniaNavigationRoot>();
		navigationRoot?.Attach(avaloniaWindow);
		ApplyInitialContent(window, windowContext, navigationRoot);

		avaloniaWindow.Opened += (_, __) =>
			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnWindowCreated>(services, del => del(lifetimeOwner, avaloniaWindow));

		avaloniaWindow.Closed += (_, __) =>
		{
			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnWindowDestroyed>(services, del => del(lifetimeOwner, avaloniaWindow));
			navigationRoot?.Detach();
			windowScope.Dispose();
		};

		AvaloniaDiagnosticsHelper.AttachIfEnabled(avaloniaWindow);

		return avaloniaWindow;
	}

	static void ApplyInitialContent(IWindow window, IMauiContext windowContext, IAvaloniaNavigationRoot? navigationRoot)
	{
		if (navigationRoot is null)
			return;

		if (window.Content is IView view)
		{
			try
			{
				var control = view.ToAvaloniaControl(windowContext);
				navigationRoot.SetContent(control);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[AvaloniaWindowHost] Failed to materialize MAUI content: {ex}");
				navigationRoot.SetPlaceholder("Avalonia backend: failed to render the MAUI window content.");
			}
		}
		else
		{
			navigationRoot.SetPlaceholder("Avalonia backend: waiting for MAUI to provide window content.");
		}
	}
}
