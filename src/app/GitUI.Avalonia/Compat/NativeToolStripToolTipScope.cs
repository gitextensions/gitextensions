using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using AvaloniaToolTip = Avalonia.Controls.ToolTip;

namespace GitUI.Compat;

/// <summary>
///  Suspends only automatic owner hints while ToolStripEx's source HWND cannot receive hover.
/// </summary>
internal sealed class NativeToolStripToolTipScope : IDisposable
{
    private readonly PopupFlyoutBase _flyout;
    private readonly bool _serviceEnabled;
    private IDisposable? _ownerValue;
    private IDisposable? _presenterValue;
    private bool _disposed;

    internal NativeToolStripToolTipScope(Control owner, PopupFlyoutBase flyout)
    {
        _flyout = flyout;
        _serviceEnabled = AvaloniaToolTip.GetServiceEnabled(owner);
        _flyout.Opened += OnOpened;

        // An owned priority lease leaves caller values and bindings intact. Turning
        // the automatic service off also cancels a tooltip's already-pending timer;
        // explicit ToolTip.Show/SetIsOpen requests remain supported, as in WinForms.
        _ownerValue = owner.SetValue(AvaloniaToolTip.ServiceEnabledProperty, false, BindingPriority.Animation);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _flyout.Opened -= OnOpened;
        IDisposable? presenterValue = _presenterValue;
        _presenterValue = null;
        presenterValue?.Dispose();
        IDisposable? ownerValue = _ownerValue;
        _ownerValue = null;
        ownerValue?.Dispose();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
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
}
