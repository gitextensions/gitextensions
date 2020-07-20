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
    public sealed class FormLocks : GitModuleForm
    {
        // private readonly TranslationString _developers = new TranslationString("Developers");
        // private readonly TranslationString _translators = new TranslationString("Translators");
        // private readonly TranslationString _designers = new TranslationString("Designers");
        // private readonly TranslationString _team = new TranslationString("Team");
        // private readonly TranslationString _contributors = new TranslationString("Contributors");
        // private readonly TranslationString _caption = new TranslationString("The application would not be possible without...");

        // [CanBeNull] private IReadOnlyList<GitItemStatus> _currentSelection;

        private FileStatusList _currentFilesList;
        private readonly AsyncLoader _unstagedLoader = new AsyncLoader();

        private void StagedSelectionChanged(object sender, EventArgs e)
        {
            _currentFilesList.ClearSelected();

            // _currentSelection = this._currentFilesList.SelectedItems.Items().ToList();

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
                return Module.GetAllChangedFilesWithSubmodulesStatus();
            }

            if (doAsync)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() =>
                {
                    return _unstagedLoader.LoadAsync(GetAllChangedFilesWithSubmodulesStatus, onComputed);
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
                if (fileStatus.Staged == StagedStatus.WorkTree || fileStatus.IsStatusOnly)
                {
                    // Present status only errors in unstaged
                    unstagedFiles.Add(fileStatus);
                }
            }

            _currentFilesList.SetDiffs(new GitRevision(ObjectId.IndexId), new GitRevision(ObjectId.WorkTreeId), unstagedFiles);
            RestoreSelectedFiles(unstagedFiles);
        }

        private void RestoreSelectedFiles(IReadOnlyList<GitItemStatus> unstagedFiles)
        {
            //
        }

        public FormLocks([NotNull] GitUICommands commands) : base(commands)
        {
            InitialiseComponent();
            InitializeComplete();

            void InitialiseComponent()
            {
                SuspendLayout();
                Controls.Clear();

                _currentFilesList = new GitUI.FileStatusList();

                // this._currentFilesList.ContextMenuStrip = this.StagedFileContext;

                _currentFilesList.Dock = System.Windows.Forms.DockStyle.Fill;
                _currentFilesList.Location = new System.Drawing.Point(0, 28);
                _currentFilesList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
                _currentFilesList.Name = "Staged";
                _currentFilesList.SelectFirstItemOnSetItems = false;
                _currentFilesList.Size = new System.Drawing.Size(397, 314);
                _currentFilesList.TabIndex = 0;
                _currentFilesList.SelectedIndexChanged += new System.EventHandler(StagedSelectionChanged);
                _currentFilesList.DataSourceChanged += new System.EventHandler(Staged_DataSourceChanged);
                _currentFilesList.DoubleClick += new System.EventHandler(Staged_DoubleClick);
                _currentFilesList.Enter += new FileStatusList.EnterEventHandler(Staged_Enter);

                _currentFilesList.SetNoFilesText("no files locked");

                Initialize();

                Controls.Add(_currentFilesList);

                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;
                ClientSize = new Size(624, 442);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;

                // Text = _caption.Text;

                ResumeLayout(false);

                return;
            }
        }
    }
}
