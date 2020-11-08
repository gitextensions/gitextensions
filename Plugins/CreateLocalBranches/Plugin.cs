using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using CreateLocalBranches.Properties;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace CreateLocalBranches
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginExecutable
    {
        public string Name => "Create local tracking branches";

        public string Description => Strings.Description;

        public Image Icon => Images.IconCreateLocalBranches;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public bool Execute(GitUIEventArgs args)
        {
            using var form = new CreateLocalBranchesForm(args);

            if (form.ShowDialog(args.OwnerForm) == DialogResult.OK)
            {
                return true;
            }

            return false;
        }
    }
}
