using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Git.Commands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRevertCommit : GitModuleForm
    {
        private readonly TranslationString _noneParentSelectedText = new("None parent is selected!");

        private bool _isMerge;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormRevertCommit()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

        public GitRevision Revision { get; }

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
                    MessageBox.Show(this, _noneParentSelectedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    parentIndex = ParentsList.SelectedItems[0].Index + 1;
                }
            }

            var command = GitCommandHelpers.RevertCmd(Revision.ObjectId, AutoCommit.Checked, parentIndex);

            // Don't verify whether the command is successful.
            // If it fails, likely there is a conflict that needs to be resolved.
            FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

            MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
