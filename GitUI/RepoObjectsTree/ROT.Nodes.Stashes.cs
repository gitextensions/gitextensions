using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces.Notifications;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        static readonly string stashKey = Guid.NewGuid().ToString();
        static readonly string stashesKey = Guid.NewGuid().ToString();

        /// <summary>Reloads the stashes.</summary>
        static void OnReloadStashes(ICollection<StashNode> stashes, RootNode<StashNode> stashesNode)
        {
            stashesNode.TreeNode.Text = string.Format("{0} ({1})", Strings.stashes, stashes.Count);
        }

        /// <summary>Adds the specified <paramref name="stashNode"/> to the stashes tree.</summary>
        TreeNode OnAddStash(TreeNodeCollection nodes, StashNode stashNode)
        {
            GitStash stash = stashNode.Value;
            TreeNode treeNode = new TreeNode(stash.Name)
            {
                Name = string.Format("stash{0}", stash.Index),
                ToolTipText = stash.Message,
                ContextMenuStrip = menuStash,
                ImageKey = stashKey,
                SelectedImageKey = stashKey,
            };
            nodes.Add(treeNode);

            return treeNode;
        }

        /// <summary>Stash node.</summary>
        sealed class StashNode : Node<GitStash, RootNode<StashNode>>
        {
            public StashNode(GitStash stash, GitUICommands uiCommands)
                : base(stash, null, uiCommands)
            {
                IsDraggable = true;
            }

            internal override void OnSelected()
            {
                base.OnSelected();
                UiCommands.BrowseRepo.SetSelectedRevision(Value.Name);
            }

            public void Pop()
            {
                throw new NotImplementedException();
            }

            public void Apply()
            {
                throw new NotImplementedException();
            }

            public void Delete()
            {
                NotifyIf(Git.StashDelete(Value.Name),
                    () => new Notification(StatusSeverity.Success, "Stash dropped/deleted."),
                    () => new Notification(StatusSeverity.Fail, "Failed to drop/delete stash."));
            }

            public void ShowDiff()
            {
                throw new NotImplementedException();
            }

            public void CreateBranch()
            {
                throw new NotImplementedException();
            }
        }
    }
}
