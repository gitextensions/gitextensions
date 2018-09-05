using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCherryPick : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _noneParentSelectedText =
            new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption =
            new TranslationString("Error");
        #endregion

        private bool _isMerge;

        [CanBeNull]
        public GitRevision Revision { get; set; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCherryPick()
        {
            InitializeComponent();
        }

        public FormCherryPick(GitUICommands commands, [CanBeNull] GitRevision revision)
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

            if (Revision != null)
            {
                _isMerge = Module.IsMerge(Revision.ObjectId);
            }

            panelParentsList.Visible = _isMerge;

            if (_isMerge)
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
            var args = new ArgumentBuilder();
            var canExecute = true;

            if (_isMerge)
            {
                if (ParentsList.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, _noneParentSelectedTextCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (canExecute)
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.CherryPickCmd(Revision.ObjectId, AutoCommit.Checked, args.ToString()));
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
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision != null)
                {
                    Revision = chooseForm.SelectedRevision;
                }
            }

            OnRevisionChanged();
        }
    }
}
