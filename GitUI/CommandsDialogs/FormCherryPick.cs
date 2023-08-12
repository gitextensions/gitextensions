using GitCommands;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCherryPick : GitExtensionsDialog
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
            : base(commands, enablePositionRestore: false)
        {
            Revision = revision;
            InitializeComponent();
            Size = MinimumSize;
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
            cbxAutoCommit.Checked = AppSettings.CommitAutomaticallyAfterCherryPick;
            cbxAddReference.Checked = AppSettings.AddCommitReferenceToCherryPick;
        }

        private void SaveSettings()
        {
            if (DialogResult == DialogResult.OK)
            {
                AppSettings.CommitAutomaticallyAfterCherryPick = cbxAutoCommit.Checked;
                AppSettings.AddCommitReferenceToCherryPick = cbxAddReference.Checked;
            }
        }

        private void OnRevisionChanged()
        {
            try
            {
                tlpnlMain.SuspendLayout();

                commitSummaryUserControl1.Revision = Revision;

                lvParentsList.Items.Clear();

                if (Revision is not null)
                {
                    _isMerge = Module.IsMerge(Revision.ObjectId);
                }

                lblParents.Visible = _isMerge;
                lvParentsList.Visible = _isMerge;

                if (_isMerge && Revision is not null)
                {
                    var parents = Module.GetParentRevisions(Revision.ObjectId);

                    for (int i = 0; i < parents.Count; i++)
                    {
                        lvParentsList.Items.Add(new ListViewItem((i + 1).ToString())
                        {
                            SubItems =
                        {
                            parents[i].Subject,
                            parents[i].Author,
                            parents[i].CommitDate.ToShortDateString()
                        }
                        });
                    }

                    lvParentsList.TopItem.Selected = true;
                    Size size = MinimumSize;
                    size.Height += 100;
                    MinimumSize = size;
                }
            }
            finally
            {
                tlpnlMain.ResumeLayout(performLayout: true);
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnPick_Click(object sender, EventArgs e)
        {
            ArgumentBuilder args = new();
            var canExecute = true;

            if (_isMerge)
            {
                if (lvParentsList.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    canExecute = false;
                }
                else
                {
                    args.Add("-m " + (lvParentsList.SelectedItems[0].Index + 1));
                }
            }

            if (cbxAddReference.Checked)
            {
                args.Add("-x");
            }

            if (canExecute && Revision is not null)
            {
                var command = GitCommandHelpers.CherryPickCmd(Revision.ObjectId, cbxAutoCommit.Checked, args.ToString());

                // Don't verify whether the command is successful.
                // If it fails, likely there is a conflict that needs to be resolved.
                FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

                MergeConflictHandler.HandleMergeConflicts(UICommands, this, cbxAutoCommit.Checked);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void CopyOptions(FormCherryPick source)
        {
            cbxAutoCommit.Checked = source.cbxAutoCommit.Checked;
            cbxAddReference.Checked = source.cbxAddReference.Checked;
        }

        private void btnChooseRevision_Click(object sender, EventArgs e)
        {
            using (FormChooseCommit chooseForm = new(UICommands, Revision?.Guid))
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
