using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.SubmodulesDialog;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormSubmodules : GitModuleForm
    {
        private readonly TranslationString _removeSelectedSubmodule = new TranslationString("Are you sure you want remove the selected submodule?");
        private readonly TranslationString _removeSelectedSubmoduleCaption = new TranslationString("Remove");

        private readonly BindingList<IGitSubmoduleInfo> _modules = new BindingList<IGitSubmoduleInfo>();
        private GitSubmoduleInfo _oldSubmoduleInfo;

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

            InitializeComplete();
        }

        private void AddSubmoduleClick(object sender, EventArgs e)
        {
            using (var formAddSubmodule = new FormAddSubmodule(UICommands))
            {
                formAddSubmodule.ShowDialog(this);
            }

            Initialize();
        }

        private void FormSubmodulesShown(object sender, EventArgs e)
        {
            Initialize();
        }

        private BackgroundWorker _bw;

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
                foreach (var oldSubmodule in Module.GetSubmodulesInfo())
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

                    if (_oldSubmoduleInfo != null)
                    {
                        DataGridViewRow row = Submodules.Rows
                            .Cast<DataGridViewRow>()
                            .FirstOrDefault(r => r.DataBoundItem as GitSubmoduleInfo == _oldSubmoduleInfo);

                        if (row != null)
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
                FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleSyncCmd(SubModuleLocalPath.Text));
                Initialize();
            }
        }

        private void UpdateSubmoduleClick(object sender, EventArgs e)
        {
            using (WaitCursorScope.Enter())
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleUpdateCmd(SubModuleLocalPath.Text));
                Initialize();
            }
        }

        private void RemoveSubmoduleClick(object sender, EventArgs e)
        {
            if (Submodules.SelectedRows.Count != 1 ||
                MessageBox.Show(this, _removeSelectedSubmodule.Text, _removeSelectedSubmoduleCaption.Text,
                                MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
            {
                return;
            }

            using (WaitCursorScope.Enter())
            {
                Module.UnstageFile(SubModuleLocalPath.Text);

                var modules = Module.GetSubmoduleConfigFile();
                modules.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
                if (modules.ConfigSections.Count > 0)
                {
                    modules.Save();
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
            if (submodule == null)
            {
                return;
            }

            new GitUICommands(submodule).StartPullDialog(this);

            using (WaitCursorScope.Enter())
            {
                Initialize();
            }
        }
    }
}