using Microsoft.Maui.Graphics;

namespace MauiAvalonia.SampleApp;

public sealed class GraphicsSampleDrawable : IDrawable
{
	public GraphicsSampleState State { get; } = new();

	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.SaveState();
		canvas.Antialias = true;

		canvas.FillColor = Color.FromArgb("#F2F2FF");
		canvas.FillRoundedRectangle(dirtyRect, 20);

		canvas.StrokeColor = Color.FromArgb("#E0E0F3");
		canvas.StrokeSize = 1;

		for (var x = dirtyRect.X + 12; x < dirtyRect.Right; x += 28)
			canvas.DrawLine(x, dirtyRect.Y + 12, x, dirtyRect.Bottom - 12);

		for (var y = dirtyRect.Y + 12; y < dirtyRect.Bottom; y += 28)
			canvas.DrawLine(dirtyRect.X + 12, y, dirtyRect.Right - 12, y);

		canvas.Translate(dirtyRect.Center.X, dirtyRect.Center.Y);
		canvas.Rotate(State.Rotation);

		var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - 30;

		canvas.StrokeColor = State.PrimaryColor;
		canvas.StrokeSize = State.StrokeThickness;
		canvas.DrawCircle(0, 0, radius);

		canvas.StrokeColor = State.AccentColor;
		canvas.StrokeSize = State.StrokeThickness + 2;
		canvas.DrawArc(-radius, -radius, radius * 2, radius * 2, -90, 360 * State.Progress, false, false);

		canvas.FillColor = State.AccentColor.WithAlpha(0.9f);
		var dotRadius = 8;
		var angle = (float)(Math.PI * 2 * State.Progress - Math.PI / 2);
		var dotX = (float)(Math.Cos(angle) * radius);
		var dotY = (float)(Math.Sin(angle) * radius);
		canvas.FillCircle(dotX, dotY, dotRadius);

		canvas.FillColor = State.PrimaryColor.WithAlpha(0.4f);
		canvas.FillCircle(0, 0, radius - 40);

		canvas.FillColor = Colors.White;
		canvas.FontColor = State.PrimaryColor;
		canvas.FontSize = 18;
		canvas.DrawString(
			$"{State.Progress:P0}",
			-radius,
			-radius,
			radius * 2,
			radius * 2,
			HorizontalAlignment.Center,
			VerticalAlignment.Center);

		canvas.RestoreState();
	}
}

public sealed class GraphicsSampleState
{
	public float Rotation { get; set; } = 18f;
	public float Progress { get; set; } = 0.65f;
	public float StrokeThickness { get; set; } = 4f;
	public Color PrimaryColor { get; set; } = Color.FromArgb("#7159DF");
	public Color AccentColor { get; set; } = Color.FromArgb("#FF7F57");
}
