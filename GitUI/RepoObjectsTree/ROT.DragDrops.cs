using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    // DragDrops
    partial class RepoObjectsTree
    {
        /// <summary>Initializes the <see cref="RepoObjectsTree"/> drag/drop operations.</summary>
        void DragDrops()
        {
            // example at: http://msdn.microsoft.com/en-us/library/system.windows.forms.treeview.itemdrag.aspx

            /* drag-drop events
              * TreeView must allow dropped data (AllowDrop)
              * when any object (another control, files, etc.) is dragged into the TreeView, DragEnter executes
              * while any objects is dragged over the TreeView, DragOver executes (possibly multiple times)
              * DragEnter and DragOver should determine whether the dragged object is allowed to be dropped
              * if the dragged object is allowed to be dropped and it is, DragDrop executes
              *
              * ItemDrag is raised when a TreeNode is dragged
              */

            treeMain.AllowDrop = true;
            treeMain.ItemDrag += OnTreeNodeDrag;
            treeMain.DragEnter += OnTreeDragEnter;
            treeMain.DragOver += OnTreeDragOver;
            treeMain.DragDrop += OnTreeDragDrop;
            treeMain.DragLeave += OnTreeDragLeave;
        }
        /// <summary>Occurs when the user begins dragging a <see cref="TreeNode"/>.</summary>
        void OnTreeNodeDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode draggedNode = (TreeNode)e.Item;
            var dragged = Node.GetNode(draggedNode);

            if (dragged.IsDraggable == false) { return; }

            treeMain.SelectedNode = draggedNode;

            if (e.Button == MouseButtons.Left)
            {// left mouse button -> move dragged node
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
            else if (e.Button == MouseButtons.Right)
            {// right mouse button -> copy dragged node
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        /// <summary>Verifies that the dragged object is valid for dropping.
        /// Occurs when an object is dragged into the <see cref="TreeView"/>.</summary>
        void OnTreeDragEnter(object sender, DragEventArgs e)
        {// set the target drop effect to the effect specified in OnTreeNodeDrag
            e.Effect = IsValidDragData(e.Data)
                ? e.AllowedEffect
                : DragDropEffects.None;
        }

        /// <summary>true if <paramref name="data"/> is valid for dragging.</summary>
        bool IsValidDragData(IDataObject data)
        {
            var treeNode = GetDraggedTreeNode(data);
            if (treeNode == null) { return false; }

            Node node = Node.GetNodeSafe<Node>(treeNode);
            return (node != null) && node.IsDraggable;
        }

        /// <summary>Verifies that the dragged object is valid for dropping.
        /// Occurs while the user is dragging an object over the <see cref="TreeView"/>.
        /// May occur multiple times during a single drag.</summary>
        void OnTreeDragOver(object sender, DragEventArgs e)
        {
            OnTreeDragEnter(sender, e);
            if (e.Effect == DragDropEffects.None) { return; }

            TreeNode draggedNode = GetDraggedTreeNode(e);
            Node dragged = Node.GetNode(draggedNode);

            TreeNode targetNode = GetTargetNode(e);
            if (targetNode == draggedNode)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            Node target = Node.GetNode(targetNode);

            if (dragged != target && target.AllowDrop && target.Accepts(dragged))
            {// NOT same AND allows drops AND accepts the object
                e.Effect = e.AllowedEffect;
                if (previousTarget != null && previousTarget != targetNode)
                {// target changed -> UN-highlight previous target
                    Highlight(previousTarget, false);
                }
                previousTarget = targetNode;
                Highlight(targetNode);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>Occurs when the user drags an object OUTSIDE the <see cref="TreeView"/>.</summary>
        void OnTreeDragLeave(object sender, EventArgs e)
        {
            if (previousTarget != null)
            {
                Highlight(previousTarget, false);
            }
        }

        /// <summary>Occurs when a drag-drop operation is successful.</summary>
        void OnTreeDragDrop(object sender, DragEventArgs e)
        {
            OnTreeDragOver(sender, e);
            if (e.Effect == DragDropEffects.None) { return; }

            TreeNode draggedNode = GetDraggedTreeNode(e);
            TreeNode targetNode = GetTargetNode(e);

            if (draggedNode != targetNode)
            {// dragged node NOT node at drop location
                Node dragged = (Node)draggedNode.Tag;
                Node target = (Node)targetNode.Tag;

                target.Drop(dragged);
                Highlight(targetNode, false);
            }
        }

        /// <summary>Gets the target <see cref="TreeNode"/>.</summary>
        TreeNode GetTargetNode(DragEventArgs e)
        {
            // get coordinates of drop location
            Point targetPoint = treeMain.PointToClient(new Point(e.X, e.Y));

            // get node at drop location
            return treeMain.GetNodeAt(targetPoint);
        }

        TreeNode previousTarget;

        /// <summary>Gets the <see cref="TreeNode"/> that's being dragged, or null.</summary>
        static TreeNode GetDraggedTreeNode(DragEventArgs e)
        {
            return GetDraggedTreeNode(e.Data);
        }

        /// <summary>Gets the <see cref="TreeNode"/> that's being dragged, or null.</summary>
        static TreeNode GetDraggedTreeNode(IDataObject data)
        {
            return data.GetData(typeof(TreeNode)) as TreeNode;
        }

        /// <summary>Highlights or UN-highlights the specified <see cref="TreeNode"/> as a valid drop target.</summary>
        /// <param name="treeNode"><see cref="TreeNode"/> to highlight.</param>
        /// <param name="on">true to highlight, false to un-highlight.</param>
        static void Highlight(TreeNode treeNode, bool on = true)
        {
            treeNode.ForeColor = on ? Color.White : Color.Black;
            treeNode.BackColor = on ? Color.MediumPurple : new Color();
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is selected.</summary>
        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node => node.OnSelected());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is clicked.</summary>
        void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Node.OnNode<Node>(e.Node, node => node.OnClick());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is double-clicked.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // e.Node won't be the one you double clicked, but a child node instead.
            Node.OnNode<Node>(treeMain.SelectedNode, node => node.OnDoubleClick());
        }
    }
}
