using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaPoint = Avalonia.Point;
using AvaloniaPointerEventArgs = Avalonia.Input.PointerEventArgs;
using AvaloniaPointerPressedEventArgs = Avalonia.Input.PointerPressedEventArgs;
using AvaloniaPointerReleasedEventArgs = Avalonia.Input.PointerReleasedEventArgs;
using AvaloniaDragEventArgsNative = Avalonia.Input.DragEventArgs;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using ButtonsMask = Microsoft.Maui.Controls.ButtonsMask;
using MauiControls = Microsoft.Maui.Controls;
using MauiPoint = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Avalonia.Input;

internal sealed class AvaloniaInputAdapter : IDisposable
{
	const double DragStartThreshold = 4d;

	readonly Control _control;
	readonly IView _view;
	readonly MauiControls.View? _controlsView;
	readonly IList<IGestureRecognizer>? _gestureCollection;
	readonly INotifyCollectionChanged? _gestureNotifier;
	readonly List<INotifyPropertyChanged> _gestureSubscriptions = new();

	MauiControls.DragGestureRecognizer? _pendingDragRecognizer;
	AvaloniaPointerPressedEventArgs? _dragTriggerEvent;
	AvaloniaPoint? _dragOrigin;
	int? _dragPointerId;
	bool _dragInProgress;
	bool _dropEventsAttached;
	bool _isDisposed;

	AvaloniaInputAdapter(IView view, Control control)
	{
		_view = view ?? throw new ArgumentNullException(nameof(view));
		_control = control ?? throw new ArgumentNullException(nameof(control));
		_controlsView = view as MauiControls.View;
		_gestureCollection = _controlsView?.GestureRecognizers;
		if (_gestureCollection is INotifyCollectionChanged notifier)
		{
			_gestureNotifier = notifier;
			notifier.CollectionChanged += OnGesturesChanged;
		}

		if (_gestureCollection != null)
		{
			foreach (var recognizer in _gestureCollection)
				SubscribeGesture(recognizer);
		}

		UpdateDropSubscriptions();
		HookControlEvents();
	}

	public static AvaloniaInputAdapter? Attach(IView view, Control control)
	{
		if (view is null || control is null)
			return null;

		return new AvaloniaInputAdapter(view, control);
	}

	public void Dispose()
	{
		if (_isDisposed)
			return;

		_isDisposed = true;
		UnhookControlEvents();
		DetachGestureSubscriptions();
	}

	void HookControlEvents()
	{
		_control.PointerEntered += OnPointerEntered;
		_control.PointerExited += OnPointerExited;
		_control.PointerMoved += OnPointerMoved;
		_control.PointerPressed += OnPointerPressed;
		_control.PointerReleased += OnPointerReleased;
		_control.GotFocus += OnGotFocus;
		_control.LostFocus += OnLostFocus;
	}

	void UnhookControlEvents()
	{
		_control.PointerEntered -= OnPointerEntered;
		_control.PointerExited -= OnPointerExited;
		_control.PointerMoved -= OnPointerMoved;
		_control.PointerPressed -= OnPointerPressed;
		_control.PointerReleased -= OnPointerReleased;
		_control.GotFocus -= OnGotFocus;
		_control.LostFocus -= OnLostFocus;
		DetachDropHandlers();
	}

	void OnPointerEntered(object? sender, AvaloniaPointerEventArgs e) =>
		DispatchPointerGestures((view, recognizer) =>
			recognizer.SendPointerEntered(view, relative => GetPointerPosition(relative, e), null, GetButtonsMask(e)));

	void OnPointerExited(object? sender, AvaloniaPointerEventArgs e)
	{
		DispatchPointerGestures((view, recognizer) =>
			recognizer.SendPointerExited(view, relative => GetPointerPosition(relative, e), null, GetButtonsMask(e)));
		ResetPendingDrag();
	}

	void OnPointerMoved(object? sender, AvaloniaPointerEventArgs e)
	{
		DispatchPointerGestures((view, recognizer) =>
			recognizer.SendPointerMoved(view, relative => GetPointerPosition(relative, e), null, GetButtonsMask(e)));

		TryStartDrag(e);
	}

	void OnPointerPressed(object? sender, AvaloniaPointerPressedEventArgs e)
	{
		var buttons = GetButtonsMask(e);
		DispatchPointerGestures((view, recognizer) =>
			recognizer.SendPointerPressed(view, relative => GetPointerPosition(relative, e), null, buttons));

		PreparePendingDrag(e);
	}

	void OnPointerReleased(object? sender, AvaloniaPointerReleasedEventArgs e)
	{
		var buttons = e.InitialPressMouseButton == MouseButton.Right ? ButtonsMask.Secondary : ButtonsMask.Primary;
		DispatchPointerGestures((view, recognizer) =>
			recognizer.SendPointerReleased(view, relative => GetPointerPosition(relative, e), null, buttons));

		ResetPendingDrag();
	}

	void OnGotFocus(object? sender, GotFocusEventArgs e)
	{
		if (!_view.IsFocused)
			_view.IsFocused = true;
	}

	void OnLostFocus(object? sender, RoutedEventArgs e)
	{
		if (_view.IsFocused)
			_view.IsFocused = false;
	}

	void DispatchPointerGestures(Action<MauiControls.View, PointerGestureRecognizer> dispatch)
	{
		var view = _controlsView;
		if (view?.GestureRecognizers is not IEnumerable<IGestureRecognizer> gestures)
			return;

		var pointerGestures = gestures.GetGesturesFor<PointerGestureRecognizer>();
		foreach (var recognizer in pointerGestures)
			dispatch(view, recognizer);
	}

	void PreparePendingDrag(AvaloniaPointerPressedEventArgs e)
	{
		if (_controlsView?.GestureRecognizers is not IEnumerable<IGestureRecognizer> gestures)
			return;

		var dragRecognizer = gestures.GetGesturesFor<MauiControls.DragGestureRecognizer>()
			.FirstOrDefault(g => g.CanDrag);

		if (dragRecognizer is null)
		{
			ResetPendingDrag();
			return;
		}

		_pendingDragRecognizer = dragRecognizer;
		_dragTriggerEvent = e;
		_dragOrigin = e.GetCurrentPoint(_control).Position;
		_dragPointerId = e.Pointer.Id;
	}

	void TryStartDrag(AvaloniaPointerEventArgs e)
	{
		if (_pendingDragRecognizer is null ||
			_dragOrigin is null ||
			_dragPointerId != e.Pointer.Id ||
			_dragInProgress ||
			_dragTriggerEvent is null)
		{
			return;
		}

		var current = e.GetCurrentPoint(_control).Position;
		if (!ExceedsDragThreshold(current, _dragOrigin.Value))
			return;

		var recognizer = _pendingDragRecognizer;
		var trigger = _dragTriggerEvent;
		ResetPendingDrag();
		_ = StartDragAsync(recognizer, trigger);
	}

	async Task StartDragAsync(MauiControls.DragGestureRecognizer recognizer, AvaloniaPointerPressedEventArgs triggerEvent)
	{
		if (_controlsView is null)
			return;

		_dragInProgress = true;

		try
		{
			var args = recognizer.SendDragStarting(
				_controlsView,
				relative => GetPointerPosition(relative, triggerEvent),
				null);

			if (args.Cancel)
				return;

			var dataObject = BuildDataObject(args.Data);
#pragma warning disable CS0618
			var effects = await DragDrop.DoDragDrop(triggerEvent, dataObject, DragDropEffects.Copy);
#pragma warning restore CS0618
			var _ = effects; // currently unused but kept for parity
			recognizer.SendDropCompleted(new DropCompletedEventArgs());
		}
		finally
		{
			_dragInProgress = false;
		}
	}

	static IDataObject BuildDataObject(DataPackage package)
	{
		var dataObject = new DataObject();
		if (!string.IsNullOrEmpty(package.Text))
			dataObject.Set(DataFormats.Text, package.Text);
		return dataObject;
	}

	void ResetPendingDrag()
	{
		_pendingDragRecognizer = null;
		_dragTriggerEvent = null;
		_dragOrigin = null;
		_dragPointerId = null;
	}

	void UpdateDropSubscriptions()
	{
		if (HasDropGestures())
			AttachDropHandlers();
		else
			DetachDropHandlers();
	}

	bool HasDropGestures()
	{
		if (_controlsView?.GestureRecognizers is not IEnumerable<IGestureRecognizer> gestures)
			return false;

		return gestures.GetGesturesFor<MauiControls.DropGestureRecognizer>()
			.Any(recognizer => recognizer.AllowDrop);
	}

	void AttachDropHandlers()
	{
		if (_dropEventsAttached)
			return;

		if (!HasDropGestures())
			return;

		_dropEventsAttached = true;
		DragDrop.SetAllowDrop(_control, true);
		_control.AddHandler(DragDrop.DragEnterEvent, OnDragEnter, RoutingStrategies.Bubble);
		_control.AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Bubble);
		_control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave, RoutingStrategies.Bubble);
		_control.AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble);
	}

	void DetachDropHandlers()
	{
		if (!_dropEventsAttached)
			return;

		_dropEventsAttached = false;
		DragDrop.SetAllowDrop(_control, false);
		_control.RemoveHandler(DragDrop.DragEnterEvent, OnDragEnter);
		_control.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
		_control.RemoveHandler(DragDrop.DragLeaveEvent, OnDragLeave);
		_control.RemoveHandler(DragDrop.DropEvent, OnDrop);
	}

	void OnDragEnter(object? sender, AvaloniaDragEventArgsNative e) =>
		HandleDragOver(e);

	void OnDragOver(object? sender, AvaloniaDragEventArgsNative e) =>
		HandleDragOver(e);

	void OnDragLeave(object? sender, AvaloniaDragEventArgsNative e)
	{
		if (!TryGetDropGestures(out var gestures))
			return;

		var package = CreateDataPackage(e.Data);
		var args = new AvaloniaDragEventArgs(package, relative => GetDragPosition(relative, e));
		foreach (var recognizer in gestures)
			recognizer.SendDragLeave(args);

		e.Handled = true;
	}

	async void OnDrop(object? sender, AvaloniaDragEventArgsNative e)
	{
		if (!TryGetDropGestures(out var gestures))
			return;

		var package = CreateDataPackage(e.Data);
		var dropArgs = new AvaloniaDropEventArgs(package.View, relative => GetDragPosition(relative, e));
		foreach (var recognizer in gestures)
			await recognizer.SendDrop(dropArgs);

		e.DragEffects = dropArgs.Handled ? DragDropEffects.Copy : DragDropEffects.None;
		e.Handled = dropArgs.Handled;
	}

	void HandleDragOver(AvaloniaDragEventArgsNative e)
	{
		if (!TryGetDropGestures(out var gestures))
			return;

		var package = CreateDataPackage(e.Data);
		var dragArgs = new AvaloniaDragEventArgs(package, relative => GetDragPosition(relative, e));
		foreach (var recognizer in gestures)
			recognizer.SendDragOver(dragArgs);

		e.DragEffects = ConvertToDragEffects(dragArgs.AcceptedOperation);
		if (dragArgs.AcceptedOperation != DataPackageOperation.None)
			e.Handled = true;
	}

	static DragDropEffects ConvertToDragEffects(DataPackageOperation operation) =>
		operation == DataPackageOperation.Copy ? DragDropEffects.Copy : DragDropEffects.None;

	static DataPackage CreateDataPackage(IDataObject? data)
	{
		var package = new DataPackage();
		if (data?.Contains(DataFormats.Text) == true)
		{
			if (data.Get(DataFormats.Text) is string text && !string.IsNullOrEmpty(text))
				package.Text = text;
		}

		return package;
	}

	bool TryGetDropGestures(out IEnumerable<MauiControls.DropGestureRecognizer> gestures)
	{
		gestures = Enumerable.Empty<MauiControls.DropGestureRecognizer>();
		if (_controlsView?.GestureRecognizers is not IEnumerable<IGestureRecognizer> list)
			return false;

		var dropGestures = list.GetGesturesFor<MauiControls.DropGestureRecognizer>().ToArray();
		if (dropGestures.Length == 0)
			return false;

		gestures = dropGestures;
		return true;
	}

	MauiPoint? GetPointerPosition(IElement? relativeTo, AvaloniaPointerEventArgs e)
	{
		var target = ResolveVisual(relativeTo);
		if (target is null)
			return null;

		var point = e.GetPosition(target);
		return new MauiPoint(point.X, point.Y);
	}

	MauiPoint? GetPointerPosition(IElement? relativeTo, AvaloniaPointerPressedEventArgs e)
	{
		var target = ResolveVisual(relativeTo);
		if (target is null)
			return null;

		var point = e.GetPosition(target);
		return new MauiPoint(point.X, point.Y);
	}

	MauiPoint? GetDragPosition(IElement? relativeTo, AvaloniaDragEventArgsNative e)
	{
		var target = ResolveVisual(relativeTo);
		if (target is null)
			return null;

		var point = e.GetPosition(target);
		return new MauiPoint(point.X, point.Y);
	}

	Visual? ResolveVisual(IElement? relativeTo)
	{
		if (relativeTo?.Handler?.PlatformView is Visual visual)
			return visual;

		return _control;
	}

	static bool ExceedsDragThreshold(AvaloniaPoint current, AvaloniaPoint origin) =>
		Math.Abs(current.X - origin.X) >= DragStartThreshold ||
		Math.Abs(current.Y - origin.Y) >= DragStartThreshold;

	ButtonsMask GetButtonsMask(AvaloniaPointerEventArgs e)
	{
		if (e.Pointer.Type != PointerType.Mouse)
			return ButtonsMask.Primary;

		var point = e.GetCurrentPoint(_control);
		if (point.Properties.IsRightButtonPressed ||
			point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed ||
			point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased)
		{
			return ButtonsMask.Secondary;
		}

		return ButtonsMask.Primary;
	}

	void OnGesturesChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.OldItems != null)
		{
			foreach (var gesture in e.OldItems.OfType<IGestureRecognizer>())
				UnsubscribeGesture(gesture);
		}

		if (e.NewItems != null)
		{
			foreach (var gesture in e.NewItems.OfType<IGestureRecognizer>())
				SubscribeGesture(gesture);
		}

		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			DetachGestureSubscriptions(detachCollectionChanged: false);
			if (_gestureCollection != null)
			{
				foreach (var gesture in _gestureCollection)
					SubscribeGesture(gesture);
			}
		}

		UpdateDropSubscriptions();
	}

	void SubscribeGesture(IGestureRecognizer recognizer)
	{
		if (recognizer is not INotifyPropertyChanged notify)
			return;

		notify.PropertyChanged += OnGesturePropertyChanged;
		_gestureSubscriptions.Add(notify);
	}

	void UnsubscribeGesture(IGestureRecognizer recognizer)
	{
		if (recognizer is not INotifyPropertyChanged notify)
			return;

		notify.PropertyChanged -= OnGesturePropertyChanged;
		_gestureSubscriptions.Remove(notify);
	}

	void OnGesturePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.Equals(e.PropertyName, nameof(MauiControls.DropGestureRecognizer.AllowDrop), StringComparison.Ordinal) ||
			string.Equals(e.PropertyName, nameof(MauiControls.DragGestureRecognizer.CanDrag), StringComparison.Ordinal))
		{
			UpdateDropSubscriptions();
		}
	}

	void DetachGestureSubscriptions(bool detachCollectionChanged = true)
	{
		if (detachCollectionChanged && _gestureNotifier != null)
			_gestureNotifier.CollectionChanged -= OnGesturesChanged;

		foreach (var notify in _gestureSubscriptions)
			notify.PropertyChanged -= OnGesturePropertyChanged;

		_gestureSubscriptions.Clear();
	}
}
