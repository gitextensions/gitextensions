using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormSelectMultipleBranches : GitExtensionsForm
    {
        // only for translation
        private FormSelectMultipleBranches()
            : base(true)
        {
            InitializeComponent();
            Translate();
        }

        public FormSelectMultipleBranches(IList<GitRef> branchesToSelect)
        {
            InitializeComponent();
            Translate();

            if (branchesToSelect.Count > 350)
                Branches.MultiColumn = true;

            Branches.DisplayMember = "Name";
            Branches.Items.AddRange(branchesToSelect.ToArray());
        }

        public void SelectBranch(string name)
        {
            int index = 0;
            foreach (object item in Branches.Items)
            {
                GitRef branch = item as GitRef;
                if (branch != null && branch.Name == name)
                {
                    Branches.SetItemChecked(index, true);
                    return;
                }
                index++;
            }
        }

        public IList<GitRef> GetSelectedBranches()
        {
            IList<GitRef> branches = new List<GitRef>();

            foreach (GitRef head in Branches.CheckedItems)
                branches.Add(head);

            return branches;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
