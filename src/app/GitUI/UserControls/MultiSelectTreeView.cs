#nullable enable

using System.ComponentModel;
using GitExtUtils.GitUI;
using Microsoft;

namespace GitUI.UserControls;

/// <summary>
///  A <see cref="TreeView"/> with basic support for selecting multiple <see cref="TreeNode"/>s.
/// </summary>
public class MultiSelectTreeView : NativeTreeView
{
    private bool _mouseClickHandled = false;
    private TreeNode? _multiselectionStartNode = null;
    private HashSet<TreeNode> _selectedNodes = [];
    private bool _settingFocusedNode = false;
    private TreeNode? _toBeFocusedNode = null;
    private int _updateSuspendCount = 0;

    public event EventHandler? SelectedNodesChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TreeNode? FocusedNode
    {
        get => base.SelectedNode;
        set
        {
            if (base.SelectedNode == value)
            {
                return;
            }

            try
            {
                _settingFocusedNode = true;

                if (value is not null)
                {
                    if (value.TreeView is null)
                    {
                        this.ExpandTopDownTo(value);
                    }

                    if (!_mouseClickHandled)
                    {
                        value.EnsureVerticallyVisible();
                    }
                }

                base.SelectedNode = value;
            }
            finally
            {
                _settingFocusedNode = false;
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TreeNode? SelectedNode
    {
        set
        {
            if (value is null)
            {
                if (_selectedNodes.Count == 0)
                {
                    return;
                }

                _selectedNodes = [];
            }
            else
            {
                bool alreadyFocused = FocusedNode == value;
                bool alreadySelected = _selectedNodes.Count == 1 && _selectedNodes.First() == value;
                if (alreadyFocused && alreadySelected)
                {
                    return;
                }

                if (!alreadySelected)
                {
                    _selectedNodes = [value];
                }

                if (!alreadyFocused)
                {
                    FocusedNode = value;
                }
                else
                {
                    OnSelectionChanged();
                }
            }

            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlySet<TreeNode> SelectedNodes => _selectedNodes;

    public void SetSelectedNodes(HashSet<TreeNode> selectedNodes, TreeNode? focusedNode)
    {
        bool focusedNodeUnchanged = FocusedNode == focusedNode;
        if (_selectedNodes.SequenceEqual(selectedNodes) && focusedNodeUnchanged)
        {
            return;
        }

        _selectedNodes = selectedNodes;
        if (!focusedNodeUnchanged && focusedNode is not null)
        {
            FocusedNode = focusedNode;
        }
        else
        {
            OnSelectionChanged();
        }

        Invalidate();
    }

    public bool UpdateSuspended => _updateSuspendCount > 0;

    public new void BeginUpdate()
    {
        if (++_updateSuspendCount == 1)
        {
            base.BeginUpdate();
            SuspendLayout();
        }
    }

    public new void EndUpdate()
    {
        if (--_updateSuspendCount == 0)
        {
            ResumeLayout();
            base.EndUpdate();
        }
    }

    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
        base.OnAfterSelect(e);

        if (!_settingFocusedNode && !_mouseClickHandled)
        {
            if (ModifierKeys == Keys.Shift && FocusedNode is TreeNode newFocusedNode)
            {
                UpdateSelection(newFocusedNode, replace: true, addRange: true);
            }
            else
            {
                SelectedNode = FocusedNode;
            }

            return;
        }

        OnSelectionChanged();
    }

    protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
    {
        if (_toBeFocusedNode is not null && e.Node != _toBeFocusedNode)
        {
            e.Cancel = true;
        }

        base.OnBeforeSelect(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Keys modifierKeys = ModifierKeys;

        if (e.Button != MouseButtons.Left

            // or other modifier keys than for selection manipulation
            || (modifierKeys | Keys.Control | Keys.Shift) != (Keys.Control | Keys.Shift)

            // or no node clicked
            || HitTest(e.Location).Node is not TreeNode newFocusedNode

            // or starting drag operation
            || (_selectedNodes.Contains(newFocusedNode) && modifierKeys == Keys.None && !ShallHandleRootIconClick(e.X, newFocusedNode, modifierKeys)))
        {
            _mouseClickHandled = false;
            base.OnMouseDown(e);
            return;
        }

        _mouseClickHandled = true;

        if (ShallHandleRootIconClick(e.X, newFocusedNode, modifierKeys) && newFocusedNode.Nodes.Count > 0)
        {
            if (newFocusedNode.IsExpanded)
            {
                newFocusedNode.Collapse(ignoreChildren: true);

                // Collapsing changes TreeView.SelectedNode without invoking AfterSelectHandler, need to explicitly emit SelectedNodesChanged
                OnSelectionChanged();
            }
            else
            {
                newFocusedNode.Expand();
            }
        }

        UpdateSelection(newFocusedNode, replace: !modifierKeys.HasFlag(Keys.Control), addRange: modifierKeys.HasFlag(Keys.Shift));
        _toBeFocusedNode = newFocusedNode;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        try
        {
            if (_toBeFocusedNode is not null)
            {
                this.InvokeAndForget(async () =>
                {
                    await Task.Delay(millisecondsDelay: 100);

                    if (_toBeFocusedNode?.TreeView == this)
                    {
                        FocusedNode = _toBeFocusedNode;
                    }

                    _toBeFocusedNode = null;
                });
                return;
            }

            if (_mouseClickHandled)
            {
                return;
            }

            Keys modifierKeys = ModifierKeys;
            if (e.Button == MouseButtons.Left
                && modifierKeys == Keys.None
                && HitTest(e.Location).Node is TreeNode newFocusedNode
                && !ShallHandleRootIconClick(e.X, newFocusedNode, modifierKeys))
            {
                // Explicit click on the same single item needs to be notified upstream.
                // In case of multi-selection, the clicked item becomes the single selection.
                if (SelectedNodes.Count == 1)
                {
                    OnSelectionChanged();
                }
                else if (FocusedNode != newFocusedNode)
                {
                    SelectedNode = newFocusedNode;
                }
                else
                {
                    _selectedNodes = [newFocusedNode];
                    Invalidate();
                    OnSelectionChanged();
                }
            }

            base.OnMouseUp(e);
        }
        finally
        {
            _mouseClickHandled = false;
        }
    }

    private void OnSelectionChanged()
    {
        if (SelectedNodes.Count <= 1 && FocusedNode is TreeNode focusedNode)
        {
            _multiselectionStartNode = focusedNode;
        }

        SelectedNodesChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool ShallHandleRootIconClick(int x, TreeNode newFocusedNode, Keys modifierKeys)
    {
        Validates.NotNull(ImageList);
        int spacing = DpiUtil.Scale(8);
        return modifierKeys == Keys.None && !ShowRootLines && newFocusedNode.Parent is null && x <= (spacing + ImageList.ImageSize.Width);
    }

    private void UpdateSelection(TreeNode newFocusedNode, bool replace, bool addRange)
    {
        bool changed = false;

        if (replace)
        {
            if (_selectedNodes.Count != 1 || _selectedNodes.First() != newFocusedNode)
            {
                changed = true;
                _selectedNodes = [newFocusedNode];
            }
        }
        else
        {
            changed = true;
            if (!_selectedNodes.Remove(newFocusedNode))
            {
                _selectedNodes.Add(newFocusedNode);
            }
        }

        if (addRange)
        {
            changed = true;
            _multiselectionStartNode ??= newFocusedNode;
            TreeNode? lastNode = _multiselectionStartNode;
            bool foundFirstNode = false;
            foreach (TreeNode node in this.Items())
            {
                if (!foundFirstNode)
                {
                    if (node == newFocusedNode)
                    {
                        foundFirstNode = true;
                    }
                    else if (node == lastNode)
                    {
                        foundFirstNode = true;
                        lastNode = newFocusedNode;
                    }
                    else
                    {
                        continue;
                    }
                }

                _selectedNodes.Add(node);

                if (node == lastNode)
                {
                    break;
                }
            }
        }

        if (FocusedNode != newFocusedNode)
        {
            FocusedNode = newFocusedNode;
        }
        else if (changed)
        {
            OnSelectionChanged();
        }

        if (changed)
        {
            Invalidate();
        }
    }
}
