using System.ComponentModel;

namespace GitUI.UserControls;

/// <summary>
///  A <see cref="TreeView"/> with basic support for selecting multiple <see cref="TreeNode"/>s.
/// </summary>
public class MultiSelectTreeView : NativeTreeView
{
    private HashSet<TreeNode> _selectedNodes = [];
    private bool _settingFocusedNode = false;

    public MultiSelectTreeView()
    {
        BeforeSelect += BeforeSelectHandler;
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

    protected override void OnEnter(EventArgs e)
    {
        base.OnEnter(e);
        Invalidate();
    }

    protected override void OnLeave(EventArgs e)
    {
        base.OnLeave(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            TreeViewHitTestInfo hitTestInfo = HitTest(e.Location);
            if (hitTestInfo.Node == base.SelectedNode)
            {
                // Enforce selection event
                FocusedNode = null;
            }
        }

        base.OnMouseDown(e);
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
            }
        }
    }
}
