using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace CreateLocalBranches
{
	public class CreateLocalBranchesPlugin : IGitPlugin
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
			foreach (string reference in gitUiCommands.GitCommands.RunGit("branch -a").Split('\n'))
			{
				try
				{
					if (string.IsNullOrEmpty(reference)) continue;

					string branchName = reference.Trim('*', ' ', '\n', '\r');

					if (branchName.StartsWith("remotes"))
						gitUiCommands.GitCommands.RunGit(string.Concat("branch --track ", branchName.Replace("remotes/origin/", ""), " ", branchName));
				}
				catch
				{ 
				}
			}

        }
    }
}
