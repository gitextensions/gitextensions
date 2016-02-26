﻿using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRevertCommit : GitModuleForm
    {
        private readonly TranslationString _noneParentSelectedText =  new TranslationString("None parent is selected!");
        private readonly TranslationString _noneParentSelectedTextCaption = new TranslationString("Error");

        private bool _isMerge;

        public FormRevertCommit(GitUICommands aCommands, GitRevision Revision)
            : base(aCommands)
        {
            this.Revision = Revision;

            InitializeComponent();
            Translate();
        }

        public GitRevision Revision { get; set; }

        private void FormRevertCommit_Load(object sender, EventArgs e)
        {
            commitSummaryUserControl1.Revision = Revision;

            ParentsList.Items.Clear(); // TODO: search this line and the ones below to find code duplication

            _isMerge = Module.IsMerge(Revision.Guid);
            parentListPanel.Visible = _isMerge;
            if (_isMerge)
            {
                var parents = Module.GetParentsRevisions(Revision.Guid);

                for (int i = 0; i < parents.Length; i++)
                {
                    var item = new ListViewItem(i + 1 + "");
                    item.SubItems.Add(parents[i].Subject);
                    item.SubItems.Add(parents[i].Author);
                    item.SubItems.Add(parents[i].CommitDate.ToShortDateString());
                    ParentsList.Items.Add(item);
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

            FormProcess.ShowDialog(this, GitCommandHelpers.RevertCmd(Revision.Guid, AutoCommit.Checked, parentIndex));
            MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
