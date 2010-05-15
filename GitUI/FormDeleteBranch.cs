using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormDeleteBranch : GitExtensionsForm
    {
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
                if (MessageBox.Show(resources.GetString("msg:delete branch"), "Delete branch", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show(resources.GetString("msg:deleted") + Environment.NewLine + GitCommands.GitCommands.DeleteBranch(Branches.Text, ForceDelete.Checked), "Delete branch");
                }
            }
            catch
            {
            }
            Close();
        }
    }
}
