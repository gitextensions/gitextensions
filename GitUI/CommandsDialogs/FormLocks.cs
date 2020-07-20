using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Patches;
using GitCommands.Utils;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormLocks : GitModuleForm
    {
        // private readonly TranslationString _developers = new TranslationString("Developers");
        // private readonly TranslationString _translators = new TranslationString("Translators");
        // private readonly TranslationString _designers = new TranslationString("Designers");
        // private readonly TranslationString _team = new TranslationString("Team");
        // private readonly TranslationString _contributors = new TranslationString("Contributors");
        // private readonly TranslationString _caption = new TranslationString("The application would not be possible without...");

        // [CanBeNull] private IReadOnlyList<GitItemStatus> _currentSelection;

        private readonly AsyncLoader _unstagedLoader = new AsyncLoader();

        private void StagedSelectionChanged(object sender, EventArgs e)
        {
            _currentFilesList.ClearSelected();

            // _currentSelection = _currentFilesList.SelectedItems.Items().ToList();

            var item = _currentFilesList.SelectedItem;
        }

        private void Staged_DataSourceChanged(object sender, EventArgs e)
        {
        }

        private void Staged_Enter(object sender, EnterEventArgs e)
        {
        }

        private void Staged_DoubleClick(object sender, EventArgs e)
        {
        }

        private void Initialize(bool loadUnstaged = true)
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
        }

        public FormLocks([NotNull] GitUICommands commands) : base(commands)
        {
            InitializeComponent();

            InitializeComplete();

            Initialize();
        }

        private void stageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
