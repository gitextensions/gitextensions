using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI
{
    public partial class BranchComboBox : GitExtensionsControl
    {
        private readonly TranslationString _branchCheckoutError = new TranslationString("Branch '{0}' is not selectable, this branch has been removed from the selection.");

        public BranchComboBox()
        {
            InitializeComponent();
            InitializeComplete();

            branches.DisplayMember = nameof(IGitRef.Name);
        }

        private IReadOnlyList<IGitRef> _branchesToSelect;
        public IReadOnlyList<IGitRef> BranchesToSelect
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
            {
                branches.Items.AddRange(_branchesToSelect.ToArray());
            }
        }

        public IEnumerable<IGitRef> GetSelectedBranches()
        {
            foreach (string branch in branches.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var gitHead = _branchesToSelect.FirstOrDefault(g => g.Name == branch);
                if (gitHead == null)
                {
                    MessageBox.Show(string.Format(_branchCheckoutError.Text, branch));
                }
                else
                {
                    yield return gitHead;
                }
            }
        }

        public string GetSelectedText()
        {
            return branches.Text;
        }

        public void SetSelectedText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            branches.Text = text;
        }

        private void selectMultipleBranchesButton_Click(object sender, EventArgs e)
        {
            using (var formSelectMultipleBranches = new FormSelectMultipleBranches(_branchesToSelect))
            {
                foreach (var branch in GetSelectedBranches())
                {
                    formSelectMultipleBranches.SelectBranch(branch.Name);
                }

                formSelectMultipleBranches.ShowDialog(this);
                string branchesText = string.Empty;
                foreach (GitRef branch in formSelectMultipleBranches.GetSelectedBranches())
                {
                    if (!string.IsNullOrEmpty(branchesText))
                    {
                        branchesText += " ";
                    }

                    branchesText += branch.Name;
                }

                branches.Text = branchesText;
            }
        }
    }
}
