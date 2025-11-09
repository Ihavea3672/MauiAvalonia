namespace MauiAvalonia.SampleApp;

[QueryProperty(nameof(TitleText), "title")]
[QueryProperty(nameof(MessageText), "message")]
public partial class ShellNavigationDetailPage : ContentPage
{
	string? titleText;
	string? messageText;

	public ShellNavigationDetailPage()
	{
		InitializeComponent();
	}

	public string? TitleText
	{
		get => titleText;
		set
		{
			titleText = Uri.UnescapeDataString(value ?? string.Empty);
			TitleLabel.Text = string.IsNullOrWhiteSpace(titleText) ? "Shell route detail" : titleText;
		}
	}

	public string? MessageText
	{
		get => messageText;
		set
		{
			messageText = Uri.UnescapeDataString(value ?? string.Empty);
			MessageLabel.Text = string.IsNullOrWhiteSpace(messageText)
				? "No payload provided."
				: $"Payload captured at {messageText}";
		}
	}

	async void OnBackClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
