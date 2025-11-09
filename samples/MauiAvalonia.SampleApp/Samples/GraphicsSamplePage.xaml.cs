namespace MauiAvalonia.SampleApp;

public partial class GraphicsSamplePage : ContentPage
{
	public GraphicsSampleDrawable Drawable { get; } = new();

	readonly Random random = new();

	public GraphicsSamplePage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	void OnRotationChanged(object? sender, ValueChangedEventArgs e)
	{
		Drawable.State.Rotation = (float)e.NewValue;
		SampleGraphicsView.Invalidate();
	}

	void OnProgressChanged(object? sender, ValueChangedEventArgs e)
	{
		Drawable.State.Progress = (float)e.NewValue;
		SampleGraphicsView.Invalidate();
	}

	void OnThicknessChanged(object? sender, ValueChangedEventArgs e)
	{
		Drawable.State.StrokeThickness = (float)e.NewValue;
		SampleGraphicsView.Invalidate();
	}

	void OnRandomizeClicked(object? sender, EventArgs e)
	{
		Drawable.State.PrimaryColor = Color.FromRgb(random.NextDouble(), random.NextDouble(), random.NextDouble());
		Drawable.State.AccentColor = Color.FromRgb(random.NextDouble(), random.NextDouble(), random.NextDouble());
		SampleGraphicsView.Invalidate();
	}
}
