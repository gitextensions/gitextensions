using System.ComponentModel.Composition;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Plugins.ReleaseNotesGenerator.Properties;

namespace GitExtensions.Plugins.ReleaseNotesGenerator
{
    [Export(typeof(IGitPlugin))]
    public class ReleaseNotesGeneratorPlugin : GitPluginBase
    {
        public ReleaseNotesGeneratorPlugin() : base(false)
        {
            Id = new Guid("49E7F2D6-AD79-489E-80A4-5CD212AE6DF3");
            Name = "Release Notes Generator";
            Translate(AppSettings.CurrentTranslation);
            Icon = Resources.IconReleaseNotesGenerator;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using ReleaseNotesGeneratorForm form = new(args);
            if (form.ShowDialog(args.OwnerForm) == DialogResult.OK)
            {
                return true;
            }

            return false;
        }
    }
}
