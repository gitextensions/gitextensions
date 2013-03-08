﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormSubmodules : GitModuleForm
    {
        private readonly TranslationString _removeSelectedSubmodule =
             new TranslationString("Are you sure you want remove the selected submodule?");

        private readonly TranslationString _removeSelectedSubmoduleCaption = new TranslationString("Remove");

        private BindingList<IGitSubmodule> modules = new BindingList<IGitSubmodule>();
        private GitSubmodule oldSubmodule;

        public FormSubmodules(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            gitSubmoduleBindingSource.DataSource = modules;
        }

        private void AddSubmoduleClick(object sender, EventArgs e)
        {
            using (var formAddSubmodule = new FormAddSubmodule(UICommands))
                formAddSubmodule.ShowDialog(this);
            Initialize();
        }

        private void FormSubmodulesShown(object sender, EventArgs e)
        {
            Initialize();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            foreach (var oldSubmodule in Module.GetSubmodules())
            {
                if (bw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                bw.ReportProgress(0, oldSubmodule);
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lock (modules)
            {
                lock (modules)
                    modules.Add(e.UserState as GitSubmodule);
                if (oldSubmodule != null)
                {
                    DataGridViewRow row = Submodules.Rows
                        .Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.DataBoundItem as GitSubmodule == oldSubmodule);

                    if (row != null)
                        row.Selected = true;
                }
            }
        }

        private void bw_RunWorkerCompleted(object sender, EventArgs e)
        {
            UseWaitCursor = false;
        }

        private BackgroundWorker bw;

        private void Initialize()
        {
            if (bw != null)
                bw.CancelAsync();
            UseWaitCursor = true;
            oldSubmodule = null;
            if (Submodules.SelectedRows.Count == 1)
                oldSubmodule = Submodules.SelectedRows[0].DataBoundItem as GitSubmodule;
            lock (modules)
                modules.Clear();
            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerAsync();
        }

        private void SynchronizeSubmoduleClick(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleSyncCmd(SubModuleLocalPath.Text));
            Initialize();
            UseWaitCursor = false;
        }

        private void UpdateSubmoduleClick(object sender, EventArgs e)
        {
            UseWaitCursor = true;
            FormProcess.ShowDialog(this, GitCommandHelpers.SubmoduleUpdateCmd(SubModuleLocalPath.Text));
            Initialize();
            UseWaitCursor = false;
        }

        private void RemoveSubmoduleClick(object sender, EventArgs e)
        {
            if (Submodules.SelectedRows.Count != 1 ||
                MessageBox.Show(this, _removeSelectedSubmodule.Text, _removeSelectedSubmoduleCaption.Text,
                                MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            UseWaitCursor = true;
            Module.UnstageFile(SubModuleLocalPath.Text);

            var modules = Module.GetSubmoduleConfigFile();
            modules.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
            if (modules.GetConfigSections().Count > 0)
            {
                modules.Save();
                Module.StageFile(".gitmodules");
            }
            else
                Module.UnstageFile(".gitmodules");

            var configFile = Module.GetLocalConfig();
            configFile.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
            configFile.Save();

            Initialize();
            UseWaitCursor = false;
        }
    }
}