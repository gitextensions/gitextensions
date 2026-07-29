using System.Reflection;

namespace WinFormsParityCapture;

internal sealed class ControlStateDriver : IDisposable
{
    private readonly List<Action> _restoreActions = [];
    private readonly List<ToolStripDropDown> _popups = [];

    private ControlStateDriver()
    {
    }

    public IReadOnlyList<ToolStripDropDown> Popups => _popups;

    public bool RequiresScreenGrab => _popups.Count > 0;

    public static ControlStateDriver Apply(Control root, CaptureStatePlan state)
    {
        ControlStateDriver driver = new();
        object? target = state.TargetField is null ? root : FindFieldValue(root, state.TargetField);
        if (target is null)
        {
            throw new CaptureStateUnsupportedException($"Field '{state.TargetField}' was not found.");
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
                throw new CaptureStateUnsupportedException($"State kind '{state.Kind}' is not implemented.");
        }

        PumpEvents();
        return driver;
    }

    public void Dispose()
    {
        for (int i = _restoreActions.Count - 1; i >= 0; i--)
        {
            _restoreActions[i]();
        }

        PumpEvents();
    }

    private static object? FindFieldValue(object owner, string fieldName)
    {
        for (Type? type = owner.GetType(); type is not null; type = type.BaseType)
        {
            FieldInfo? field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field.GetValue(owner);
            }
        }

        return null;
    }

    private static void PumpEvents()
    {
        Application.DoEvents();
        Thread.Sleep(75);
        Application.DoEvents();
    }

    private void Check(object target)
    {
        if (target is not CheckBox checkBox)
        {
            throw new CaptureStateUnsupportedException("The checked state requires a CheckBox.");
        }

        CheckState previous = checkBox.CheckState;
        checkBox.CheckState = checkBox.ThreeState ? CheckState.Indeterminate : CheckState.Checked;
        _restoreActions.Add(() => checkBox.CheckState = previous);
    }

    private void Disable(object target)
    {
        if (target is not Control control)
        {
            throw new CaptureStateUnsupportedException("The disabled state requires a Control.");
        }

        bool previous = control.Enabled;
        control.Enabled = false;
        _restoreActions.Add(() => control.Enabled = previous);
    }

    private void Expand(object target)
    {
        TreeView? treeView = target as TreeView
            ?? (target as Control)?.Controls
                .Cast<Control>()
                .SelectMany(EnumerateSelfAndDescendants)
                .OfType<TreeView>()
                .FirstOrDefault(candidate => candidate.Nodes.Count > 0);
        if (treeView is null || treeView.Nodes.Count == 0)
        {
            throw new CaptureStateUnsupportedException("The expanded state requires a populated TreeView.");
        }

        TreeNode node = treeView.Nodes[0];
        bool wasExpanded = node.IsExpanded;
        node.Expand();
        _restoreActions.Add(() =>
        {
            if (!wasExpanded)
            {
                node.Collapse();
            }
        });

        static IEnumerable<Control> EnumerateSelfAndDescendants(Control control)
        {
            yield return control;
            foreach (Control child in control.Controls)
            {
                foreach (Control descendant in EnumerateSelfAndDescendants(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    private void Focus(object target)
    {
        if (target is not Control control || !control.CanFocus)
        {
            throw new CaptureStateUnsupportedException("The focused state requires a focusable Control.");
        }

        Control? previous = control.FindForm()?.ActiveControl;
        control.Focus();
        _restoreActions.Add(() => previous?.Focus());
    }

    private void Hover(object target)
    {
        if (target is not Control control || !control.IsHandleCreated)
        {
            throw new CaptureStateUnsupportedException("The hover state requires a created Control handle.");
        }

        NativeMethods.SendMouseMessage(control.Handle, NativeMethods.WmMouseMove, Math.Max(1, control.ClientSize.Width / 2), Math.Max(1, control.ClientSize.Height / 2));
        _restoreActions.Add(() => NativeMethods.SendMouseMessage(control.Handle, NativeMethods.WmMouseLeave, 0, 0));
    }

    private void OpenMenu(object target)
    {
        ToolStripDropDown popup = target switch
        {
            ToolStripDropDownItem item => Open(item),
            MenuStrip menu when menu.Items.OfType<ToolStripDropDownItem>().FirstOrDefault() is { } item => Open(item),
            _ => throw new CaptureStateUnsupportedException("The open-menu state requires a MenuStrip or ToolStripDropDownItem.")
        };
        _popups.Add(popup);
        _restoreActions.Add(popup.Close);

        static ToolStripDropDown Open(ToolStripDropDownItem item)
        {
            item.ShowDropDown();
            return item.DropDown;
        }
    }

    private void Press(object target)
    {
        if (target is not ButtonBase button || !button.IsHandleCreated)
        {
            throw new CaptureStateUnsupportedException("The pressed state requires a created ButtonBase handle.");
        }

        NativeMethods.SendMouseMessage(button.Handle, NativeMethods.WmMouseMove, Math.Max(1, button.ClientSize.Width / 2), Math.Max(1, button.ClientSize.Height / 2));
        NativeMethods.SendMouseMessage(button.Handle, NativeMethods.WmLButtonDown, Math.Max(1, button.ClientSize.Width / 2), Math.Max(1, button.ClientSize.Height / 2));
        _restoreActions.Add(() =>
        {
            NativeMethods.SendMouseMessage(button.Handle, NativeMethods.WmCancelMode, 0, 0);
            button.Capture = false;
        });
    }
}

internal sealed class CaptureStateUnsupportedException(string message) : Exception(message);
