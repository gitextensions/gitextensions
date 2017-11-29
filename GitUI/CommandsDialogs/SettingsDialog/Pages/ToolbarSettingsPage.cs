﻿using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ToolbarSettingsPage : SettingsPageWithHeader
    {
        public ToolbarSettingsPage()
        {
            InitializeComponent();
            Text = "Toolbar";
            Translate();
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
            return new SettingsPageReferenceByType(typeof(ToolbarSettingsPage));
        }
    }
}