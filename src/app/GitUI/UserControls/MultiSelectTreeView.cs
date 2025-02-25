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
    private HashSet<TreeNode> _selectedNodes = [];
    private bool _settingFocusedNode = false;
    private TreeNode? _toBeFocusedNode = null;
    private int _updateSuspendCount = 0;

    public MultiSelectTreeView()
    {
        BeforeSelect += BeforeSelectHandler;
        AfterSelect += AfterSelectHandler;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BeforeSelect -= BeforeSelectHandler;
            AfterSelect -= AfterSelectHandler;
        }

        base.Dispose(disposing);
    }

    public event EventHandler? FocusedNodeChanged;

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
            }

            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HashSet<TreeNode> SelectedNodes => _selectedNodes;

    public void SetSelectedNodes(HashSet<TreeNode> selectedNodes, TreeNode? focusedNode)
    {
        _selectedNodes = selectedNodes;
        if (focusedNode is not null)
        {
            FocusedNode = focusedNode;
        }

        Invalidate();
    }

    public bool UpdateSuspended => _updateSuspendCount > 0;

    public void AddToSelection(TreeNode node)
    {
        _selectedNodes.Add(node);
        FocusedNode = node;
        Invalidate();
    }

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
            || (_selectedNodes.Contains(newFocusedNode) && modifierKeys == Keys.None && !ShallHandleRootIconClick()))
        {
            _mouseClickHandled = false;
            base.OnMouseDown(e);
            return;
        }

        _mouseClickHandled = true;

        if (ShallHandleRootIconClick() && newFocusedNode.Nodes.Count > 0)
        {
            // Expand / collapse root folder nodes unless there is only one
            if (newFocusedNode.IsExpanded)
            {
                newFocusedNode.Collapse(ignoreChildren: true);

                // Collapsing changes TreeView.SelectedNode without invoking AfterSelectHandler, need to explicitly emit FocusedNodeChanged
                FocusedNodeChanged?.Invoke(this, e);
            }
            else
            {
                newFocusedNode.Expand();
            }
        }

        UpdateSelection(replace: !modifierKeys.HasFlag(Keys.Control), addRange: modifierKeys.HasFlag(Keys.Shift));
        Invalidate();

        return;

        bool ShallHandleRootIconClick()
        {
            Validates.NotNull(ImageList);
            int spacing = DpiUtil.Scale(8);
            return modifierKeys == Keys.None && !ShowRootLines && newFocusedNode.Parent is null && e.X <= (spacing + ImageList.ImageSize.Width);
        }

        void UpdateSelection(bool replace, bool addRange)
        {
            if (replace)
            {
                _selectedNodes = [newFocusedNode];
            }
            else
            {
                if (_selectedNodes.Contains(newFocusedNode))
                {
                    _selectedNodes.Remove(newFocusedNode);
                }
                else
                {
                    _selectedNodes.Add(newFocusedNode);
                }
            }

            if (addRange)
            {
                TreeNode? lastNode = FocusedNode;
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

                // Keep FocusedNode
                _toBeFocusedNode = FocusedNode;
                return;
            }

            FocusedNode = newFocusedNode;
            _toBeFocusedNode = newFocusedNode;
        }
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

            if (!_mouseClickHandled)
            {
                base.OnMouseUp(e);
            }
        }
        finally
        {
            _mouseClickHandled = false;
        }
    }

    private void AfterSelectHandler(object? sender, TreeViewEventArgs e)
    {
        if (!_settingFocusedNode && !_mouseClickHandled)
        {
            SelectedNode = FocusedNode;
        }

        FocusedNodeChanged?.Invoke(sender, e);
    }

    private void BeforeSelectHandler(object? sender, TreeViewCancelEventArgs e)
    {
        if (_toBeFocusedNode is not null && e.Node != _toBeFocusedNode)
        {
            e.Cancel = true;
        }
    }
}
