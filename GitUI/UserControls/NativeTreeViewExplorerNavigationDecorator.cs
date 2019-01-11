using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    // Add explorer-like navigation to NativeTreeView:
    // * Arrow key navigation to highlight without selecting node
    // * Space or Enter key to select node
    // * Mouse clicking highlighted node selects it
    //
    // As this decorator sets TreeView.SelectedNode, you should avoid hooking into
    // node selection-based events. Instead, hook into AfterSelect on this decorator
    // to know when a node has been selected (either by Space/Enter, or mouse click).
    public class NativeTreeViewExplorerNavigationDecorator
    {
        private readonly NativeTreeView _treeView;
        private DateTime _lastKeyNavigateTime = DateTime.MinValue;
        private readonly Func<DateTime> _getCurrentTime;

        public event TreeViewEventHandler AfterSelect;

        public NativeTreeViewExplorerNavigationDecorator(NativeTreeView treeView, Func<DateTime> getCurrentTime)
        {
            _treeView = treeView;
            _getCurrentTime = getCurrentTime;

            _treeView.KeyDown += OnKeyDown;
            _treeView.PreviewKeyDown += OnPreviewKeyDown;
            _treeView.AfterSelect += OnAfterSelect;
            _treeView.NodeMouseClick += OnNodeMouseClick;
        }

        public NativeTreeViewExplorerNavigationDecorator(NativeTreeView treeView)
            : this(treeView, () => DateTime.Now)
        {
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Supress the "ding" when Enter is pressed
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                _lastKeyNavigateTime = _getCurrentTime();
            }
            else if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                // Force a reselection of the current node
                _lastKeyNavigateTime = DateTime.MinValue;
                var currNode = _treeView.SelectedNode;
                _treeView.SelectedNode = null;
                _treeView.SelectedNode = currNode;
            }
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            // If arrow key was used to navigate to this node, don't send OnSelected
            int delta = (int)DateTime.Now.Subtract(_lastKeyNavigateTime).TotalMilliseconds;
            if (delta >= 0 && delta < 500)
            {
                return;
            }

            AfterSelect(sender, e);
        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // If selected node is clicked, make sure to force re-selection. This way, if user
            // navigates to a node by keyboard, then clicks on the same node with the mouse, it
            // will perform the AfterSelect action (i.e. select revision in revision graph).
            if (_treeView.SelectedNode == e.Node)
            {
                _treeView.SelectedNode = null;
            }

            _treeView.SelectedNode = e.Node;
        }
    }
}
