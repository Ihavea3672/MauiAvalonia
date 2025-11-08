using System;
using Avalonia;
using AvaloniaFocusManager = Avalonia.Input.FocusManager;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Fonts;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Avalonia.Platform;
using Microsoft.Maui.Handlers;
using AvaloniaTextChangedEventArgs = global::Avalonia.Controls.TextChangedEventArgs;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaEntryHandler : AvaloniaViewHandler<IEntry, TextBox>, IEntryHandler
{
	public static IPropertyMapper<IEntry, AvaloniaEntryHandler> Mapper = new PropertyMapper<IEntry, AvaloniaEntryHandler>(ViewHandler.ViewMapper)
	{
		[nameof(ITextInput.Text)] = MapText,
		[nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
		[nameof(ITextStyle.TextColor)] = MapTextColor,
		[nameof(ITextStyle.Font)] = MapFont,
		[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
		[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
		[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
		[nameof(IEntry.IsPassword)] = MapIsPassword,
		[nameof(ITextInput.Keyboard)] = MapKeyboard,
		[nameof(ITextInput.IsTextPredictionEnabled)] = MapTextInputOptions,
		[nameof(ITextInput.IsSpellCheckEnabled)] = MapTextInputOptions,
		[nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
		[nameof(ITextInput.MaxLength)] = MapMaxLength,
		[nameof(ITextInput.CursorPosition)] = MapCursorPosition,
		[nameof(ITextInput.SelectionLength)] = MapSelectionLength,
		[nameof(IEntry.ReturnType)] = MapReturnType
	};

	public AvaloniaEntryHandler()
		: base(Mapper)
	{
	}

	protected override TextBox CreatePlatformView()
	{
		var textBox = new TextBox
		{
			AcceptsReturn = false,
			TextWrapping = global::Avalonia.Media.TextWrapping.NoWrap
		};
		ScrollViewer.SetVerticalScrollBarVisibility(textBox, global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled);
		return textBox;
	}

	protected override void ConnectHandler(TextBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.TextChanged += OnTextChanged;
		platformView.PropertyChanged += OnTextBoxPropertyChanged;
		platformView.KeyUp += OnKeyUp;
	}

	protected override void DisconnectHandler(TextBox platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.TextChanged -= OnTextChanged;
		platformView.PropertyChanged -= OnTextBoxPropertyChanged;
		platformView.KeyUp -= OnKeyUp;
	}

	static void MapText(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		var text = entry.Text ?? string.Empty;
		if (handler.PlatformView.Text != text)
			handler.PlatformView.Text = text;
	}

	static void MapPlaceholder(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Watermark = entry.Placeholder;
	}

	static void MapTextColor(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.UpdateForegroundColor(entry);
	}

	static void MapFont(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		var fontManager = handler.GetRequiredService<IAvaloniaFontManager>();
		handler.PlatformView.UpdateFont(entry, fontManager);
	}

	static void MapCharacterSpacing(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.LetterSpacing = entry.CharacterSpacing.ToAvaloniaLetterSpacing();
	}

	static void MapHorizontalTextAlignment(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.TextAlignment = entry.HorizontalTextAlignment.ToAvaloniaHorizontalAlignment();
	}

	static void MapVerticalTextAlignment(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.VerticalContentAlignment = entry.VerticalTextAlignment.ToAvaloniaVerticalAlignment();
	}

	static void MapIsPassword(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.PasswordChar = entry.IsPassword ? '●' : '\0';
		handler.UpdateTextInput(entry);
	}

	static void MapKeyboard(AvaloniaEntryHandler handler, IEntry entry) =>
		handler.UpdateTextInput(entry);

	static void MapTextInputOptions(AvaloniaEntryHandler handler, IEntry entry) =>
		handler.UpdateTextInput(entry);

	void UpdateTextInput(IEntry entry)
	{
		if (PlatformView is null || entry is null)
			return;

		PlatformView.UpdateTextInputOptions(entry, isMultiline: false, entry.ReturnType, entry.IsPassword);
	}

	static void MapIsReadOnly(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.IsReadOnly = entry.IsReadOnly;
	}

	static void MapMaxLength(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.MaxLength = entry.MaxLength <= 0 ? int.MaxValue : entry.MaxLength;
	}

	static void MapCursorPosition(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.SelectionStart = Math.Min(entry.CursorPosition, handler.PlatformView.Text?.Length ?? 0);
		UpdateSelection(handler.PlatformView, entry);
	}

	static void MapSelectionLength(AvaloniaEntryHandler handler, IEntry entry)
	{
		if (handler.PlatformView is null)
			return;

		UpdateSelection(handler.PlatformView, entry);
	}

	static void MapReturnType(AvaloniaEntryHandler handler, IEntry entry)
	{
		handler.UpdateTextInput(entry);
	}

	static void UpdateSelection(TextBox textBox, ITextInput input)
	{
		var start = Math.Min(input.CursorPosition, textBox.Text?.Length ?? 0);
		var length = Math.Clamp(input.SelectionLength, 0, Math.Max(0, (textBox.Text?.Length ?? 0) - start));
		textBox.SelectionStart = start;
		textBox.SelectionEnd = start + length;
	}

	void OnTextChanged(object? sender, AvaloniaTextChangedEventArgs args)
	{
		if (VirtualView is null || PlatformView is null)
			return;

		var text = PlatformView.Text ?? string.Empty;
		if (VirtualView.Text != text)
			VirtualView.UpdateText(text);
	}

	void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (VirtualView is null || PlatformView is null)
			return;

		if (e.Property == TextBox.SelectionStartProperty || e.Property == TextBox.SelectionEndProperty)
			UpdateVirtualSelection();
	}

	void UpdateVirtualSelection()
	{
		if (VirtualView is null || PlatformView is null)
			return;

		var cursor = PlatformView.SelectionStart;
		var length = Math.Max(0, PlatformView.SelectionEnd - PlatformView.SelectionStart);

		if (VirtualView.CursorPosition != cursor)
			VirtualView.CursorPosition = cursor;

		if (VirtualView.SelectionLength != length)
			VirtualView.SelectionLength = length;
	}

	void OnKeyUp(object? sender, KeyEventArgs e)
	{
		if (e.Key != Key.Enter && e.Key != Key.Return)
			return;

		VirtualView?.Completed();
	}
}
