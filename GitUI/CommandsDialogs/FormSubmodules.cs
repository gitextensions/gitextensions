﻿using System.ComponentModel;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git.Commands;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SubmodulesDialog;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormSubmodules : GitModuleForm
    {
        private readonly SplitterManager _splitterManager = new(new AppSettingsPath("FormSubmodules"));
        private readonly TranslationString _removeSelectedSubmodule = new("Are you sure you want remove the selected submodule?");
        private readonly TranslationString _removeSelectedSubmoduleCaption = new("Remove");

        private readonly BindingList<IGitSubmoduleInfo?> _modules = new();
        private GitSubmoduleInfo? _oldSubmoduleInfo;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormSubmodules()
        {
            InitializeComponent();
        }

        public FormSubmodules(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            nameDataGridViewTextBoxColumn.DataPropertyName = nameof(GitSubmoduleInfo.Name);
            Status.DataPropertyName = nameof(GitSubmoduleInfo.Status);
            gitSubmoduleBindingSource.DataSource = _modules;
            splitContainer1.SplitterDistance = DpiUtil.Scale(222);
            Pull.AdaptImageLightness();
            InitializeComplete();
        }

        protected override void OnLoad(EventArgs e)
        {
            _splitterManager.AddSplitter(splitContainer1, nameof(splitContainer1));
            _splitterManager.RestoreSplitters();
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _splitterManager.SaveSplitters();
            base.OnClosing(e);
        }

        private void AddSubmoduleClick(object sender, EventArgs e)
        {
            using (FormAddSubmodule formAddSubmodule = new(UICommands))
            {
                formAddSubmodule.ShowDialog(this);
            }

            Initialize();
        }

        private void FormSubmodulesShown(object sender, EventArgs e)
        {
            Initialize();
        }

        private BackgroundWorker? _bw;

        private void Initialize()
        {
            _bw?.CancelAsync();
            var waitScope = WaitCursorScope.Enter();
            _oldSubmoduleInfo = null;
            if (Submodules.SelectedRows.Count == 1)
            {
                _oldSubmoduleInfo = Submodules.SelectedRows[0].DataBoundItem as GitSubmoduleInfo;
            }

            lock (_modules)
            {
                _modules.Clear();
            }

            _bw = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _bw.DoWork += (sender, e) =>
            {
                foreach (var oldSubmodule in Module.GetSubmodulesInfo().Where(submodule => submodule is not null))
                {
                    if (_bw.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    _bw.ReportProgress(0, oldSubmodule);
                }
            };
            _bw.ProgressChanged += (sender, e) =>
            {
                lock (_modules)
                {
                    lock (_modules)
                    {
                        _modules.Add(e.UserState as GitSubmoduleInfo);
                    }

                    if (_oldSubmoduleInfo is not null)
                    {
                        DataGridViewRow row = Submodules.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.DataBoundItem as GitSubmoduleInfo == _oldSubmoduleInfo);

                        if (row is not null)
                        {
                            row.Selected = true;
                        }
                    }
                }
            };
            _bw.RunWorkerCompleted += (_, e) => waitScope.Dispose();
            _bw.RunWorkerAsync();
        }

        private void SynchronizeSubmoduleClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.SubmoduleSyncCmd(SubModuleLocalPath.Text), Module.WorkingDir, input: null, useDialogSettings: true);
                Initialize();
            }
        }

        private void UpdateSubmoduleClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.SubmoduleUpdateCmd(SubModuleLocalPath.Text), Module.WorkingDir, input: null, useDialogSettings: true);
                Initialize();
            }
        }

        private void RemoveSubmoduleClick(object sender, EventArgs e)
        {
            if (Submodules.SelectedRows.Count != 1 ||
                MessageBox.Show(this, _removeSelectedSubmodule.Text, _removeSelectedSubmoduleCaption.Text,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) !=
                DialogResult.Yes)
            {
                return;
            }

            using (WaitCursorScope.Enter())
            {
                Module.UnstageFile(SubModuleLocalPath.Text);

                ConfigFile submoduleConfigFile;
                try
                {
                    submoduleConfigFile = Module.GetSubmoduleConfigFile();
                }
                catch (GitConfigurationException ex)
                {
                    MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
                    return;
                }

                submoduleConfigFile.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
                if (submoduleConfigFile.ConfigSections.Count > 0)
                {
                    submoduleConfigFile.Save();
                    Module.StageFile(".gitmodules");
                }
                else
                {
                    Module.UnstageFile(".gitmodules");
                }

                var configFile = Module.LocalConfigFile;
                configFile.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
                configFile.Save();

                Initialize();
            }
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            var submodule = Module.GetSubmodule(SubModuleLocalPath.Text);

            new GitUICommands(submodule).StartPullDialog(this);

            using (WaitCursorScope.Enter())
            {
                Initialize();
            }
        }
    }
}
