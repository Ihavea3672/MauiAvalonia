using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Microsoft.Maui.Accessibility;

namespace Microsoft.Maui.Avalonia.ApplicationModel;

internal sealed class AvaloniaSemanticScreenReader : ISemanticScreenReader
{
	readonly global::Avalonia.Threading.Dispatcher _dispatcher = global::Avalonia.Threading.Dispatcher.UIThread;
	static readonly AutomationProperty? HelpTextAutomationProperty;
	static readonly bool AutomationSupported;

	static AvaloniaSemanticScreenReader()
	{
		try
		{
			HelpTextAutomationProperty = GetAutomationProperty("HelpTextProperty");
			AutomationSupported = true;
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[MauiAvalonia] SemanticScreenReader disabled: {ex.Message}");
			AutomationSupported = false;
		}
	}

	public void Announce(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		if (_dispatcher.CheckAccess())
			AnnounceCore(text);
		else
			_dispatcher.Post(() => AnnounceCore(text));
	}

	void AnnounceCore(string text)
	{
		if (!AutomationSupported || HelpTextAutomationProperty is null)
		{
			Debug.WriteLine($"[MauiAvalonia] SemanticScreenReader announcement skipped (automation unsupported). Text='{text}'.");
			return;
		}

		var target = GetAutomationTarget();
		if (target is null)
		{
			Debug.WriteLine($"[MauiAvalonia] SemanticScreenReader announcement skipped (no automation target). Text='{text}'.");
			return;
		}

		var peer = ControlAutomationPeer.CreatePeerForElement(target);
		var previousHelpText = global::Avalonia.Automation.AutomationProperties.GetHelpText(target);

		try
		{
			global::Avalonia.Automation.AutomationProperties.SetHelpText(target, text);
			peer.RaisePropertyChangedEvent(HelpTextAutomationProperty, previousHelpText, text);
		}
		finally
		{
			if (string.IsNullOrEmpty(previousHelpText))
				target.ClearValue(global::Avalonia.Automation.AutomationProperties.HelpTextProperty);
			else
				global::Avalonia.Automation.AutomationProperties.SetHelpText(target, previousHelpText);
		}
	}

	static Control? GetAutomationTarget()
	{
		var topLevel = EnumerateTopLevels().FirstOrDefault();
		if (topLevel?.FocusManager?.GetFocusedElement() is Control focusedControl)
			return focusedControl;

		return topLevel;
	}

	static IEnumerable<TopLevel> EnumerateTopLevels()
	{
		if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
		{
			foreach (var window in desktopLifetime.Windows.OrderByDescending(w => w.IsActive))
				yield return window;

			if (desktopLifetime.MainWindow is not null)
				yield return desktopLifetime.MainWindow;
		}
		else if (global::Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
		{
			var topLevel = TopLevel.GetTopLevel(singleViewLifetime.MainView);
			if (topLevel is not null)
				yield return topLevel;
		}
	}

	static AutomationProperty GetAutomationProperty(string propertyName)
	{
		var propertyInfo = typeof(AutomationElementIdentifiers).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
		if (propertyInfo?.GetValue(null) is AutomationProperty automationProperty)
			return automationProperty;

		throw new InvalidOperationException($"Automation property '{propertyName}' is not available in this Avalonia build.");
	}

	public static void TryRegisterAsDefault(ISemanticScreenReader implementation)
	{
		var method = typeof(SemanticScreenReader).GetMethod("SetDefault", BindingFlags.Static | BindingFlags.NonPublic);
		method?.Invoke(null, new object?[] { implementation });
	}
}
