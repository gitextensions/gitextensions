using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.UserControls
{
    // DragDrops
    partial class RepoObjectsTree
    {
        /// <summary>Initializes the <see cref="RepoObjectsTree"/> drag/drop operations.</summary>
        void DragDrops()
        {
            treeMain.AllowDrop = true;
            treeMain.ItemDrag += OnTreeNodeDrag;
            treeMain.DragEnter += OnTreeDragEnter;
            treeMain.DragOver += OnTreeDragOver;
            treeMain.DragDrop += OnTreeDragDrop;
        }

        void OnTreeNodeDrag(object sender, ItemDragEventArgs e)
        {
            // example from: http://msdn.microsoft.com/en-us/library/system.windows.forms.treeview.itemdrag.aspx

            if (e.Button == MouseButtons.Left)
            {// left mouse button -> move dragged node
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
            else if (e.Button == MouseButtons.Right)
            {// right mouse button -> copy dragged node
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }

            throw new NotImplementedException();
        }

        /// <summary>Set the target drop effect to the effect specified in the ItemDrag event handler.</summary>
        void OnTreeDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        /// <summary>Select the node under the mouse pointer to indicate the expected drop location.</summary>
        void OnTreeDragOver(object sender, DragEventArgs e)
        {
            // Retrieve the client coordinates of the mouse position.
            Point targetPoint = treeMain.PointToClient(new Point(e.X, e.Y));

            // Select the node at the mouse position.
            treeMain.SelectedNode = treeMain.GetNodeAt(targetPoint);
        }

        void OnTreeDragDrop(object sender, DragEventArgs e)
        {
            // get dragged node
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // get coordinates of drop location
            Point targetPoint = treeMain.PointToClient(new Point(e.X, e.Y));

            // get node at drop location
            TreeNode targetNode = treeMain.GetNodeAt(targetPoint);


            if (draggedNode != targetNode && IsValidDrop(draggedNode, targetNode))
            {// dragged node NOT node at drop location AND valid drop
                if (e.Effect == DragDropEffects.Move)
                {// move -> remove the node from its current location; add it to node at drop location.
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                }
                else if (e.Effect == DragDropEffects.Copy)
                {// copy -> clone the dragged node; add it to node at drop location 
                    targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                }

                // Expand the node at the location to show dropped node
                targetNode.Expand();
            }

            throw new NotImplementedException();
        }

        bool IsValidDrop(TreeNode draggedNode, TreeNode targetNode)
        {
            // branch onto branch

            throw new NotImplementedException();
        }

        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {

        }

        /// <summary>Performed on a <see cref="TreeNode"/> double-click.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.IsAncestorOf(nodeBranches))
            {// branches/
                if (node.HasNoChildren())
                {// no children -> branch
                    // needs to go into Settings, but would probably like an option to:
                    // stash; checkout;
                    uiCommands.StartCheckoutBranchDialog(base.ParentForm, node.Text, false);
                }

            }
        }

    }
}
