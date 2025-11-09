using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;
using MauiAvalonia.SampleApp;

namespace Microsoft.Maui.Avalonia.Tests;

public sealed class SampleMatrixTests
{
	[Fact]
	public async Task TabBarTemplatesCreateExpectedPages()
	{
		await AvaloniaHeadlessFixture.Session.Dispatch(() =>
		{
			WithSampleApp(() =>
			{
				var shell = new AppShell();
				var tabBar = shell.Items.OfType<TabBar>().Single();

				var expectedPageTypes = new[]
				{
					typeof(MainPage),
					typeof(TabsOverviewPage),
					typeof(TabsTasksPage),
					typeof(TabsNotesPage)
				};

				var sections = tabBar.Items.OfType<ShellSection>().ToList();
				Assert.Equal(expectedPageTypes.Length, sections.Count);

				for (var i = 0; i < expectedPageTypes.Length; i++)
				{
					var section = sections[i];
					var content = section.Items.OfType<ShellContent>().Single();
					var template = content.ContentTemplate;
					Assert.NotNull(template);

					var page = template!.CreateContent() as Page;
					Assert.NotNull(page);
					Assert.IsType(expectedPageTypes[i], page);
				}
			});
		}, CancellationToken.None);
	}

	[Fact]
	public async Task SampleMatrixFlyoutRegistersAllRoutes()
	{
		await AvaloniaHeadlessFixture.Session.Dispatch(() =>
		{
			WithSampleApp(() =>
			{
				var shell = new AppShell();
				var flyoutItem = shell.Items.OfType<FlyoutItem>().Single(item => item.Route == "sample-matrix");

				var expectedRoutes = new[]
				{
					"matrix-overview",
					"shell-navigation",
					"flyout-sample",
					"data-grid-sample",
					"drag-drop-sample",
					"graphics-sample"
				};

				var actualRoutes = flyoutItem.Items
					.OfType<ShellSection>()
					.SelectMany(section => section.Items.OfType<ShellContent>())
					.Select(sc => sc.Route)
					.ToArray();

				Assert.Equal(expectedRoutes, actualRoutes);
			});
		}, CancellationToken.None);
	}

	static void WithSampleApp(Action action)
	{
		var previous = Microsoft.Maui.Controls.Application.Current;
		Microsoft.Maui.Controls.Application.Current = new App();

		try
		{
			action();
		}
		finally
		{
			Microsoft.Maui.Controls.Application.Current = previous;
		}
	}
}
