using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using GitExtensions.ParityCapture;

namespace GitExtensionsTests;

internal sealed class AvaloniaControlStateDriver : IDisposable
{
    private readonly List<Action> _restoreActions = [];
    private readonly TopLevel _topLevel;

    private AvaloniaControlStateDriver(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    public bool RequiresExternalSurfaceCapture { get; private set; }

    public static AvaloniaControlStateDriver Apply(Control root, CaptureStatePlan state)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(root)
            ?? throw new AvaloniaCaptureStateUnsupportedException("The control is not attached to a headless top level.");
        AvaloniaControlStateDriver driver = new(topLevel);
        object? target = state.TargetField is null ? root : FindFieldValue(root, state.TargetField);
        if (target is null)
        {
            throw new AvaloniaCaptureStateUnsupportedException($"Field '{state.TargetField}' was not found.");
        }

        switch (state.Kind)
        {
            case CaptureStateKind.Normal:
                break;
            case CaptureStateKind.Focus:
                driver.Focus(target);
                break;
            case CaptureStateKind.Disabled:
                driver.Disable(target);
                break;
            case CaptureStateKind.Checked:
                driver.Check(target);
                break;
            case CaptureStateKind.Expanded:
                driver.Expand(target);
                break;
            case CaptureStateKind.Hover:
                driver.Hover(target);
                break;
            case CaptureStateKind.Pressed:
                driver.Press(target);
                break;
            case CaptureStateKind.MenuOpen:
                driver.OpenMenu(target);
                break;
            default:
                throw new AvaloniaCaptureStateUnsupportedException($"State kind '{state.Kind}' is not implemented.");
        }

        Dispatcher.UIThread.RunJobs();
        return driver;
    }

    public void Dispose()
    {
        for (int i = _restoreActions.Count - 1; i >= 0; i--)
        {
            _restoreActions[i]();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private static object? FindFieldValue(Control root, string fieldName)
    {
        for (Type? type = root.GetType(); type is not null; type = type.BaseType)
        {
            FieldInfo? field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field.GetValue(root);
            }
        }

        return EnumerateLogicalControls(root).FirstOrDefault(control => control.Name == fieldName);
    }

    private static IEnumerable<Control> EnumerateLogicalControls(Control root)
    {
        yield return root;
        foreach (Control child in root.GetLogicalChildren().OfType<Control>())
        {
            foreach (Control descendant in EnumerateLogicalControls(child))
            {
                yield return descendant;
            }
        }
    }

    private void Check(object target)
    {
        if (target is not ToggleButton toggle)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The checked state requires a ToggleButton.");
        }

        bool? previous = toggle.IsChecked;
        toggle.IsChecked = toggle.IsThreeState ? null : true;
        _restoreActions.Add(() => toggle.IsChecked = previous);
    }

    private void Disable(object target)
    {
        if (target is not Control control)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The disabled state requires a Control.");
        }

        bool previous = control.IsEnabled;
        control.IsEnabled = false;
        _restoreActions.Add(() => control.IsEnabled = previous);
    }

    private void Expand(object target)
    {
        Control? control = target as Control;
        TreeViewItem? treeItem = control as TreeViewItem
            ?? (control is null ? null : EnumerateLogicalControls(control).OfType<TreeViewItem>().FirstOrDefault());
        if (treeItem is not null)
        {
            bool previous = treeItem.IsExpanded;
            treeItem.IsExpanded = true;
            _restoreActions.Add(() => treeItem.IsExpanded = previous);
            return;
        }

        Expander? expander = control as Expander
            ?? (control is null ? null : EnumerateLogicalControls(control).OfType<Expander>().FirstOrDefault());
        if (expander is null)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The expanded state requires a populated TreeViewItem or Expander.");
        }

        bool wasExpanded = expander.IsExpanded;
        expander.IsExpanded = true;
        _restoreActions.Add(() => expander.IsExpanded = wasExpanded);
    }

    private void Focus(object target)
    {
        if (target is not Control { Focusable: true } control)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The focused state requires a focusable Control.");
        }

        control.Focus();
        Dispatcher.UIThread.RunJobs();
        if (!control.IsFocused)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The headless focus manager did not focus the requested Control.");
        }
    }

    private void Hover(object target)
    {
        Control control = RequireVisibleControl(target, "hover");
        Point point = GetCenter(control);
        _topLevel.MouseMove(point, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        if (!control.IsPointerOver)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The headless pointer did not enter the requested Control.");
        }

        _restoreActions.Add(() => _topLevel.MouseMove(new Point(-1, -1), RawInputModifiers.None));
    }

    private void OpenMenu(object target)
    {
        MenuItem? menuItem = target switch
        {
            MenuItem direct => direct,
            Control control => EnumerateLogicalControls(control).OfType<MenuItem>().FirstOrDefault(),
            _ => null
        };
        if (menuItem is null)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The open-menu state requires a Menu or MenuItem.");
        }

        bool previous = menuItem.IsSubMenuOpen;
        menuItem.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        if (!menuItem.IsSubMenuOpen)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The headless menu did not open.");
        }

        RequiresExternalSurfaceCapture = true;
        _restoreActions.Add(() => menuItem.IsSubMenuOpen = previous);
    }

    private void Press(object target)
    {
        Control control = RequireVisibleControl(target, "pressed");
        Point point = GetCenter(control);
        _topLevel.MouseMove(point, RawInputModifiers.None);
        _topLevel.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        _restoreActions.Add(() =>
        {
            _topLevel.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            _topLevel.MouseMove(new Point(-1, -1), RawInputModifiers.None);
        });
    }

    private static Control RequireVisibleControl(object target, string stateName)
    {
        if (target is not Control { IsVisible: true } control || control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
        {
            throw new AvaloniaCaptureStateUnsupportedException($"The {stateName} state requires a visible Control with layout bounds.");
        }

        return control;
    }

    private Point GetCenter(Control control)
    {
        Point localCenter = new(control.Bounds.Width / 2, control.Bounds.Height / 2);
        return control.TranslatePoint(localCenter, _topLevel)
            ?? throw new AvaloniaCaptureStateUnsupportedException("The requested Control cannot be translated to headless coordinates.");
    }
}

internal sealed class AvaloniaCaptureStateUnsupportedException(string message) : Exception(message);
