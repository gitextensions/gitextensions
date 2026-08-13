using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.AutoCompletion;
using GitUI.SpellChecker;

namespace GitExtensionsTests;

internal sealed class AvaloniaControlStateDriver : IDisposable
{
    private readonly List<TopLevel> _externalTopLevels = [];
    private readonly List<Control> _popupSurfaceRoots = [];
    private readonly List<Action> _restoreActions = [];
    private readonly Control _root;
    private readonly TopLevel _topLevel;

    private AvaloniaControlStateDriver(Control root, TopLevel topLevel)
    {
        _root = root;
        _topLevel = topLevel;
    }

    public IReadOnlyList<TopLevel> ExternalTopLevels => _externalTopLevels;

    public IReadOnlyList<Control> PopupSurfaceRoots => _popupSurfaceRoots;

    public bool RequiresExternalSurfaceCapture => _externalTopLevels.Count > 0;

    public static AvaloniaControlStateDriver Apply(Control root, CaptureStatePlan state)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(root)
            ?? throw new AvaloniaCaptureStateUnsupportedException("The control is not attached to a headless top level.");
        AvaloniaControlStateDriver driver = new(root, topLevel);
        object? target = state.TargetField is null ? root : FindFieldValue(root, state.TargetField);
        if (target is null)
        {
            throw new AvaloniaCaptureStateUnsupportedException($"Field '{state.TargetField}' was not found.");
        }

        target = ResolveFrameworkSplitTarget(root, target);

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

    // parity-scaffolding: The original FileStatusList has one FileStatusListView while the
    // Avalonia twin uses separate list/tree visuals; drive whichever native visual owns that state.
    private static object ResolveFrameworkSplitTarget(Control root, object target)
    {
        if (target is Control { Name: "FileStatusListView", IsEffectivelyVisible: false }
            && EnumerateLogicalControls(root).FirstOrDefault(
                control => control.Name == "tvDiffFiles" && control.IsEffectivelyVisible) is Control activeDiffTree)
        {
            return activeDiffTree;
        }

        return target;
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
        foreach (Control owner in EnumerateFieldControls(root))
        {
            for (Type? type = owner.GetType(); type is not null; type = type.BaseType)
            {
                FieldInfo? field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field is not null)
                {
                    return field.GetValue(owner);
                }
            }
        }

        return EnumerateLogicalControls(root).FirstOrDefault(control => control.Name == fieldName);
    }

    private static IEnumerable<Control> EnumerateFieldControls(Control root)
    {
        HashSet<Control> visited = new(ReferenceEqualityComparer.Instance);
        return Enumerate(root);

        IEnumerable<Control> Enumerate(Control control)
        {
            if (!visited.Add(control))
            {
                yield break;
            }

            yield return control;
            for (Type? type = control.GetType(); type is not null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.GetValue(control) is not Control child)
                    {
                        continue;
                    }

                    foreach (Control descendant in Enumerate(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }
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
        if (target is not Control control)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The focused state requires a focusable Control.");
        }

        ActivateContainingTabs(control);

        MethodInfo? hostedFocusMethod = control.GetType().GetMethod(
            nameof(InputElement.Focus),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);
        if (!control.Focusable
            && hostedFocusMethod?.ReturnType == typeof(bool)
            && hostedFocusMethod.Invoke(control, null) is true)
        {
            Dispatcher.UIThread.RunJobs();
            if (IsFocusWithin(control))
            {
                return;
            }
        }

        if (!control.Focusable
            && control.IsEffectivelyVisible
            && control.Bounds.Width > 0
            && control.Bounds.Height > 0)
        {
            Point point = GetCenter(control);
            _topLevel.MouseMove(point, RawInputModifiers.None);
            _topLevel.MouseDown(point, MouseButton.Left, RawInputModifiers.None);
            _topLevel.MouseUp(point, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            if (IsFocusWithin(control))
            {
                return;
            }
        }

        Control? focusTarget = control.Focusable && control.IsEffectivelyVisible
            ? control
            : control.GetVisualDescendants()
                .OfType<Control>()
                .FirstOrDefault(candidate => candidate.Focusable && candidate.IsEffectivelyVisible);
        if (focusTarget is null)
        {
            throw new AvaloniaCaptureStateUnsupportedException(
                "The focused state requires a focusable Control or generated focusable descendant.");
        }

        focusTarget.Focus(NavigationMethod.Tab);
        Dispatcher.UIThread.RunJobs();
        if (!IsFocusWithin(control))
        {
            throw new AvaloniaCaptureStateUnsupportedException(
                $"The headless focus manager did not focus the requested {control.GetType().FullName} "
                + $"named '{control.Name}' (visible={control.IsEffectivelyVisible}, enabled={control.IsEffectivelyEnabled}, "
                + $"bounds={control.Bounds}).");
        }
    }

    private bool IsFocusWithin(Control control)
    {
        IInputElement? focusedElement = _topLevel.FocusManager?.GetFocusedElement();
        return ReferenceEquals(focusedElement, control)
            || (focusedElement is Visual focusedVisual
                && focusedVisual.GetVisualAncestors().Contains(control));
    }

    private void ActivateContainingTabs(Control control)
    {
        TabItem[] tabItems = control.GetLogicalAncestors()
            .OfType<TabItem>()
            .Reverse()
            .ToArray();
        foreach (TabItem tabItem in tabItems)
        {
            TabControl? tabControl = tabItem.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault();
            if (tabControl is null || ReferenceEquals(tabControl.SelectedItem, tabItem))
            {
                continue;
            }

            object? previous = tabControl.SelectedItem;
            tabControl.SelectedItem = tabItem;
            Dispatcher.UIThread.RunJobs();
            _restoreActions.Add(() => tabControl.SelectedItem = previous);
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
        if (target is ListBox { Name: "AutoComplete" } && _root is EditNetSpell editNetSpell)
        {
            OpenAutoComplete(editNetSpell);
            return;
        }

        if (target is ContextMenu contextMenu)
        {
            // parity-scaffolding: Exercise the grid-owned ContextMenu through Avalonia's real popup surface.
            Control owner = EnumerateLogicalControls(_root)
                .FirstOrDefault(control => ReferenceEquals(control.ContextMenu, contextMenu))
                ?? throw new AvaloniaCaptureStateUnsupportedException("The ContextMenu is not attached to a control in the captured view.");
            contextMenu.Open(owner);
            Dispatcher.UIThread.RunJobs();
            TrackExternalTopLevels(contextMenu);
            _restoreActions.Add(contextMenu.Close);
            return;
        }

        if (target is Control { ContextMenu: { } attachedContextMenu })
        {
            attachedContextMenu.Open((Control)target);
            Dispatcher.UIThread.RunJobs();
            TrackExternalTopLevels(attachedContextMenu);
            _restoreActions.Add(attachedContextMenu.Close);
            return;
        }

        if (target is Control controlWithFlyout && GetFlyout(controlWithFlyout) is PopupFlyoutBase flyout)
        {
            flyout.ShowAt(controlWithFlyout);
            Dispatcher.UIThread.RunJobs();
            if (!flyout.IsOpen)
            {
                throw new AvaloniaCaptureStateUnsupportedException("The headless flyout did not open.");
            }

            TrackPopup(flyout.Popup);
            _restoreActions.Add(flyout.Hide);
            return;
        }

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

        ContextMenu? owningContextMenu = EnumerateLogicalControls(_root)
            .Select(control => control.ContextMenu)
            .OfType<ContextMenu>()
            .FirstOrDefault(contextMenu => ContainsMenuItem(contextMenu, menuItem));
        if (owningContextMenu is not null && !owningContextMenu.IsOpen)
        {
            Control owner = EnumerateLogicalControls(_root)
                .First(control => ReferenceEquals(control.ContextMenu, owningContextMenu));
            owningContextMenu.Open(owner);
            Dispatcher.UIThread.RunJobs();
            _restoreActions.Add(owningContextMenu.Close);
        }

        if (owningContextMenu is not null
            && (!menuItem.IsEffectivelyVisible || menuItem.Bounds.Width <= 0 || menuItem.Bounds.Height <= 0))
        {
            // parity-scaffolding: Headless OverlayPopupHost clips long menus to the tiny standalone owner;
            // reparent the actual product item into a compact Menu on a taller real owner viewport.
            if (_topLevel is not Window captureWindow)
            {
                throw new AvaloniaCaptureStateUnsupportedException(
                    "The requested menu item needs a taller owner-hosted viewport, but the capture top level is not a Window.");
            }

            double originalHeight = captureWindow.Height;
            int originalIndex = owningContextMenu.Items.IndexOf(menuItem);
            owningContextMenu.Close();
            owningContextMenu.Items.RemoveAt(originalIndex);
            captureWindow.Height = Math.Max(originalHeight, 700);
            object? originalContent = captureWindow.Content;
            if (originalContent is not Control originalControl)
            {
                throw new AvaloniaCaptureStateUnsupportedException(
                    "The requested menu item needs a real owner-hosted menu, but the capture window content is not a Control.");
            }

            Menu captureMenu = new() { ItemsSource = new[] { menuItem } };
            DockPanel.SetDock(captureMenu, Dock.Top);
            DockPanel captureHost = new();
            captureHost.Children.Add(captureMenu);
            captureWindow.Content = null;
            Dispatcher.UIThread.RunJobs();
            captureHost.Children.Add(originalControl);
            captureWindow.Content = captureHost;
            Dispatcher.UIThread.RunJobs();
            captureWindow.CaptureRenderedFrame();
            Dispatcher.UIThread.RunJobs();
            _restoreActions.Add(() =>
            {
                menuItem.IsSubMenuOpen = false;
                captureMenu.ItemsSource = null;
                captureHost.Children.Remove(originalControl);
                captureWindow.Content = originalContent;
                owningContextMenu.Items.Insert(originalIndex, menuItem);
                captureWindow.Height = originalHeight;
            });
        }

        bool previous = menuItem.IsSubMenuOpen;
        menuItem.IsSubMenuOpen = true;
        Dispatcher.UIThread.RunJobs();
        if (!menuItem.IsSubMenuOpen)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The headless menu did not open.");
        }

        if (!menuItem.IsEffectivelyVisible || menuItem.Bounds.Width <= 0 || menuItem.Bounds.Height <= 0)
        {
            throw new AvaloniaCaptureStateUnsupportedException(
                "The requested menu item was not realized in the opened menu, so its popup surface cannot be captured honestly.");
        }

        TrackExternalTopLevels(menuItem);
        _restoreActions.Add(() => menuItem.IsSubMenuOpen = previous);

        static bool ContainsMenuItem(ItemsControl owner, MenuItem expected)
        {
            foreach (object? item in owner.Items)
            {
                if (ReferenceEquals(item, expected)
                    || (item is ItemsControl child && ContainsMenuItem(child, expected)))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static FlyoutBase? GetFlyout(Control control)
        => control switch
        {
            Button button => button.Flyout,
            SplitButton splitButton => splitButton.Flyout,
            _ => null
        };

    private void TrackExternalTopLevels(Control popupOwner)
    {
        TrackPopupHost(popupOwner);

        if (popupOwner is ItemsControl itemsControl)
        {
            foreach (Control subItem in EnumerateMenuItems(itemsControl))
            {
                TrackTopLevel(TopLevel.GetTopLevel(subItem));
            }
        }

        Control[] controls = EnumerateLogicalControls(popupOwner)
            .Concat(popupOwner.GetVisualDescendants().OfType<Control>())
            .ToArray();
        foreach (Control control in controls)
        {
            TrackTopLevel(TopLevel.GetTopLevel(control));
            if (control is Popup popup)
            {
                TrackPopup(popup);
            }
        }

        if (_externalTopLevels.Count == 0)
        {
            if (_popupSurfaceRoots.Count == 0)
            {
                throw new AvaloniaCaptureStateUnsupportedException(
                    "The requested popup opened without a capturable popup host.");
            }
        }

        static IEnumerable<Control> EnumerateMenuItems(ItemsControl owner)
        {
            foreach (Control item in owner.Items.OfType<Control>())
            {
                yield return item;
                if (item is not ItemsControl childItemsControl)
                {
                    continue;
                }

                foreach (Control descendant in EnumerateMenuItems(childItemsControl))
                {
                    yield return descendant;
                }
            }
        }
    }

    // parity-scaffolding: Avalonia exposes popup placement publicly but keeps the created
    // IPopupHost internal; the capture harness needs that actual host, never a fabricated surface.
    private void TrackPopupHost(Control popupOwner)
    {
        FieldInfo? popupField = null;
        for (Type? type = popupOwner.GetType(); type is not null && popupField is null; type = type.BaseType)
        {
            popupField = type.GetField("_popup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        }

        if (popupField?.GetValue(popupOwner) is not Popup popup)
        {
            return;
        }

        TrackPopup(popup);
    }

    private void TrackPopup(Popup popup)
    {
        PropertyInfo? hostProperty = typeof(Popup).GetProperty("Host", BindingFlags.Instance | BindingFlags.NonPublic);
        object? popupHost = hostProperty?.GetValue(popup);
        if (popupHost is TopLevel popupTopLevel)
        {
            TrackTopLevel(popupTopLevel);
        }
        else if (popupHost is Control popupSurfaceRoot
                 && !_popupSurfaceRoots.Contains(popupSurfaceRoot, ReferenceEqualityComparer.Instance))
        {
            _popupSurfaceRoots.Add(popupSurfaceRoot);
        }

        if (popup.Child is Control child)
        {
            TrackTopLevel(TopLevel.GetTopLevel(child));
        }
    }

    private void TrackTopLevel(TopLevel? topLevel)
    {
        if (topLevel is not null
            && !ReferenceEquals(topLevel, _topLevel)
            && !_externalTopLevels.Contains(topLevel, ReferenceEqualityComparer.Instance))
        {
            _externalTopLevels.Add(topLevel);
        }
    }

    private void OpenAutoComplete(EditNetSpell editNetSpell)
    {
        string previousText = editNetSpell.Text;
        int previousCaretIndex = editNetSpell.CaretIndex;
        EditNetSpell.TestAccessor accessor = editNetSpell.GetTestAccessor();

        editNetSpell.Text = "Br";
        editNetSpell.CaretIndex = editNetSpell.Text.Length;
        accessor.ShowAutoCompleteForCapture(
        [
            new AutoCompleteWord("BranchParser"),
            new AutoCompleteWord("BranchPolicy"),
        ]);
        Dispatcher.UIThread.RunJobs();
        if (!accessor.IsAutoCompleteVisible)
        {
            throw new AvaloniaCaptureStateUnsupportedException("The native autocomplete list did not open.");
        }

        _restoreActions.Add(() =>
        {
            accessor.CloseAutoComplete();
            editNetSpell.Text = previousText;
            editNetSpell.CaretIndex = Math.Min(previousCaretIndex, editNetSpell.Text.Length);
        });
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
            Point outside = new(-1, -1);
            _topLevel.MouseUp(outside, MouseButton.Left, RawInputModifiers.None);
            if (_topLevel.IsVisible)
            {
                _topLevel.MouseMove(outside, RawInputModifiers.None);
            }
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
