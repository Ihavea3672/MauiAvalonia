using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using MauiAvalonia.SampleApp;
using Xunit;

namespace Microsoft.Maui.Avalonia.Tests;

public sealed class HeadlessIntegrationTests
{
	[Fact]
	public async Task AvaloniaHostBootstrapsMauiServices()
	{
		await AvaloniaHeadlessFixture.Session.Dispatch(() =>
		{
			var platformApplication = IPlatformApplication.Current;
			Assert.NotNull(platformApplication);

			var services = platformApplication.Services;
			Assert.NotNull(services);

			var application = services.GetService<IApplication>();
			Assert.NotNull(application);
			Assert.IsType<App>(application);

			var windowHost = services.GetService<IAvaloniaWindowHost>();
			Assert.NotNull(windowHost);
		}, CancellationToken.None);
	}
}

public sealed class AvaloniaHeadlessFixture : IDisposable
{
	public static HeadlessUnitTestSession Session { get; } =
		HeadlessUnitTestSession.StartNew(typeof(AvaloniaHostHeadlessEntryPoint));

	public AvaloniaHeadlessFixture()
	{
	}

	public void Dispose()
	{
	}
}

internal static class AvaloniaHostHeadlessEntryPoint
{
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<AvaloniaHostApplication>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
