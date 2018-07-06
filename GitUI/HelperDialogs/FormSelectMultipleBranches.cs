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
            InitializeComplete();
        }

        public FormSelectMultipleBranches(IReadOnlyList<IGitRef> branchesToSelect)
        {
            InitializeComponent();
            InitializeComplete();

            if (branchesToSelect.Count > 350)
            {
                Branches.MultiColumn = true;
            }

            Branches.DisplayMember = nameof(IGitRef.Name);
            Branches.Items.AddRange(branchesToSelect.ToArray());
        }

        public void SelectBranch(string name)
        {
            int index = 0;
            foreach (object item in Branches.Items)
            {
                if (item is IGitRef branch && branch.Name == name)
                {
                    Branches.SetItemChecked(index, true);
                    return;
                }

                index++;
            }
        }

        public IReadOnlyList<IGitRef> GetSelectedBranches()
        {
            var branches = new List<IGitRef>();

            foreach (IGitRef head in Branches.CheckedItems)
            {
                branches.Add(head);
            }

            return branches;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
