using System.Collections.ObjectModel;

namespace MauiAvalonia.SampleApp;

public partial class FlyoutSamplePage : ContentPage
{
	public ObservableCollection<FlyoutEntry> FlyoutEntries { get; } = new();

	bool isFlyoutLocked;

	public FlyoutSamplePage()
	{
		InitializeComponent();
		BindingContext = this;

		FlyoutEntries.Add(new FlyoutEntry("Sample matrix overview", "Lists every scenario and deep-links into each page."));
		FlyoutEntries.Add(new FlyoutEntry("Shell navigation", "Pushes routes and query parameters to test Avalonia window stacks."));
		FlyoutEntries.Add(new FlyoutEntry("Drag/drop board", "Moves cards between columns and exercises gesture recognizers."));
		FlyoutEntries.Add(new FlyoutEntry("Graphics view", "Renders animated drawing primitives via Microsoft.Maui.Graphics."));

		if (Shell.Current is Shell shell)
			IsFlyoutLocked = shell.FlyoutBehavior == FlyoutBehavior.Locked;
	}

	public bool IsFlyoutLocked
	{
		get => isFlyoutLocked;
		set
		{
			if (isFlyoutLocked == value)
				return;

			isFlyoutLocked = value;
			OnPropertyChanged(nameof(IsFlyoutLocked));
		}
	}

	void OnShowFlyout(object? sender, EventArgs e)
	{
		if (Shell.Current is Shell shell)
			shell.FlyoutIsPresented = true;
	}

	void OnHideFlyout(object? sender, EventArgs e)
	{
		if (Shell.Current is Shell shell)
			shell.FlyoutIsPresented = false;
	}

	void OnFlyoutLockToggled(object? sender, ToggledEventArgs e)
	{
		if (Shell.Current is not Shell shell)
			return;

		IsFlyoutLocked = e.Value;
		shell.FlyoutBehavior = e.Value ? FlyoutBehavior.Locked : FlyoutBehavior.Flyout;
	}

	public sealed record FlyoutEntry(string Title, string Description);
}
