using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormSelectMultipleBranches : GitExtensionsForm
    {
        public FormSelectMultipleBranches(IList<GitHead> branchesToSelect)
        {
            InitializeComponent();
            Translate();

            Branches.DisplayMember = "Name";
            Branches.Items.AddRange(branchesToSelect.ToArray());
        }

        public void SelectBranch(string name)
        {
            int index = 0;
            foreach (object item in Branches.Items)
            {
                GitHead branch = item as GitHead;
                if (branch != null && branch.Name == name)
                {
                    Branches.SetItemChecked(index, true);
                    return;
                }
                index++;
            }
        }

        public IList<GitHead> GetSelectedBranches()
        {
            IList<GitHead> branches = new List<GitHead>();

            foreach (GitHead head in Branches.CheckedItems)
                branches.Add(head);

            return branches;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
