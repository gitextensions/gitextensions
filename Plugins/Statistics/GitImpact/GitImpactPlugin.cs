using System;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public class GitImpactPlugin : IGitPlugin
    {
        #region IGitPlugin Members

        public string Description
        {
            get { return "Impact Graph"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
        }

        public void Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (string.IsNullOrEmpty(gitUiCommands.GitWorkingDir))
                return;

            FormImpact form = new FormImpact();
            form.ShowDialog();
        }

        #endregion
    }
}
