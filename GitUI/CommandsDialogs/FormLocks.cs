using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Patches;
using GitCommands.Utils;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormLocks : GitModuleForm
    {
        [CanBeNull] private IReadOnlyList<GitItemStatus> _currentSelection;
        private readonly AsyncLoader _unstagedLoader = new AsyncLoader();

        private void SelectionChanged(object sender, EventArgs e)
        {
            // _currentFilesList.ClearSelected();

            _currentSelection = _currentFilesList.SelectedItems.Items().ToList();

            // var item = _currentFilesList.SelectedItem;
        }

        private void FreshLockedList(bool loadUnstaged = true)
        {
            using (WaitCursorScope.Enter())
            {
                if (loadUnstaged)
                {
                    ComputeUnstagedFiles(LoadUnstagedOutput, true);
                }
            }
        }

        private void ComputeUnstagedFiles(Action<IReadOnlyList<GitItemStatus>> onComputed, bool doAsync)
        {
            IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus()
            {
                return Module.LfsLockedFiles();
            }

            if (doAsync)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() =>
                {
                    return _unstagedLoader.LoadAsync(Module.LfsLockedFiles, onComputed);
                });
            }
            else
            {
                _unstagedLoader.Cancel();
                onComputed(GetAllChangedFilesWithSubmodulesStatus());
            }
        }

        private void LoadUnstagedOutput(IReadOnlyList<GitItemStatus> allChangedFiles)
        {
            var lastSelection = _currentFilesList != null
                ? _currentSelection ?? Array.Empty<GitItemStatus>()
                : Array.Empty<GitItemStatus>();

            var unstagedFiles = new List<GitItemStatus>();

            foreach (var fileStatus in allChangedFiles)
            {
                if (fileStatus.Staged == StagedStatus.Unset || fileStatus.IsStatusOnly)
                {
                    // Present status only errors in unstaged
                    unstagedFiles.Add(fileStatus);
                }
            }

            _currentFilesList.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), unstagedFiles);

            RestoreSelectedFiles(unstagedFiles, lastSelection);
        }

        private void RestoreSelectedFiles(IReadOnlyList<GitItemStatus> unstagedFiles, IReadOnlyList<GitItemStatus> lastSelection)
        {
            if (_currentFilesList == null || _currentFilesList.IsEmpty)
            {
                return;
            }

            var newItems = unstagedFiles;
            var names = lastSelection.ToHashSet(x => x.Name);
            var newSelection = newItems.Where(x => names.Contains(x.Name)).ToList();

            if (newSelection.Any())
            {
                _currentFilesList.SelectedGitItems = newSelection;
            }
            else
            {
                _currentFilesList.SelectStoredNextIndex(0);
            }

            return;
        }

        public FormLocks([NotNull] GitUICommands commands) : base(commands)
        {
            InitializeComponent();

            InitializeComplete();

            _currentFilesList.ContextMenuStrip = _unstagedFileContext;

            FreshLockedList();
        }

        private void stageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentFilesList.SelectedGitItem != null)
            {
                foreach (var item in _currentFilesList.SelectedItems)
                {
                    string result = Module.LfsUnLock(item.Item.File);

                    MessageBox.Show(result, "LFS UnLock Result");
                }
            }

            FreshLockedList();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            FreshLockedList();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FreshLockedList();
        }
    }
}
