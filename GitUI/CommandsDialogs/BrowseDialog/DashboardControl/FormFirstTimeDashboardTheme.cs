namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public sealed partial class FormFirstTimeDashboardTheme : GitModuleForm
    {
        public FormFirstTimeDashboardTheme()
        {
            InitializeComponent();
            Translate();

            cboDashboardTheme.SelectedIndex = 0;
        }


        /// <summary>
        /// Gets the index of the selected dashboard theme.
        /// </summary>
        public int SelectedThemeIndex => cboDashboardTheme.SelectedIndex;
    }
}