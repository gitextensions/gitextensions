using System.ComponentModel;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.HelperDialogs;
using Microsoft;
using ResourceManager;

namespace GitUI
{
    public partial class BranchComboBox : GitExtensionsControl
    {
        private readonly TranslationString _branchCheckoutError = new("Branch '{0}' is not selectable, this branch has been removed from the selection.");

        public BranchComboBox()
        {
            InitializeComponent();
            InitializeComplete();

            branches.DisplayMember = nameof(IGitRef.Name);
        }

        /// <summary>
        /// Occurs whenever the branch selection has changed.
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the branch selection has changed.")]
        public event EventHandler SelectedValueChanged;

        private IReadOnlyList<IGitRef>? _branchesToSelect;
        public IReadOnlyList<IGitRef>? BranchesToSelect
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
            if (_branchesToSelect is not null)
            {
                branches.Items.AddRange(_branchesToSelect.ToArray());
            }
        }

        public IEnumerable<IGitRef> GetSelectedBranches()
        {
            foreach (string branch in branches.Text.LazySplit(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                IGitRef gitHead = _branchesToSelect.FirstOrDefault(g => g.Name == branch);
                if (gitHead is null)
                {
                    MessageBox.Show(string.Format(_branchCheckoutError.Text, branch), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    yield return gitHead;
                }
            }
        }

        public string GetSelectedText() => branches.Text;

        public void SetSelectedText(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            branches.Text = text;
        }

        private void selectMultipleBranchesButton_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_branchesToSelect);

            using FormSelectMultipleBranches formSelectMultipleBranches = new(_branchesToSelect);
            foreach (IGitRef branch in GetSelectedBranches())
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

            OnSelectedValueChanged();
        }

        private void branches_SelectedValueChanged(object sender, EventArgs e)
            => OnSelectedValueChanged();

        private void OnSelectedValueChanged() => SelectedValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
