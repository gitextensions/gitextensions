using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>Adapts NativeTreeView's horizontal wheel and vertical-only visibility routes.</summary>
internal sealed class NativeTreeScrollAdapter
{
    // The real native TREEVIEW at 96 DPI advances five pixels per SB_LINE and uses
    // viewport minus that line for SB_PAGE, independently of its 9/11pt control font.
    internal const int NativeHorizontalLine = 5;
    private const int PortableWheelScrollLines = 3;
    private readonly TreeView _tree;
    private readonly Func<int> _getWheelScrollLines;

    internal NativeTreeScrollAdapter(TreeView tree, Func<int>? getWheelScrollLines = null)
    {
        _tree = tree;
        _getWheelScrollLines = getWheelScrollLines ?? GetWheelScrollLines;
        tree.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel);
    }

    internal bool ScrollHorizontally(Vector delta, KeyModifiers modifiers)
    {
        bool shifted = modifiers.HasFlag(KeyModifiers.Shift);
        if (delta.X == 0 && !shifted)
        {
            return false;
        }

        // Avalonia Win32, X11 and Wayland normalize positive X to left. WinForms'
        // WM_MOUSEHWHEEL is the opposite sign; Shift+positive Y likewise scrolls left.
        double wheel = delta.X != 0 ? -delta.X : -delta.Y;
        if (wheel == 0 || GetScrollViewer() is not { } scroll)
        {
            return true;
        }

        int lines = _getWheelScrollLines();
        double distance = lines == -1
            ? Math.Max(0, scroll.Viewport.Width - NativeHorizontalLine)
            : Math.Max(1, (int)(Math.Abs(wheel) * lines * 3)) * NativeHorizontalLine;
        double next = Math.Clamp(scroll.Offset.X + (Math.Sign(wheel) * distance), 0, Math.Max(0, scroll.Extent.Width - scroll.Viewport.Width));
        scroll.Offset = new Vector(next, scroll.Offset.Y);
        return true;
    }

    internal void EnsureVerticallyVisible(TreeViewItem item)
    {
        item.BringIntoView();
        ScrollLeftMost();
        if (TopLevel.GetTopLevel(_tree) is not null)
        {
            // Expansion can realize the target only on the next layout pass. Keep the
            // source's final SB_LEFT after that framework-required deferred realization.
            Dispatcher.UIThread.Post(() =>
            {
                if (item.FindAncestorOfType<TreeView>() == _tree)
                {
                    item.BringIntoView();
                    ScrollLeftMost();
                }
            }, DispatcherPriority.Loaded);
        }
    }

    internal void ScrollLeftMost()
    {
        if (GetScrollViewer() is { } scroll)
        {
            scroll.Offset = new Vector(0, scroll.Offset.Y);
        }
    }

    internal static int GetWheelScrollLines()
    {
        // Windows exposes the source setting directly. Avalonia's platform settings
        // provide no cross-desktop wheel-lines preference; other platforms use the
        // documented three-line substitute, rather than guessing a desktop's setting.
        return OperatingSystem.IsWindows() && SystemParametersInfo(0x68, 0, out uint lines, 0)
            ? unchecked((int)lines) : PortableWheelScrollLines;
    }

    private ScrollViewer? GetScrollViewer()
        => _tree.GetVisualDescendants().OfType<ScrollViewer>()
            .FirstOrDefault(scroll => scroll.FindAncestorOfType<TreeView>() == _tree);

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ScrollHorizontally(e.Delta, e.KeyModifiers))
        {
            e.Handled = true;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, out uint value, uint flags);
}
