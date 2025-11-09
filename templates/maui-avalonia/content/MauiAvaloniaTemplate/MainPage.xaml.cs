using System.ComponentModel;

namespace MauiAvaloniaTemplate;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
	int _count;
	string _counterText = "Click me";

	public MainPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	public string CounterText
	{
		get => _counterText;
		set
		{
			if (_counterText == value)
				return;

			_counterText = value;
			OnPropertyChanged();
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	void OnCounterClicked(object sender, EventArgs e)
	{
		_count++;
		CounterText = _count == 1 ? "Clicked once" : $"Clicked {_count} times";
	}
}
