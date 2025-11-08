using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Avalonia.Fonts;
using Microsoft.Maui.Avalonia.Internal;
using Microsoft.Maui.Avalonia.Platform;
using Microsoft.Maui.Handlers;
using AvaloniaTextChangedEventArgs = global::Avalonia.Controls.TextChangedEventArgs;

namespace Microsoft.Maui.Avalonia.Handlers;

public class AvaloniaEditorHandler : AvaloniaViewHandler<IEditor, TextBox>, IEditorHandler
{
	public static IPropertyMapper<IEditor, AvaloniaEditorHandler> Mapper = new PropertyMapper<IEditor, AvaloniaEditorHandler>(ViewHandler.ViewMapper)
	{
		[nameof(ITextInput.Text)] = MapText,
		[nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
		[nameof(ITextStyle.TextColor)] = MapTextColor,
		[nameof(ITextStyle.Font)] = MapFont,
		[nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
		[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
		[nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
		[nameof(ITextInput.Keyboard)] = MapKeyboard,
		[nameof(ITextInput.IsTextPredictionEnabled)] = MapTextInputOptions,
		[nameof(ITextInput.IsSpellCheckEnabled)] = MapTextInputOptions,
		[nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
		[nameof(ITextInput.MaxLength)] = MapMaxLength,
		[nameof(ITextInput.CursorPosition)] = MapCursorPosition,
		[nameof(ITextInput.SelectionLength)] = MapSelectionLength
	};

	public AvaloniaEditorHandler()
		: base(Mapper)
	{
	}

	protected override TextBox CreatePlatformView()
	{
		var textBox = new TextBox
		{
			AcceptsReturn = true,
			TextWrapping = global::Avalonia.Media.TextWrapping.Wrap
		};
		ScrollViewer.SetVerticalScrollBarVisibility(textBox, global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
		return textBox;
	}

	protected override void ConnectHandler(TextBox platformView)
	{
		base.ConnectHandler(platformView);
		platformView.TextChanged += OnTextChanged;
		platformView.PropertyChanged += OnTextBoxPropertyChanged;
		platformView.LostFocus += OnLostFocus;
	}

	protected override void DisconnectHandler(TextBox platformView)
	{
		base.DisconnectHandler(platformView);
		platformView.TextChanged -= OnTextChanged;
		platformView.PropertyChanged -= OnTextBoxPropertyChanged;
		platformView.LostFocus -= OnLostFocus;
	}

	static void MapText(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		var text = editor.Text ?? string.Empty;
		if (handler.PlatformView.Text != text)
			handler.PlatformView.Text = text;
	}

	static void MapPlaceholder(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.Watermark = editor.Placeholder;
	}

	static void MapTextColor(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.UpdateForegroundColor(editor);
	}

	static void MapFont(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		var fontManager = handler.GetRequiredService<IAvaloniaFontManager>();
		handler.PlatformView.UpdateFont(editor, fontManager);
	}

	static void MapCharacterSpacing(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.LetterSpacing = editor.CharacterSpacing.ToAvaloniaLetterSpacing();
	}

	static void MapHorizontalTextAlignment(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.TextAlignment = editor.HorizontalTextAlignment.ToAvaloniaHorizontalAlignment();
	}

	static void MapVerticalTextAlignment(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.VerticalContentAlignment = editor.VerticalTextAlignment.ToAvaloniaVerticalAlignment();
	}

	static void MapIsReadOnly(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.IsReadOnly = editor.IsReadOnly;
		handler.UpdateTextInput(editor);
	}

	static void MapMaxLength(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.MaxLength = editor.MaxLength <= 0 ? int.MaxValue : editor.MaxLength;
	}

	static void MapKeyboard(AvaloniaEditorHandler handler, IEditor editor) =>
		handler.UpdateTextInput(editor);

	static void MapTextInputOptions(AvaloniaEditorHandler handler, IEditor editor) =>
		handler.UpdateTextInput(editor);

	void UpdateTextInput(IEditor editor)
	{
		if (PlatformView is null || editor is null)
			return;

		PlatformView.UpdateTextInputOptions(editor, isMultiline: true);
	}

	static void MapCursorPosition(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		handler.PlatformView.SelectionStart = Math.Min(editor.CursorPosition, handler.PlatformView.Text?.Length ?? 0);
		UpdateSelection(handler.PlatformView, editor);
	}

	static void MapSelectionLength(AvaloniaEditorHandler handler, IEditor editor)
	{
		if (handler.PlatformView is null)
			return;

		UpdateSelection(handler.PlatformView, editor);
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

	void OnLostFocus(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e) =>
		VirtualView?.Completed();
}
