using System.Collections.ObjectModel;

namespace MauiAvalonia.SampleApp;

public partial class DragDropSamplePage : ContentPage
{
	public ObservableCollection<DragTask> BacklogTasks { get; } = new();
	public ObservableCollection<DragTask> DoneTasks { get; } = new();

	public DragDropSamplePage()
	{
		InitializeComponent();
		BindingContext = this;

		BacklogTasks.CollectionChanged += (_, _) => UpdateSummaries();
		DoneTasks.CollectionChanged += (_, _) => UpdateSummaries();

		Seed();
		UpdateSummaries();
	}

	public string BacklogSummary => $"{BacklogTasks.Count} task(s) waiting";
	public string DoneSummary => $"{DoneTasks.Count} task(s) finished";

	void Seed()
	{
		BacklogTasks.Clear();
		DoneTasks.Clear();

		BacklogTasks.Add(new DragTask("Build AppShell chrome", "Integrate Avalonia flyout window buttons."));
		BacklogTasks.Add(new DragTask("Hook Shell navigation", "Map GoToAsync routes to Avalonia windows."));
		BacklogTasks.Add(new DragTask("Data grid parity", "Validate CollectionView virtualization and templates."));
		DoneTasks.Add(new DragTask("GraphicsView host", "Connect MAUI drawing pipeline to Avalonia."));
	}

	void OnDragStarting(object? sender, DragStartingEventArgs e)
	{
		if (sender is not BindableObject { BindingContext: DragTask task })
			return;

		e.Data.Properties["Task"] = task;
		e.Data.Text = task.Title;
	}

	void OnDrop(object? sender, DropEventArgs e)
	{
		if (!e.Data.Properties.TryGetValue("Task", out var taskObj) || taskObj is not DragTask task)
			return;

		var targetId = (sender as DropGestureRecognizer)?.ClassId;

		var source = BacklogTasks.Contains(task) ? BacklogTasks : DoneTasks.Contains(task) ? DoneTasks : null;
		var target = string.Equals(targetId, "Done", StringComparison.OrdinalIgnoreCase) ? DoneTasks : BacklogTasks;

		if (source == target || source is null || target.Contains(task))
			return;

		source.Remove(task);
		target.Add(task);
	}

	void UpdateSummaries()
	{
		OnPropertyChanged(nameof(BacklogSummary));
		OnPropertyChanged(nameof(DoneSummary));
	}

	public sealed record DragTask(string Title, string Description);
}
