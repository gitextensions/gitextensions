using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

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

        public FormSelectMultipleBranches(IList<IGitRef> branchesToSelect)
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
                var branch = item as IGitRef;
                if (branch != null && branch.Name == name)
                {
                    Branches.SetItemChecked(index, true);
                    return;
                }
                index++;
            }
        }

        public IList<IGitRef> GetSelectedBranches()
        {
            IList<IGitRef> branches = new List<IGitRef>();

            foreach (IGitRef head in Branches.CheckedItems)
                branches.Add(head);

            return branches;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
