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
            treeMain.AllowDrop = true;
            treeMain.ItemDrag += OnTreeNodeDrag;
            treeMain.DragEnter += OnTreeDragEnter;
            treeMain.DragOver += OnTreeDragOver;
            treeMain.DragDrop += OnTreeDragDrop;
        }

        void OnTreeNodeDrag(object sender, ItemDragEventArgs e)
        {
            // example from: http://msdn.microsoft.com/en-us/library/system.windows.forms.treeview.itemdrag.aspx

            treeMain.SelectedNode = (TreeNode)e.Item;

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
            e.Effect = IsValidData(e.Data)
                ? e.AllowedEffect
                : DragDropEffects.None;
        }

        bool IsValidData(IDataObject data)
        {
            return data.GetData(typeof(TreeNode)) is TreeNode;
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

            if (draggedNode != targetNode)
            {// dragged node NOT node at drop location
                var dragDropAction = DragDropAction.GetFirstOrDefault(draggedNode, targetNode);
                if (dragDropAction != null)
                {
                    //!dragDropAction.Action(draggedNode, targetNode, uiCommands);
                }
            }

            throw new NotImplementedException();
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

        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {

        }

        /// <summary>Performed on a <see cref="TreeNode"/> click.</summary>
        void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Node node = e.Node.Tag as Node;
            if (node != null)
            {
                node.OnClick();
            }
        }

        /// <summary>Performed on a <see cref="TreeNode"/> double-click.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Node node = e.Node.Tag as Node;
            if (node != null)
            {
                node.OnDoubleClick();
            }
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
                AcceptableDragDrops.Add(New<RemoteBranchNode, BranchesNode>((dragged, target, cmds) =>
                {
                    // TODO: check if local branch with same name already exists
                    //cmds.Module.GetLocalConfig().
                    var cmd = GitCommandHelpers.BranchCmd(dragged.FullBranchName, dragged.FullPath, false);
                    FormProcess.ShowDialog(null, cmd);

                    return true;
                }));
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

        class DropNode<T>
            where T : class
        {
            public TreeNode TreeNode { get; set; }
            public T Value { get; set; }

            public DropNode(TreeNode treeNode)
            {
                TreeNode = treeNode;
                Value = treeNode.Tag as T;
            }
        }
    }
}
