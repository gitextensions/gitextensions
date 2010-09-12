using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesPlugin : IGitPlugin
    {
        public string Description
        {
            get { return "Delete obsolete branches"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            Settings.AddSetting("Delete obsolete branches older than (days)", "30");
        }

        public void Execute(IGitUIEventArgs gitUiCommands)
        {
            int days;
            if (!int.TryParse(Settings.GetSetting("Delete obsolete branches older than (days)"), out days))
                days = 30;

            new DeleteUnusedBranchesForm(days, gitUiCommands.GitCommands).ShowDialog();
        }
    }
}
