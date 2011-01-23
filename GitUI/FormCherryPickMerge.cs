using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormCherryPickMerge : Form
    {
        public bool OkClicked;
        public FormCherryPickMerge(GitRevision[] parents)
        {
            InitializeComponent();

            for (int i=0; i<parents.Length;i++)
            {
                ParentsList.Items.Add(i + 1+"");
                ParentsList.Items[ParentsList.Items.Count-1].SubItems.Add(parents[i].Message);
                ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].Author);
                ParentsList.Items[ParentsList.Items.Count - 1].SubItems.Add(parents[i].CommitDate.ToShortDateString());
            }
            ParentsList.TopItem.Selected = true;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (ParentsList.SelectedItems.Count == 0)
                MessageBox.Show("None parent is selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                OkClicked = true;
                Close();
            }

        }
        
    }
}
