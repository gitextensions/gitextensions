using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitExtensions.Plugins.BuildServerIntegration.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.BuildServerIntegration
{
    [Export(typeof(IGitPlugin))]
    internal sealed class Plugin : GitPluginBase, IGitPluginForRepository
    {
        private IGitUICommands? _gitUiCommands;

        public Plugin()
            : base(true)
        {
            Id = new Guid("6BF184BF-D34E-4B0B-BA13-F050BE8C359D");
            Name = "Build Server Integration";
            Translate();
            Icon = Resources.Integration;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            _gitUiCommands = gitUiCommands;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            args.GitUICommands.StartSettingsDialog(this);

            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            if (_gitUiCommands is null)
            {
                yield break;
            }

            yield return new Settings("Setting", _gitUiCommands.GitModule);
        }
    }
}
