using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCherryPick : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _noneParentSelectedText =
            new("None parent is selected!");
        #endregion

        private bool _isMerge;

        public GitRevision? Revision { get; set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCherryPick()
        {
            InitializeComponent();
        }

        public FormCherryPick(GitUICommands commands, GitRevision? revision)
            : base(commands)
        {
            Revision = revision;
            InitializeComponent();
            InitializeComplete();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LoadSettings();
            OnRevisionChanged();
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            AutoCommit.Checked = AppSettings.CommitAutomaticallyAfterCherryPick;
            checkAddReference.Checked = AppSettings.AddCommitReferenceToCherryPick;
        }

        private void SaveSettings()
        {
            AppSettings.CommitAutomaticallyAfterCherryPick = AutoCommit.Checked;
            AppSettings.AddCommitReferenceToCherryPick = checkAddReference.Checked;
        }

        private void OnRevisionChanged()
        {
            commitSummaryUserControl1.Revision = Revision;

            ParentsList.Items.Clear();

            if (Revision is not null)
            {
                _isMerge = Module.IsMerge(Revision.ObjectId);
            }

            panelParentsList.Visible = _isMerge;

            if (_isMerge && Revision is not null)
            {
                var parents = Module.GetParentRevisions(Revision.ObjectId);

                for (int i = 0; i < parents.Count; i++)
                {
                    ParentsList.Items.Add(new ListViewItem((i + 1).ToString())
                    {
                        SubItems =
                        {
                            parents[i].Subject,
                            parents[i].Author,
                            parents[i].CommitDate.ToShortDateString()
                        }
                    });
                }

                ParentsList.TopItem.Selected = true;
                Size size = MinimumSize;
                size.Height += 100;
                MinimumSize = size;
            }
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            ArgumentBuilder args = new();
            var canExecute = true;

            if (_isMerge)
            {
                if (ParentsList.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    canExecute = false;
                }
                else
                {
                    args.Add("-m " + (ParentsList.SelectedItems[0].Index + 1));
                }
            }

            if (checkAddReference.Checked)
            {
                args.Add("-x");
            }

            if (canExecute && Revision is not null)
            {
                var command = GitCommandHelpers.CherryPickCmd(Revision.ObjectId, AutoCommit.Checked, args.ToString());

                // Don't verify whether the command is successful.
                // If it fails, likely there is a conflict that needs to be resolved.
                FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

                MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void CopyOptions(FormCherryPick source)
        {
            AutoCommit.Checked = source.AutoCommit.Checked;
            checkAddReference.Checked = source.checkAddReference.Checked;
        }

        private void btnChooseRevision_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, Revision?.Guid))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision is not null)
                {
                    Revision = chooseForm.SelectedRevision;
                }
            }

            OnRevisionChanged();
        }
    }
}
