using System.Reflection;

using GitExtensions.ParityCapture;
using GitUI.AutoCompletion;
using GitUI.Editor;

namespace WinFormsParityCapture;

internal sealed class ControlStateDriver : IDisposable
{
    private readonly List<Action> _restoreActions = [];
    private readonly List<ToolStripDropDown> _popups = [];
    private readonly List<ComboBoxPopup> _comboBoxPopups = [];
    private readonly Control _root;

    private ControlStateDriver(Control root)
    {
        _root = root;
    }

    public IReadOnlyList<ToolStripDropDown> Popups => _popups;

    public IReadOnlyList<ComboBoxPopup> ComboBoxPopups => _comboBoxPopups;

    public bool RequiresScreenGrab => _popups.Count > 0 || _comboBoxPopups.Count > 0;

    public static ControlStateDriver Apply(Control root, CaptureStatePlan state)
    {
        ControlStateDriver driver = new(root);
        driver.ApplyRequestedSize(state);
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

    private void ApplyRequestedSize(CaptureStatePlan state)
    {
        if (state.WidthDip is not int widthDip || state.HeightDip is not int heightDip)
        {
            return;
        }

        Size originalSize = _root is Form originalForm ? originalForm.ClientSize : _root.Size;
        int dpi = _root.DeviceDpi;
        Size requestedSize = new(
            (int)Math.Round(widthDip * dpi / 96d, MidpointRounding.AwayFromZero),
            (int)Math.Round(heightDip * dpi / 96d, MidpointRounding.AwayFromZero));
        if (_root is Form form)
        {
            form.ClientSize = requestedSize;
        }
        else
        {
            _root.Size = requestedSize;
        }

        PumpEvents();
        _restoreActions.Add(() =>
        {
            if (_root is Form restoredForm)
            {
                restoredForm.ClientSize = originalSize;
            }
            else
            {
                _root.Size = originalSize;
            }
        });
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

        if (owner is Control root)
        {
            foreach (Control control in EnumerateSelfAndDescendants(root))
            {
                if (control.Name == fieldName)
                {
                    return control;
                }

                if (control is ToolStrip toolStrip
                    && FindToolStripItem(toolStrip.Items, fieldName) is ToolStripItem item)
                {
                    return item;
                }
            }
        }

        return null;

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

        static ToolStripItem? FindToolStripItem(ToolStripItemCollection items, string name)
        {
            foreach (ToolStripItem item in items)
            {
                if (item.Name == name)
                {
                    return item;
                }

                if (item is ToolStripDropDownItem dropDownItem
                    && FindToolStripItem(dropDownItem.DropDownItems, name) is ToolStripItem child)
                {
                    return child;
                }
            }

            return null;
        }
    }

    private static void PumpEvents()
    {
        Application.DoEvents();
        Thread.Sleep(75);
        Application.DoEvents();
    }

    private void Check(object target)
    {
        if (target is RadioButton radioButton)
        {
            bool radioPrevious = radioButton.Checked;
            radioButton.Checked = true;
            _restoreActions.Add(() => radioButton.Checked = radioPrevious);
            return;
        }

        CheckBox? checkBox = target as CheckBox;
        if (checkBox is null && target is Control composite)
        {
            CheckBox[] descendants = EnumerateDescendants(composite).OfType<CheckBox>().Take(2).ToArray();
            if (descendants.Length == 1)
            {
                checkBox = descendants[0];
            }
        }

        if (checkBox is null)
        {
            throw new CaptureStateUnsupportedException(
                "The checked state requires a CheckBox, RadioButton, or a composite control with one CheckBox.");
        }

        CheckState previous = checkBox.CheckState;
        checkBox.CheckState = checkBox.ThreeState ? CheckState.Indeterminate : CheckState.Checked;
        _restoreActions.Add(() => checkBox.CheckState = previous);

        static IEnumerable<Control> EnumerateDescendants(Control control)
        {
            foreach (Control child in control.Controls)
            {
                yield return child;
                foreach (Control descendant in EnumerateDescendants(child))
                {
                    yield return descendant;
                }
            }
        }
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
        if (target is not Control control)
        {
            throw new CaptureStateUnsupportedException("The focused state requires a focusable Control.");
        }

        ActivateContainingTabs(control);
        _ = control.Handle;
        Form? form = control.FindForm();
        form?.Activate();
        PumpEvents();
        Control? previous = form?.ActiveControl;
        control.Select();
        if (form is not null)
        {
            form.ActiveControl = control;
        }

        control.Focus();
        PumpEvents();
        if (!control.Focused && !control.ContainsFocus)
        {
            NativeMethods.FocusWindow(control.Handle);
            PumpEvents();
        }

        if (!control.Focused && !control.ContainsFocus)
        {
            throw new CaptureStateUnsupportedException("WinForms did not focus the requested Control.");
        }

        _restoreActions.Add(() => previous?.Focus());
    }

    private void ActivateContainingTabs(Control control)
    {
        TabPage[] tabPages = EnumerateParents(control)
            .OfType<TabPage>()
            .Reverse()
            .ToArray();
        foreach (TabPage tabPage in tabPages)
        {
            if (tabPage.Parent is not TabControl tabControl
                || ReferenceEquals(tabControl.SelectedTab, tabPage))
            {
                continue;
            }

            TabPage? previous = tabControl.SelectedTab;
            tabControl.SelectedTab = tabPage;
            PumpEvents();
            _restoreActions.Add(() => tabControl.SelectedTab = previous);
        }

        static IEnumerable<Control> EnumerateParents(Control child)
        {
            for (Control? parent = child.Parent; parent is not null; parent = parent.Parent)
            {
                yield return parent;
            }
        }
    }

    private void Hover(object target)
    {
        if (target is not Control control || !control.IsHandleCreated)
        {
            throw new CaptureStateUnsupportedException("The hover state requires a created Control handle.");
        }

        (Control mouseTarget, Point mousePoint) = FindMouseTarget(control);
        Point originalCursorPosition = NativeMethods.GetCursorPosition();
        NativeMethods.SetCursorPosition(mouseTarget.PointToScreen(mousePoint));
        NativeMethods.SendMouseMessage(mouseTarget.Handle, NativeMethods.WmMouseMove, mousePoint.X, mousePoint.Y);
        _restoreActions.Add(() =>
        {
            NativeMethods.SendMouseMessage(mouseTarget.Handle, NativeMethods.WmMouseLeave, 0, 0);
            NativeMethods.SetCursorPosition(originalCursorPosition);
        });
    }

    private static (Control Control, Point Point) FindMouseTarget(Control control)
    {
        if (control is FileViewerInternal
            && FindFieldValue(control, "TextEditor") is object textEditor
            && textEditor.GetType().GetProperty("ActiveTextAreaControl")?.GetValue(textEditor) is object textAreaControl
            && textAreaControl.GetType().GetProperty("TextArea")?.GetValue(textAreaControl) is Control textArea)
        {
            // parity-scaffolding: This legacy composite republishes mouse events only from the
            // inner text area, which is the native child window Windows actually hit-tests.
            return (textArea, new Point(
                Math.Max(1, textArea.ClientSize.Width / 2),
                Math.Max(1, textArea.ClientSize.Height / 2)));
        }

        Control current = control;
        Point point = new(Math.Max(1, control.ClientSize.Width / 2), Math.Max(1, control.ClientSize.Height / 2));
        const GetChildAtPointSkip skip = GetChildAtPointSkip.Invisible
                                         | GetChildAtPointSkip.Disabled
                                         | GetChildAtPointSkip.Transparent;
        while (current.GetChildAtPoint(point, skip) is { IsHandleCreated: true } child)
        {
            point.Offset(-child.Left, -child.Top);
            current = child;
        }

        return (current, point);
    }

    private void OpenMenu(object target)
    {
        if (target is ListBox { Name: "AutoComplete" } autoComplete)
        {
            OpenAutoComplete(autoComplete);
            return;
        }

        if (target is ToolStripComboBox toolStripComboBox)
        {
            target = toolStripComboBox.ComboBox;
        }

        if (target is ComboBox comboBox)
        {
            if (!comboBox.IsHandleCreated || comboBox.Items.Count == 0)
            {
                throw new CaptureStateUnsupportedException("The ComboBox popup requires a created, populated control.");
            }

            bool previous = comboBox.DroppedDown;
            comboBox.DroppedDown = true;
            PumpEvents();
            if (!comboBox.DroppedDown)
            {
                throw new CaptureStateUnsupportedException("The requested ComboBox popup declined to open.");
            }

            Rectangle bounds = NativeMethods.GetComboBoxListRectangle(comboBox.Handle);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                throw new CaptureStateUnsupportedException("The native ComboBox popup has no drawable area.");
            }

            _comboBoxPopups.Add(new ComboBoxPopup(comboBox, bounds));
            _restoreActions.Add(() => comboBox.DroppedDown = previous);
            return;
        }

        // parity-scaffolding: Capture the grid-owned ContextMenuStrip through its real popup surface.
        ToolStripDropDown popup = target switch
        {
            ContextMenuStrip contextMenu => OpenContextMenu(contextMenu, _root),
            ToolStripDropDownItem item => Open(item),
            MenuStrip menu when menu.Items.OfType<ToolStripDropDownItem>().FirstOrDefault() is { } item => Open(item),
            _ => throw new CaptureStateUnsupportedException("The open-menu state requires a MenuStrip or ToolStripDropDownItem.")
        };
        if (!popup.Visible)
        {
            throw new CaptureStateUnsupportedException("The requested popup declined to open in the current control state.");
        }

        _popups.Add(popup);
        _restoreActions.Add(popup.Close);

        static ToolStripDropDown Open(ToolStripDropDownItem item)
        {
            item.ShowDropDown();
            return item.DropDown;
        }

        static ToolStripDropDown OpenContextMenu(ContextMenuStrip contextMenu, Control root)
        {
            Point screenLocation = root.PointToScreen(
                new Point(Math.Max(1, root.ClientSize.Width / 2), Math.Max(1, root.ClientSize.Height / 2)));
            try
            {
                contextMenu.Show(screenLocation);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "revision")
            {
                contextMenu.Close();
                throw new CaptureStateNotReadyException(
                    $"The original revision-grid context menu observed a replaced selection while opening ({ex.Message}).");
            }

            return contextMenu;
        }
    }

    private void OpenAutoComplete(ListBox autoComplete)
    {
        RichTextBox? textBox = autoComplete.Parent?.Controls.Find("TextBox", searchAllChildren: true).OfType<RichTextBox>().FirstOrDefault();
        if (textBox is null)
        {
            throw new CaptureStateUnsupportedException("The autocomplete state requires the EditNetSpell text box.");
        }

        bool previousVisible = autoComplete.Visible;
        object? previousDataSource = autoComplete.DataSource;
        Rectangle previousBounds = autoComplete.Bounds;
        int previousSelectedIndex = autoComplete.SelectedIndex;
        string previousText = textBox.Text;
        int previousSelectionStart = textBox.SelectionStart;
        int previousSelectionLength = textBox.SelectionLength;

        textBox.Text = "Br";
        textBox.Select(textBox.TextLength, 0);
        List<AutoCompleteWord> words =
        [
            new("BranchParser"),
            new("BranchPolicy"),
        ];
        List<Size> sizes = [.. words.Select(word => TextRenderer.MeasureText(word.Word, textBox.Font))];
        Point cursorPosition = textBox.GetPositionFromCharIndex(textBox.SelectionStart);
        cursorPosition.Y += (int)Math.Ceiling(textBox.Font.GetHeight());
        cursorPosition.X += 2;

        int top = cursorPosition.Y;
        int height = (sizes.Count + 1) * autoComplete.ItemHeight;
        int width = sizes.Max(size => size.Width);
        if (top + height > textBox.Height)
        {
            if (textBox.Height - top > textBox.Height / 2)
            {
                height = textBox.Height - top;
            }
            else
            {
                top = Math.Max(0, textBox.Height - height);
                height = Math.Min(textBox.Height - top, height);
            }

            width += SystemInformation.VerticalScrollBarWidth;
        }

        autoComplete.SetBounds(cursorPosition.X, top, width, height);
        autoComplete.DataSource = words;
        autoComplete.SelectedIndex = 0;
        autoComplete.Show();
        textBox.Focus();

        _restoreActions.Add(() =>
        {
            autoComplete.Hide();
            autoComplete.DataSource = previousDataSource;
            autoComplete.Bounds = previousBounds;
            if (previousDataSource is not null && previousSelectedIndex >= 0)
            {
                autoComplete.SelectedIndex = previousSelectedIndex;
            }

            autoComplete.Visible = previousVisible;
            textBox.Text = previousText;
            int restoredSelectionStart = Math.Min(previousSelectionStart, textBox.TextLength);
            textBox.Select(restoredSelectionStart, Math.Min(previousSelectionLength, textBox.TextLength - restoredSelectionStart));
        });
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

internal sealed record ComboBoxPopup(ComboBox Owner, Rectangle Bounds);

internal class CaptureStateUnsupportedException(string message) : Exception(message);

internal sealed class CaptureStateNotReadyException(string message) : CaptureStateUnsupportedException(message);
