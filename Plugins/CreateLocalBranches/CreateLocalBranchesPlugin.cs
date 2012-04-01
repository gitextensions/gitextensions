using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
	public class CreateLocalBranchesPlugin : IGitPluginForRepository
    {
        public string Description
        {
            get { return "Create local tracking branches"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            new CreateLocalBranchesForm(gitUiCommands).ShowDialog();
        }
    }
}
