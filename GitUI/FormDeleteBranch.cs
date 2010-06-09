using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormDeleteBranch : GitExtensionsForm
    {
        TranslationString deleteBranchQuestion = new TranslationString("Are you sure you want to delete this branch?\nDeleting a branch can cause commits to be deleted too!");
        TranslationString deleteBranchCaption = new TranslationString("Delete branch");
        TranslationString branchDeleted = new TranslationString("Command executed");

        private System.ComponentModel.ComponentResourceManager resources;
        private readonly string defaultBranch;

        public FormDeleteBranch(string defaultBranch)
        {
            resources = new ComponentResourceManager(typeof(FormDeleteBranch));

            InitializeComponent();
            this.defaultBranch = defaultBranch;
        }

        private void FormDeleteBranch_Load(object sender, EventArgs e)
        {
            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(false, true);

            if (defaultBranch != null)
            {
                Branches.Text = defaultBranch;
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(deleteBranchQuestion.Text, deleteBranchCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show(branchDeleted + Environment.NewLine + GitCommands.GitCommands.DeleteBranch(Branches.Text, ForceDelete.Checked), deleteBranchCaption.Text);
                }
            }
            catch
            {
            }
            Close();
        }
    }
}
