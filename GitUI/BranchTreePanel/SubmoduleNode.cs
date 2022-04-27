using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using GitCommands.Git;
using GitCommands.Submodules;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.BranchTreePanel
{
    // Node representing a submodule
    internal sealed class SubmoduleNode : Node
    {
        public SubmoduleInfo Info { get; set; }
        public bool IsCurrent { get; }
        public IReadOnlyList<GitItemStatus>? GitStatus { get; }
        public string LocalPath { get; }
        public string SuperPath { get; }
        public string SubmoduleName { get; }
        public string BranchText { get; }

        public SubmoduleNode(Tree tree, SubmoduleInfo submoduleInfo, bool isCurrent,
            IReadOnlyList<GitItemStatus>? gitStatus, string localPath, string superPath)
            : base(tree)
        {
            Info = submoduleInfo;
            IsCurrent = isCurrent;
            GitStatus = gitStatus;
            LocalPath = localPath;
            SuperPath = superPath;

            // Extract submodule name and branch
            // e.g. Info.Text = "Externals/conemu-inside [no branch]"
            // Note that the branch portion won't be there if the user hasn't yet init'd + updated the submodule.
            var pathAndBranch = Info.Text.Split(Delimiters.Space, 2);
            Trace.Assert(pathAndBranch.Length >= 1);
            SubmoduleName = pathAndBranch[0].SubstringAfterLast('/'); // Remove path
            BranchText = pathAndBranch.Length == 2 ? " " + pathAndBranch[1] : "";
        }

        public void RefreshDetails()
        {
            ApplyStatus();
        }

        public bool CanOpen => !IsCurrent;

        protected override string DisplayText()
        {
            return SubmoduleName + BranchText + Info.Detailed?.AddedAndRemovedText;
        }

        protected override string NodeName()
        {
            return SubmoduleName;
        }

        public void Open()
        {
            if (!Directory.Exists(Info.Path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(owner: null, Info.Path, Info.Text);
                return;
            }

            if (Info.Detailed?.RawStatus is not null)
            {
                UICommands.BrowseSetWorkingDir(Info.Path, ObjectId.WorkTreeId, Info.Detailed.RawStatus.OldCommit);
                return;
            }

            UICommands.BrowseSetWorkingDir(Info.Path);
        }

        public void LaunchGitExtensions()
        {
            if (!Directory.Exists(Info.Path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(owner: null, Info.Path, Info.Text);
                return;
            }

            GitUICommands.LaunchBrowse(workingDir: Info.Path.EnsureTrailingPathSeparator(), ObjectId.WorkTreeId, Info?.Detailed?.RawStatus?.OldCommit);
        }

        internal override void OnSelected()
        {
            if (Tree.IgnoreSelectionChangedEvent)
            {
                return;
            }

            base.OnSelected();
        }

        internal override void OnDoubleClick()
        {
            Open();
        }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();
            ApplyStatus(); // Note that status is applied also after the tree is created, when status is applied
        }

        protected override FontStyle GetFontStyle()
            => base.GetFontStyle() | (IsCurrent ? FontStyle.Bold : FontStyle.Regular);

        private void ApplyStatus()
        {
            TreeViewNode.ToolTipText = DisplayText();
            TreeViewNode.ImageKey = GetSubmoduleItemImage(Info?.Detailed);
            TreeViewNode.SelectedImageKey = TreeViewNode.ImageKey;

            return;

            // NOTE: Copied and adapted from FormBrowse.GetSubmoduleItemImage
            static string GetSubmoduleItemImage(DetailedSubmoduleInfo? details)
            {
                return (details?.Status, details?.IsDirty) switch
                {
                    (SubmoduleStatus.FastForward, true) => nameof(Images.SubmoduleRevisionUpDirty),
                    (SubmoduleStatus.FastForward, false) => nameof(Images.SubmoduleRevisionUp),
                    (SubmoduleStatus.Rewind, true) => nameof(Images.SubmoduleRevisionDownDirty),
                    (SubmoduleStatus.Rewind, false) => nameof(Images.SubmoduleRevisionDown),
                    (SubmoduleStatus.NewerTime, true) => nameof(Images.SubmoduleRevisionSemiUpDirty),
                    (SubmoduleStatus.NewerTime, false) => nameof(Images.SubmoduleRevisionSemiUp),
                    (SubmoduleStatus.OlderTime, true) => nameof(Images.SubmoduleRevisionSemiDownDirty),
                    (SubmoduleStatus.OlderTime, false) => nameof(Images.SubmoduleRevisionSemiDown),
                    (_, true) => nameof(Images.SubmoduleDirty),
                    (_, false) => nameof(Images.FileStatusModified),
                    _ => nameof(Images.FolderSubmodule)
                };
            }
        }

        internal async Task SetStatusToolTipAsync(CancellationToken token)
        {
            string toolTip;
            if (Info.Detailed?.RawStatus is not null)
            {
                // Prefer submodule status, shows ahead/behind
                toolTip = LocalizationHelpers.ProcessSubmoduleStatus(
                    new GitModule(Info.Path),
                    Info.Detailed.RawStatus,
                    moduleIsParent: false,
                    limitOutput: true);
            }
            else if (GitStatus is not null)
            {
                ArtificialCommitChangeCount changeCount = new();
                changeCount.Update(GitStatus);
                toolTip = changeCount.GetSummary();
            }
            else
            {
                // No data need to be set
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            TreeViewNode.ToolTipText = toolTip;
        }
    }
}
