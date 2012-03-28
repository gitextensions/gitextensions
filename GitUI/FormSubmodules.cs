using System;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using GitCommands.Config;

namespace GitUI
{
    public partial class FormSubmodules : GitExtensionsForm
    {
        private readonly TranslationString _removeSelectedSubmodule =
             new TranslationString("Are you sure you want remove the selected submodule?");

        private readonly TranslationString _removeSelectedSubmoduleCaption = new TranslationString("Remove");

        public FormSubmodules()
        {
            InitializeComponent();
            Translate();
        }

        private void FormSubmodulesFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("submodules");
        }

        private void FormSubmodulesLoad(object sender, EventArgs e)
        {
            RestorePosition("submodules");
        }

        private void AddSubmoduleClick(object sender, EventArgs e)
        {
            var formAddSubmodule = new FormAddSubmodule();
            formAddSubmodule.ShowDialog(this);
            Initialize();
        }

        private void FormSubmodulesShown(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            Cursor.Current = Cursors.WaitCursor;
            Submodules.DataSource = (new GitCommandsInstance()).GetSubmodules();
            Cursor.Current = Cursors.Default;
        }

        private void SubmodulesSelectionChanged(object sender, EventArgs e)
        {
            if (Submodules.SelectedRows.Count != 1)
                return;

            var submodule = Submodules.SelectedRows[0].DataBoundItem as GitSubmodule;
            if (submodule == null)
                return;

            Cursor.Current = Cursors.WaitCursor;
            SubModuleName.Text = submodule.Name;
            SubModuleRemotePath.Text = submodule.RemotePath;
            SubModuleLocalPath.Text = submodule.LocalPath;
            SubModuleCommit.Text = submodule.CurrentCommitGuid;
            SubModuleBranch.Text = submodule.Branch;
            SubModuleStatus.Text = submodule.Status;
            Cursor.Current = Cursors.Default;
        }

        private void SynchronizeSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(SubModuleName.Text));
            process.ShowDialog(this);
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void InitSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommandHelpers.SubmoduleUpdateCmd(SubModuleName.Text));
            process.ShowDialog(this);
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void UpdateSubmoduleClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommandHelpers.SubmoduleUpdateCmd(SubModuleName.Text));
            process.ShowDialog(this);
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void RemoveSubmoduleClick(object sender, EventArgs e)
        {
            if (Submodules.SelectedRows.Count != 1 ||
                MessageBox.Show(this, _removeSelectedSubmodule.Text, _removeSelectedSubmoduleCaption.Text, MessageBoxButtons.YesNo) !=
                DialogResult.Yes)
                return;

            Cursor.Current = Cursors.WaitCursor;
            Settings.Module.RunGitCmd("rm --cached \"" + SubModuleName.Text + "\"");

            var modules = new ConfigFile(Settings.WorkingDir + ".gitmodules");
            modules.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
            if (modules.GetConfigSections().Count > 0)
                modules.Save();
            else
                Settings.Module.RunGitCmd("rm --cached \".gitmodules\"");

            var configFile = Settings.Module.GetLocalConfig();
            configFile.RemoveConfigSection("submodule \"" + SubModuleName.Text + "\"");
            configFile.Save();

            Initialize();
            Cursor.Current = Cursors.Default;
        }
    }
}