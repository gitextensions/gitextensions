#nullable enable

using System.ComponentModel;

namespace GitUI.UserControls;

/// <summary>
///  A <see cref="TreeView"/> with basic support for selecting multiple <see cref="TreeNode"/>s.
/// </summary>
public class MultiSelectTreeView : NativeTreeView
{
    private HashSet<TreeNode> _selectedNodes = [];
    private bool _settingFocusedNode = false;
    private int _updateSuspendCount = 0;
    private TreeNode? _toBeSelectedNode = null;

    public MultiSelectTreeView()
    {
        AfterSelect += AfterSelectHandler;
        BeforeSelect += BeforeSelectHandler;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            AfterSelect -= AfterSelectHandler;
            BeforeSelect -= BeforeSelectHandler;
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
            try
            {
                _settingFocusedNode = true;
                base.SelectedNode = value;
                value?.EnsureVerticallyVisible();
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

                if (alreadyFocused)
                {
                    // Notify that the node is also selected now
                    FocusedNodeChanged?.Invoke(this, EventArgs.Empty);
                }
                else
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
        if (e.Button == MouseButtons.Left && HitTest(e.Location).Node is TreeNode node)
        {
            // Expand / collapse root folder nodes
            if (!ShowRootLines && ModifierKeys == Keys.None && node.Parent is null && node.Nodes.Count > 0)
            {
                if (node.IsExpanded)
                {
                    node.Collapse(ignoreChildren: true);
                }
                else
                {
                    node.Expand();
                }

                _toBeSelectedNode = node;
                return;
            }

            if (node == base.SelectedNode)
            {
                // Enforce selection event
                FocusedNode = null;
            }
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_toBeSelectedNode is not null)
        {
            this.InvokeAndForget(async () =>
            {
                await Task.Delay(millisecondsDelay: 100);

                if (_toBeSelectedNode.TreeView == this)
                {
                    SelectedNode = _toBeSelectedNode;
                }

                _toBeSelectedNode = null;
            });
            return;
        }

        base.OnMouseUp(e);
    }

    private void AfterSelectHandler(object? sender, TreeViewEventArgs e)
    {
        FocusedNodeChanged?.Invoke(sender, e);
    }

    private void BeforeSelectHandler(object? sender, TreeViewCancelEventArgs e)
    {
        if (!_settingFocusedNode && e.Node is TreeNode newFocusedNode)
        {
            Keys modifierKeys = ModifierKeys;
            if ((modifierKeys | Keys.Control | Keys.Shift) != (Keys.Control | Keys.Shift))
            {
                e.Cancel = true;
                return;
            }

            UpdateSelection(replace: !modifierKeys.HasFlag(Keys.Control), addRange: modifierKeys.HasFlag(Keys.Shift));
            Invalidate();
        }

        return;

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
                e.Cancel = true;
            }
        }
    }
}
