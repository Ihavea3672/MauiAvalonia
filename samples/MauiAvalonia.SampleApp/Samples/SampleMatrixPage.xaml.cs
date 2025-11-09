using System.Collections.ObjectModel;

namespace MauiAvalonia.SampleApp;

public partial class SampleMatrixPage : ContentPage
{
	public ObservableCollection<SampleScenario> Scenarios { get; } = new();

	public SampleMatrixPage()
	{
		InitializeComponent();
		BindingContext = this;
		PopulateScenarios();
	}

	void PopulateScenarios()
	{
		if (Scenarios.Count > 0)
			return;

		Scenarios.Add(new SampleScenario(
			"Shell navigation",
			"Exercises route registration, query parameters, and Shell.Current.GoToAsync across the Avalonia host.",
			"shell-navigation",
			"Shell",
			"Navigation"));

		Scenarios.Add(new SampleScenario(
			"Tabs + Flyout",
			"Runs tabbed content inside a FlyoutItem so Avalonia chrome stays in sync with Shell tabs.",
			"flyout-sample",
			"Tabs",
			"Flyout"));

		Scenarios.Add(new SampleScenario(
			"Data grids",
			"Renders tabular data with column headers, sorting, and alternating rows using CollectionView + Grid.",
			"data-grid-sample",
			"Data grid",
			"CollectionView"));

		Scenarios.Add(new SampleScenario(
			"Drag & drop board",
			"Drag gesture recognizers move cards between Backlog and Done columns using Avalonia drag/drop adapters.",
			"drag-drop-sample",
			"Drag/drop",
			"Gestures"));

		Scenarios.Add(new SampleScenario(
			"Graphics view",
			"Animates DrawingView/GraphicsView rendering through Avalonia's Skia surface.",
			"graphics-sample",
			"GraphicsView",
			"DrawingView"));
	}

	async void OnNavigateToScenario(object? sender, EventArgs e)
	{
		if (sender is not BindableObject { BindingContext: SampleScenario scenario })
			return;

		try
		{
			var route = $"///sample-matrix/{scenario.Route}";
			await Shell.Current.GoToAsync(route);
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Navigation to {scenario.Route} failed: {ex}");
		}
	}

	public sealed record SampleScenario(string Title, string Description, string Route, params string[] Features)
	{
		public string FeatureList => string.Join(" · ", Features);
	}
}
