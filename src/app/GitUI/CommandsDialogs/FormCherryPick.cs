using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
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
        private int _lblParentsControlHeight;
        private int _lvParentsListControlHeight;

        private const int _parentsListItemHeight = 18;

        public GitRevision? Revision { get; set; }

        public FormCherryPick(IGitUICommands commands, GitRevision? revision)
            : base(commands, enablePositionRestore: false)
        {
            Revision = revision;

            InitializeComponent();

            columnHeader1.Width = DpiUtil.Scale(columnHeader1.Width);
            columnHeader2.Width = DpiUtil.Scale(columnHeader2.Width);
            columnHeader3.Width = DpiUtil.Scale(columnHeader3.Width);
            columnHeader4.Width = DpiUtil.Scale(columnHeader4.Width);

            InitializeComplete();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            _lblParentsControlHeight = lblParents.Size.Height;
            _lvParentsListControlHeight = lvParentsList.Size.Height;

            LoadSettings();
            OnRevisionChanged();
        }

        private void Form_Shown(object? sender, EventArgs e)
        {
            if (lvParentsList.Visible)
            {
                lvParentsList.Focus();
            }
            else
            {
                cbxAutoCommit.Focus();
            }
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
                SuspendLayout();

                commitSummaryUserControl1.Revision = Revision;

                lvParentsList.Items.Clear();

                if (Revision is not null)
                {
                    _isMerge = Module.IsMerge(Revision.ObjectId);
                }

                // We need to hide these optional components first to get a correct base value of PreferredMinimumHeight
                lblParents.Visible = false;
                lvParentsList.Visible = false;

                if (_isMerge)
                {
                    MinimumSize = new Size(MinimumSize.Width, PreferredMinimumHeight + _lblParentsControlHeight + _lvParentsListControlHeight);
                    Size = MinimumSize;
                    lblParents.Visible = true;
                    lvParentsList.Visible = true;
                }
                else
                {
                    MinimumSize = new Size(MinimumSize.Width, PreferredMinimumHeight - _lblParentsControlHeight - _lvParentsListControlHeight);
                    Size = MinimumSize;
                }

                if (_isMerge && Revision is not null)
                {
                    IReadOnlyList<GitRevision> parents = Module.GetParentRevisions(Revision.ObjectId);

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
                    size.Height += DpiUtil.Scale(_parentsListItemHeight * parents.Count);
                    Size = size;
                    MinimumSize = size;
                }
            }
            finally
            {
                ResumeLayout(performLayout: true);
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnPick_Click(object sender, EventArgs e)
        {
            ArgumentBuilder args = [];
            bool canExecute = true;

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
                ArgumentString command = Commands.CherryPick(Revision.ObjectId, cbxAutoCommit.Checked, args.ToString());

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
