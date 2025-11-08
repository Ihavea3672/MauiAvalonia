using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Avalonia.Internal;

internal static class MauiServiceUtilities
{
	public static void InitializeAppServices(MauiApp mauiApp)
	{
		var initServices = mauiApp.Services.GetServices<IMauiInitializeService>();
		if (initServices is null)
			return;

		foreach (var service in initServices)
			service.Initialize(mauiApp.Services);
	}

	public static void InitializeScopedServices(IServiceProvider services)
	{
		var scopedServices = services.GetServices<IMauiInitializeScopedService>();
		if (scopedServices is null)
			return;

		foreach (var service in scopedServices)
			service.Initialize(services);
	}
}
