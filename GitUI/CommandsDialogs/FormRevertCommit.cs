using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRevertCommit : GitModuleForm
    {
        private readonly TranslationString _noneParentSelectedText = new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption = new TranslationString("Error");

        private bool _isMerge;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormRevertCommit()
        {
            InitializeComponent();
        }

        public FormRevertCommit(GitUICommands commands, GitRevision revision)
            : base(commands)
        {
            Revision = revision;

            InitializeComponent();
            InitializeComplete();
        }

        public GitRevision Revision { get; set; }

        private void FormRevertCommit_Load(object sender, EventArgs e)
        {
            commitSummaryUserControl1.Revision = Revision;

            ParentsList.Items.Clear(); // TODO: search this line and the ones below to find code duplication

            _isMerge = Module.IsMerge(Revision.ObjectId);
            parentListPanel.Visible = _isMerge;
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
            var parentIndex = 0;
            if (_isMerge)
            {
                if (ParentsList.SelectedItems.Count != 1)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, _noneParentSelectedTextCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    parentIndex = ParentsList.SelectedItems[0].Index + 1;
                }
            }

            FormProcess.ShowDialog(this, GitCommandHelpers.RevertCmd(Revision.ObjectId, AutoCommit.Checked, parentIndex));
            MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
