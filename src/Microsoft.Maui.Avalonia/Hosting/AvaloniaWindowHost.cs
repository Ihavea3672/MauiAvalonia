using System;
using System.Collections.Generic;
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
using Microsoft.Maui.Handlers;
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
	readonly Dictionary<IWindow, WindowRegistration> _windowRegistrations = new();
	readonly object _registrationLock = new();

	AvaloniaApplication? _lifetimeOwner;
	IApplication? _mauiApplication;
	IMauiContext? _applicationContext;
	IServiceProvider? _services;
	IClassicDesktopStyleApplicationLifetime? _desktopLifetime;

	public void AttachLifetime(AvaloniaApplication lifetimeOwner, IApplication mauiApp, IMauiContext applicationContext)
	{
		_lifetimeOwner = lifetimeOwner ?? throw new ArgumentNullException(nameof(lifetimeOwner));
		_mauiApplication = mauiApp ?? throw new ArgumentNullException(nameof(mauiApp));
		_applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
		_services = applicationContext.Services;

		switch (lifetimeOwner.ApplicationLifetime)
		{
			case IClassicDesktopStyleApplicationLifetime desktop:
				desktop.Startup += OnDesktopStartup;
				desktop.Exit += OnDesktopExit;
				_desktopLifetime = desktop;
				EnsureMainWindow();
				break;

			case ISingleViewApplicationLifetime singleView:
				LifecycleInvoker.Invoke<AvaloniaLifecycle.OnStartup>(
					_services,
					del => del(lifetimeOwner, new ControlledApplicationLifetimeStartupEventArgs(Array.Empty<string>())));

				singleView.MainView ??= new TextBlock
				{
					Text = "Avalonia backend initialization is still in progress.",
					HorizontalAlignment = AvaloniaHorizontalAlignment.Center,
					VerticalAlignment = AvaloniaVerticalAlignment.Center
				};
				break;
		}
	}

	public void OpenWindow(IApplication application, OpenWindowRequest? request)
	{
		if (_desktopLifetime is null)
			return;

		if (!ReferenceEquals(_mauiApplication, application))
			_mauiApplication = application;

		InvokeOnUiThread(() =>
		{
			var registration = CreateWindowInternal(request);
			registration?.PlatformWindow.Show();
		});
	}

	void OnDesktopStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs args)
	{
		InvokeOnUiThread(() =>
		{
			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnStartup>(_services, del => del(_lifetimeOwner!, args));
			EnsureMainWindow();
		});
	}

	void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
	{
		LifecycleInvoker.Invoke<AvaloniaLifecycle.OnExit>(_services, del => del(_lifetimeOwner!, args));

		if (_desktopLifetime is null)
			return;

		foreach (var window in _desktopLifetime.Windows)
		{
			try
			{
				window.Close();
			}
			catch
			{
				// Ignore failures during shutdown – Avalonia will tear down remaining windows.
			}
		}
	}

	void EnsureMainWindow()
	{
		if (_desktopLifetime is null)
			return;

		var desktopLifetime = _desktopLifetime;
		InvokeOnUiThread(() =>
		{
			if (desktopLifetime.MainWindow is not null)
				return;

			var registration = CreateWindowInternal(request: null);
			if (registration is not null)
				desktopLifetime.MainWindow = registration.PlatformWindow;
		});
	}

	WindowRegistration? CreateWindowInternal(OpenWindowRequest? request)
	{
		if (_mauiApplication is null || _applicationContext is null)
			return null;

		var avaloniaWindow = new AvaloniaWindow
		{
			Width = 1024,
			Height = 768,
			Title = "MAUI on Avalonia (preview)"
		};

		var windowScope = _applicationContext.Services.CreateScope();
		try
		{
			var windowContext = new MauiContext(windowScope.ServiceProvider);
			MauiServiceUtilities.InitializeScopedServices(windowScope.ServiceProvider);

			var activationState = request?.State is IPersistedState persistedState
				? new ActivationState(windowContext, persistedState)
				: new ActivationState(windowContext);

			var window = _mauiApplication.CreateWindow(activationState);
			MauiContextAccessor.TryAddSpecific(windowContext, window);
			MauiContextAccessor.TryAddWeakSpecific(windowContext, avaloniaWindow);

			try
			{
				avaloniaWindow.SetWindowHandler(window, windowContext);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[AvaloniaWindowHost] Window handler wiring failed: {ex}");
				throw;
			}

			var navigationRoot = windowScope.ServiceProvider.GetService<IAvaloniaNavigationRoot>();
			navigationRoot?.Attach(avaloniaWindow);
			ApplyInitialContent(window, windowContext, navigationRoot);

			var registration = new WindowRegistration(window, avaloniaWindow, navigationRoot, windowScope);
			RegisterWindow(registration);
			AvaloniaDiagnosticsHelper.AttachIfEnabled(avaloniaWindow);
			return registration;
		}
		catch
		{
			windowScope.Dispose();
			throw;
		}
	}

	void RegisterWindow(WindowRegistration registration)
	{
		void OnOpened(object? sender, EventArgs e)
		{
			registration.PlatformWindow.Opened -= OnOpened;
			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnWindowCreated>(_services, del => del(_lifetimeOwner!, registration.PlatformWindow));
		}

		void OnClosed(object? sender, EventArgs e)
		{
			registration.PlatformWindow.Closed -= OnClosed;
			registration.PlatformWindow.Opened -= OnOpened;

			lock (_registrationLock)
				_windowRegistrations.Remove(registration.MauiWindow);

			LifecycleInvoker.Invoke<AvaloniaLifecycle.OnWindowDestroyed>(_services, del => del(_lifetimeOwner!, registration.PlatformWindow));
			registration.Dispose();
		}

		registration.PlatformWindow.Opened += OnOpened;
		registration.PlatformWindow.Closed += OnClosed;

		lock (_registrationLock)
			_windowRegistrations[registration.MauiWindow] = registration;
	}

	static void InvokeOnUiThread(Action callback)
	{
		if (AvaloniaUiDispatcher.UIThread.CheckAccess())
		{
			callback();
		}
		else
		{
			AvaloniaUiDispatcher.UIThread.Post(callback);
		}
	}

	sealed class WindowRegistration : IDisposable
	{
		public WindowRegistration(IWindow mauiWindow, AvaloniaWindow platformWindow, IAvaloniaNavigationRoot? navigationRoot, IServiceScope scope)
		{
			MauiWindow = mauiWindow ?? throw new ArgumentNullException(nameof(mauiWindow));
			PlatformWindow = platformWindow ?? throw new ArgumentNullException(nameof(platformWindow));
			NavigationRoot = navigationRoot;
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		public IWindow MauiWindow { get; }

		public AvaloniaWindow PlatformWindow { get; }

		public IAvaloniaNavigationRoot? NavigationRoot { get; }

		public IServiceScope Scope { get; }

		public void Dispose()
		{
			try
			{
				NavigationRoot?.Detach();
			}
			finally
			{
				Scope.Dispose();
			}
		}
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
