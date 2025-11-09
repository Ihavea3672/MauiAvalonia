using System.Collections.ObjectModel;

namespace MauiAvalonia.SampleApp;

public partial class ShellNavigationSamplePage : ContentPage
{
	public ObservableCollection<RouteDefinition> Routes { get; } = new();
	public ObservableCollection<string> NavigationLog { get; } = new();

	public ShellNavigationSamplePage()
	{
		InitializeComponent();
		BindingContext = this;

		Routes.Add(new RouteDefinition(
			"Push detail page",
			"Navigates to ShellNavigationDetailPage using query arguments.",
			"Navigate",
			RouteAction.PushDetail));

		Routes.Add(new RouteDefinition(
			"Deep link to tabs sample",
			"Jumps directly to the stand-alone TabbedPage handler route.",
			"Go to TabsPage",
			RouteAction.OpenTabsPage));

		Routes.Add(new RouteDefinition(
			"Jump to drag/drop demo",
			"Verifies named routes under the Sample Matrix flyout.",
			"Go to board",
			RouteAction.OpenDragDrop));

		Routes.Add(new RouteDefinition(
			"Navigate back",
			"Pops the current Shell page using relative navigation.",
			"Go back",
			RouteAction.GoBack));
	}

	async void OnExecuteRoute(object? sender, EventArgs e)
	{
		if (sender is not BindableObject { BindingContext: RouteDefinition definition })
			return;

		try
		{
			switch (definition.Action)
			{
				case RouteAction.PushDetail:
					var query =
						$"{nameof(ShellNavigationDetailPage)}?title={Uri.EscapeDataString("Route detail")}&message={Uri.EscapeDataString(DateTime.Now.ToLongTimeString())}";
					await Shell.Current.GoToAsync(query);
					AppendLog("Navigated to ShellNavigationDetailPage.");
					break;

				case RouteAction.OpenTabsPage:
					await Shell.Current.GoToAsync(nameof(TabsPage));
					AppendLog("Opened standalone TabsPage route.");
					break;

				case RouteAction.OpenDragDrop:
					await Shell.Current.GoToAsync("///sample-matrix/drag-drop-sample");
					AppendLog("Deep linked to drag/drop sample.");
					break;

				case RouteAction.GoBack:
					await Shell.Current.GoToAsync("..");
					AppendLog("Requested Shell back navigation.");
					break;
			}
		}
		catch (Exception ex)
		{
			AppendLog($"Navigation error: {ex.Message}");
		}
	}

	void OnClearLog(object? sender, EventArgs e) => NavigationLog.Clear();

	void AppendLog(string message)
	{
		var timestamped = $"{DateTime.Now:HH:mm:ss} • {message}";
		NavigationLog.Insert(0, timestamped);

		const int maxEntries = 10;
		while (NavigationLog.Count > maxEntries)
			NavigationLog.RemoveAt(NavigationLog.Count - 1);
	}

	public sealed record RouteDefinition(string Title, string Description, string ActionLabel, RouteAction Action);

	public enum RouteAction
	{
		PushDetail,
		OpenTabsPage,
		OpenDragDrop,
		GoBack
	}
}
