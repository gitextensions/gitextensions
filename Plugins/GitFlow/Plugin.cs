using System.ComponentModel.Composition;
using System.Drawing;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitFlow.Properties;

namespace GitFlow
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public string Name => "GitFlow";

        public string Description => Strings.Description;

        public Image Icon => Images.IconGitFlow;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public bool Execute(GitUIEventArgs args)
        {
            using var frm = new GitFlowForm(args);

            frm.ShowDialog(args.OwnerForm);

            return frm.IsRefreshNeeded;
        }
    }
}
