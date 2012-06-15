using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class BranchComboBox : GitExtensionsControl
    {
        public BranchComboBox()
        {
            InitializeComponent();
            Translate();

            branches.DisplayMember = "Name";
        }

        private IList<GitHead> _branchesToSelect;
        public IList<GitHead> BranchesToSelect 
        {
            get
            {
                return _branchesToSelect;
            }
            set
            {
                _branchesToSelect = value;
                LoadBranches();
            }
        }

        private void LoadBranches()
        {
            if (_branchesToSelect != null)
                branches.Items.AddRange(_branchesToSelect.ToArray());
        }

        public IList<GitHead> GetSelectedBranches()
        {
            IList<GitHead> selectedBranches = new List<GitHead>();
            if (!string.IsNullOrEmpty(branches.Text))
            {
                foreach (string branch in branches.Text.Split(new char[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    GitHead gitHead = _branchesToSelect.FirstOrDefault(g => g.Name == branch);
                    if (gitHead == null)
                        MessageBox.Show("Branch '" + branch + "' is not selectable, this branch has been removed from the selection.");
                    else
                        selectedBranches.Add(gitHead);
                }
            }

            return selectedBranches;
        }

        public string GetSelectedText()
        {
            return branches.Text;
        }

        public void SetSelectedText(string text)
        {
            branches.Text = text;
        }

        private void selectMultipleBranchesButton_Click(object sender, EventArgs e)
        {
            using (FormSelectMultipleBranches formSelectMultipleBranches = new FormSelectMultipleBranches(_branchesToSelect))
            {
                foreach (GitHead branch in GetSelectedBranches())
                    formSelectMultipleBranches.SelectBranch(branch.Name);
                formSelectMultipleBranches.ShowDialog(this);
                string branchesText = string.Empty;
                foreach (GitHead branch in formSelectMultipleBranches.GetSelectedBranches())
                {
                    if (!string.IsNullOrEmpty(branchesText))
                        branchesText += " ";
                    branchesText += branch.Name;
                }

                branches.Text = branchesText;
            }
        }
    }
}
