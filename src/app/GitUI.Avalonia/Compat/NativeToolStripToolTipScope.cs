using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;

namespace GitUI.Compat;

/// <summary>
///  Preserves automatic hints on the original item's current main or overflow parent.
/// </summary>
internal sealed class NativeToolStripToolTipScope : IDisposable
{
    private readonly PopupFlyoutBase _flyout;
    private readonly Control _owner;
    private readonly NativeToolStrip? _overflowStrip;
    private readonly long _hoverEpoch;
    private readonly bool _serviceEnabled;
    private IDisposable? _ownerValue;
    private IDisposable? _presenterValue;
    private IDisposable? _overflowTimer;
    private IDisposable? _overflowDismissValue;
    private IDisposable? _overflowPlacementValue;
    private bool _disposed;

    internal NativeToolStripToolTipScope(Control owner, PopupFlyoutBase flyout)
    {
        _owner = owner;
        _flyout = flyout;
        _serviceEnabled = AvaloniaToolTip.GetServiceEnabled(owner);
        _flyout.Opened += OnOpened;
        if (owner.Parent is NativeToolStrip strip && ReferenceEquals(strip.GetCurrentParent(owner), strip.OverflowContent))
        {
            // ToolStripEx freezes the logical main HWND, not this item's still-visible
            // ToolStripOverflow parent. Its native parent refreshes the owner's hint.
            _overflowStrip = strip;
            _hoverEpoch = strip.GetToolTipHoverEpoch(owner);

            // ToolStripDropDownItem cascades from its current dropdown parent,
            // rather than retaining the main toolbar's below-owner direction.
            // The framework still applies its normal screen-edge constraints.
            _overflowPlacementValue = flyout.Popup.SetValue(Popup.PlacementProperty,
                owner.FlowDirection == Avalonia.Media.FlowDirection.RightToLeft ? PlacementMode.LeftEdgeAlignedTop : PlacementMode.RightEdgeAlignedTop,
                BindingPriority.Animation);

            // This is an owned submenu of the visible overflow. As with Avalonia's
            // MenuItem submenu, its parent popup owns outside dismissal. A second
            // light-dismiss overlay inside an offset overlay host would intercept
            // the real parent-row pointer route at the wrong root coordinates.
            _overflowDismissValue = flyout.Popup.SetValue(Popup.IsLightDismissEnabledProperty, false, BindingPriority.Animation);
            strip.OverflowContent.AddHandler(InputElement.PointerPressedEvent, OverflowParentPointerPressed, RoutingStrategies.Tunnel);
            return;
        }

        // An owned priority lease leaves caller values and bindings intact. Turning
        // the automatic service off also cancels a tooltip's already-pending timer;
        // explicit ToolTip.Show/SetIsOpen requests remain supported, as in WinForms.
        _ownerValue = owner.SetValue(AvaloniaToolTip.ServiceEnabledProperty, false, BindingPriority.Animation);
    }

    internal bool SuppressesOwnerHints => _overflowStrip is null;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _flyout.Opened -= OnOpened;
        _overflowStrip?.OverflowContent.RemoveHandler(InputElement.PointerPressedEvent, OverflowParentPointerPressed);
        _overflowTimer?.Dispose();
        _overflowTimer = null;
        IDisposable? overflowPlacementValue = _overflowPlacementValue;
        _overflowPlacementValue = null;
        overflowPlacementValue?.Dispose();
        IDisposable? overflowDismissValue = _overflowDismissValue;
        _overflowDismissValue = null;
        overflowDismissValue?.Dispose();
        IDisposable? presenterValue = _presenterValue;
        _presenterValue = null;
        presenterValue?.Dispose();
        IDisposable? ownerValue = _ownerValue;
        _ownerValue = null;
        ownerValue?.Dispose();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (_overflowStrip is not null)
        {
            if (CanShowOverflowHint())
            {
                TimeSpan delay = _overflowStrip.GetOverflowToolTipDelay(_owner);

                // Raw mouse-down's automatic service cancellation runs after the
                // routed handler opens this flyout. Even an already-active native
                // hint must refresh after that event, not be erased within it.
                _overflowTimer = DispatcherTimer.RunOnce(ShowOverflowHint, delay);
            }

            return;
        }

        if (!_disposed && _flyout.Popup.Child is Control presenter && !presenter.IsSet(AvaloniaToolTip.ServiceEnabledProperty))
        {
            // A popup is a distinct native hover surface. Do not let the owner's
            // inherited suspension disable its rows, or replace a caller's own value.
            IDisposable? value = presenter.SetValue(AvaloniaToolTip.ServiceEnabledProperty, _serviceEnabled, BindingPriority.Style);
            if (_disposed)
            {
                value?.Dispose();
            }
            else
            {
                _presenterValue = value;
            }
        }
    }

    private bool CanShowOverflowHint()
        => !_disposed && _flyout.IsOpen && ReferenceEquals(_flyout.Target, _owner)
            && _overflowStrip?.IsOverflowOpen == true
            && ReferenceEquals(_owner.Parent, _overflowStrip)
            && ReferenceEquals(_overflowStrip.GetCurrentParent(_owner), _overflowStrip.OverflowContent)
            && _hoverEpoch != 0 && _overflowStrip.GetToolTipHoverEpoch(_owner) == _hoverEpoch
            && _owner.IsPointerOver && _owner.IsEffectivelyVisible && _owner.IsEffectivelyEnabled
            && AvaloniaToolTip.GetServiceEnabled(_owner) && AvaloniaToolTip.GetTip(_owner) is not null;

    private void OverflowParentPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_disposed && _flyout.IsOpen && ReferenceEquals(_flyout.Target, _owner)
            && _overflowStrip?.IsOverflowOpen == true
            && ReferenceEquals(_owner.Parent, _overflowStrip)
            && ReferenceEquals(_overflowStrip.GetCurrentParent(_owner), _overflowStrip.OverflowContent)
            && e.Source is Avalonia.Visual source
            && (ReferenceEquals(source, _overflowStrip.OverflowContent) || _overflowStrip.OverflowContent.IsVisualAncestorOf(source))
            && !ReferenceEquals(source, _owner) && !_owner.IsVisualAncestorOf(source)
            && e.GetCurrentPoint(_overflowStrip.OverflowContent).Properties.IsLeftButtonPressed)
        {
            // Native blank-parent and sibling presses retire only this child.
            // Keep the event untouched: a real sibling command still receives it,
            // and its normal click may close the complete overflow chain.
            _flyout.Hide();
        }
    }

    private void ShowOverflowHint()
    {
        _overflowTimer = null;
        if (CanShowOverflowHint())
        {
            // Avalonia cancels automatic hints on raw mouse-down. The original overflow
            // parent's hover/refresh survives that route; keep its remaining real delay.
            _owner.SetCurrentValue(AvaloniaToolTip.IsOpenProperty, true);
        }
    }
}
