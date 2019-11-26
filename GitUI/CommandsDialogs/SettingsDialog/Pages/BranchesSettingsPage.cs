using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class BranchesSettingsPage : SettingsPageWithHeader
    {
        public BranchesSettingsPage()
        {
            InitializeComponent();
            Text = "Branches";
            InitializeComplete();
        }

        protected override void SettingsToPage()
        {
            cbBranchOrderingCriteria.SelectedIndex = (int)AppSettings.BranchOrderingCriteria;
        }

        protected override void PageToSettings()
        {
            AppSettings.BranchOrderingCriteria = (BranchOrdering)cbBranchOrderingCriteria.SelectedIndex;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(BranchesSettingsPage));
        }
    }
}