using System.ComponentModel;

namespace GitUI.UserControls;

/// <summary>
///  A <see cref="TreeView"/> with basic support for selecting multiple <see cref="TreeNode"/>s.
/// </summary>
public class MultiSelectTreeView : NativeTreeView
{
    private HashSet<TreeNode> _selectedNodes = [];
    private bool _settingFocusedNode = false;
    private TreeNode? _toBeSelectedNode = null;

    public MultiSelectTreeView()
    {
        BeforeSelect += BeforeSelectHandler;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
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
                value?.EnsureVisible();
            }
            finally
            {
                _settingFocusedNode = false;
            }
        }
    }

    /// <summary>
    /// <para>For practical purposes: The last <see cref="TreeNode"/> added to selection.</para>
    /// <para>Actually: Focused item if selected, otherwise last item in <see cref="SelectedNodes"/> list.</para>
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TreeNode? LastSelectedNode
        => _selectedNodes.Contains(base.SelectedNode) ? base.SelectedNode : _selectedNodes.LastOrDefault();

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TreeNode? SelectedNode
    {
        set
        {
            if (value is null
                ? _selectedNodes.Count == 0
                : (_selectedNodes.Count == 1 && _selectedNodes.Contains(value)))
            {
                return;
            }

            _selectedNodes = value is null ? [] : [value];
            if (value is not null)
            {
                FocusedNode = value;
            }

            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HashSet<TreeNode> SelectedNodes
    {
        get => _selectedNodes;
        set
        {
            _selectedNodes = value;
            if (_selectedNodes.Count > 0)
            {
                FocusedNode = _selectedNodes.Last();
            }

            Invalidate();
        }
    }

    public void AddToSelection(TreeNode node)
    {
        _selectedNodes.Add(node);
        FocusedNode = node;
        Invalidate();
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
                SelectedNode = _toBeSelectedNode;
                _toBeSelectedNode = null;
            });
            return;
        }

        base.OnMouseUp(e);
    }

    private void BeforeSelectHandler(object? sender, TreeViewCancelEventArgs e)
    {
        if (!_settingFocusedNode)
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

        FocusedNodeChanged?.Invoke(sender, e);

        return;

        void UpdateSelection(bool replace, bool addRange)
        {
            TreeNode newFocusedNode = e.Node;

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
                TreeNode lastNode = FocusedNode;
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
