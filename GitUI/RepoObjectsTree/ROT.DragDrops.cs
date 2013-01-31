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

        /// <summary>Occurs when an object is dragged into the <see cref="TreeView"/>.</summary>
        void OnTreeDragEnter(object sender, DragEventArgs e)
        {// set the target drop effect to the effect specified in OnTreeNodeDrag
            e.Effect = IsValidData(e.Data)
                ? e.AllowedEffect
                : DragDropEffects.None;
        }

        bool IsValidData(IDataObject data)
        {
            var treeNode = GetDraggedTreeNode(data);
            if (treeNode == null) { return false; }

            Node node = Node.GetNodeSafe(treeNode);
            return (node != null) && node.IsDraggable;
        }

        /// <summary>Occurs while the user is dragging an object over the <see cref="TreeView"/>.
        /// May occur multiple times during a single drag.</summary>
        void OnTreeDragOver(object sender, DragEventArgs e)
        {
            OnTreeDragEnter(sender, e);
            if (e.Effect == DragDropEffects.None) { return; }

            TreeNode draggedNode = GetDraggedTreeNode(e);
            Node dragged = Node.GetNode(draggedNode);

            TreeNode targetNode = GetTargetNode(e);
            Node node = Node.GetNode(targetNode);

            if (node.AllowDrop && node.Accepts(dragged))
            {
                if (previousTarget != null && previousTarget != targetNode)
                {
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

            // get dragged node
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            var targetNode = GetTargetNode(e);

            if (draggedNode != targetNode)
            {// dragged node NOT node at drop location
                Node dragged = (Node)draggedNode.Tag;
                Node target = (Node)targetNode.Tag;

                target.Drop(dragged);
                Highlight(targetNode, false);
            }

            throw new NotImplementedException();
        }

        /// <summary>Gets the target <see cref="TreeNode"/>.</summary>
        TreeNode GetTargetNode(DragEventArgs e)
        {
            // get coordinates of drop location
            Point targetPoint = treeMain.PointToClient(new Point(e.X, e.Y));

            // get node at drop location
            return treeMain.GetNodeAt(targetPoint);
        }

        bool IsValidDrop(TreeNode draggedNode, TreeNode targetNode)
        {
            GitStash stash = draggedNode.Tag as GitStash;
            if (stash != null)
            {// stash -> local branch = apply stash


                return true;
            }

            BranchNode draggedBranch = draggedNode.Tag as BranchNode;
            if (draggedBranch != null)
            {
                if (draggedBranch.IsLocal)
                {// local branch
                    // local branch -> branches header = new branch
                    // local branch -> remotes header = publish new

                    BranchNode targetBranch = targetNode.Tag as BranchNode;
                    if (targetBranch != null)
                    {
                        //!if (targetBranch.IsRemote)
                        //{// local branch -> remote branch = push
                        //    uiCommands.StartPushDialog(
                        //        new GitPushAction(
                        //            ((RemoteBranchNode)targetBranch).Remote,
                        //            draggedBranch.FullPath,
                        //            targetBranch.FullPath));
                        //}
                        //else if (targetBranch.IsLocal && Equals(git.GetSelectedBranch(), targetBranch.FullPath))
                        //{// local branch -> current local branch
                        //    // TODO: rebase on Alt+Drag
                        //    uiCommands.StartMergeBranchDialog(draggedBranch.FullPath);
                        //}
                    }
                }
                else
                {// remote
                    // remote branch -> branch = pull/fetch
                    // remote branch -> branches header = new local tracking branch


                    throw new NotImplementedException();
                }
            }

            throw new NotImplementedException();
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

        /// <summary>Highlights the specified <see cref="TreeNode"/> as a valid drop target.</summary>
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
            Node.OnNode(e.Node, node => node.OnSelected());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is clicked.</summary>
        void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Node.OnNode(e.Node, node => node.OnClick());
        }

        /// <summary>Occurs when a <see cref="TreeNode"/> is double-clicked.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Node.OnNode(e.Node, node => node.OnDoubleClick());
        }

        /// <summary>Represents a valid drag-drop action.</summary>
        class DragDropAction
        {
            static DragDropAction<TDragged, TTarget> New<TDragged, TTarget>(
                Func<TDragged, TTarget, GitUICommands, bool> action)
                where TDragged : class
                where TTarget : class
            {
                return new DragDropAction<TDragged, TTarget>(action);
            }

            /// <summary>Gets the type of the dragged object.</summary>
            public Type DraggedType { get; private set; }
            /// <summary>Gets the type of the target object.</summary>
            public Type TargetType { get; private set; }
            /// <summary>Gets the action to perform on the drag-drop. 
            /// <remarks>Returns true if both the dragged and target objects are compatible.</remarks></summary>
            public Func<object, object, GitUICommands, bool> Action { get; private set; }

            public DragDropAction(Type draggedType, Type targetType, Func<object, object, GitUICommands, bool> action)
            {
                DraggedType = draggedType;
                TargetType = targetType;
                Action = action;
            }

            public static DragDropAction GetFirstOrDefault(TreeNode dragged, TreeNode target)
            {
                return AcceptableDragDrops
                    .FirstOrDefault(
                        dda =>
                            dda.DraggedType == dragged.Tag.GetType() &&
                            dda.TargetType == target.Tag.GetType());
            }

            public static bool IsValidDragDrop(TreeNode dragged, TreeNode target)
            {
                return GetFirstOrDefault(dragged, target) != null;
            }

            public static DragDropAction<TDragged, TTarget> GetDragDropAction<TDragged, TTarget>(
                TreeNode dragged, TreeNode target)
                where TDragged : class
                where TTarget : class
            {
                return GetFirstOrDefault(dragged, target) as DragDropAction<TDragged, TTarget>;
            }

            static List<DragDropAction> AcceptableDragDrops;

            static DragDropAction()
            {
                AcceptableDragDrops = new List<DragDropAction>();
                AcceptableDragDrops.Add(New<BranchNode, BranchNode>((dragged, target, cmds) =>
                {
                    if (Equals(dragged, target)) { return false; }// disallow merge into same exact branch

                    string currentBranch = cmds.GitModule.GetSelectedBranch();
                    if (Equals(dragged.FullPath, currentBranch))
                    {// current branch -> local branch = merge
                        cmds.StartMergeBranchDialog(target.FullPath);
                        return true;
                    }

                    if (Equals(target.FullPath, currentBranch))
                    {// local branch -> current branch = merge
                        cmds.StartMergeBranchDialog(dragged.FullPath);
                        return true;
                    }
                    return false;
                }));
                //AcceptableDragDrops.Add(New<RemoteBranchNode, BranchesNode>((dragged, target, cmds) =>
                //{
                //    // TODO: check if local branch with same name already exists
                //    //cmds.Module.GetLocalConfig().
                //    var cmd = GitCommandHelpers.BranchCmd(dragged.FullBranchName, dragged.FullPath, false);
                //    FormProcess.ShowDialog(null, cmd);

                //    return true;
                //}));
                //AcceptableDragDrops.Add(New<Branch,RemotesList>());
                AcceptableDragDrops.Add(New<BranchNode, BranchesNode>((dragged, target, cmds) =>
                {// local branch -> branches header = new branch
                    using (FormBranchSmall branchForm = new FormBranchSmall(cmds, dragged.FullPath))
                    {
                        branchForm.ShowDialog();
                    }
                    return true;
                }));
            }
        }

        /// <summary>Provides a strong-type, generic implementation of <see cref="DragDropAction"/>.</summary>
        class DragDropAction<TDragged, TTarget> : DragDropAction
            where TDragged : class
            where TTarget : class
        {
            public Func<TDragged, TTarget, GitUICommands, bool> Action { get; private set; }

            public DragDropAction(
                Func<TDragged, TTarget, GitUICommands, bool> action)
                : base(typeof(TDragged), typeof(TTarget),
                (dragged, target, cmds) => action(
                    ((TreeNode)dragged).Tag as TDragged,
                    ((TreeNode)target).Tag as TTarget,
                    cmds))
            {
                Action = action;
            }
        }
    
    }
}
