using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitExtensions.Plugins.BuildServer.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.BuildServer
{
    [Export(typeof(IGitPlugin))]
    internal sealed class Plugin : GitPluginBase, IGitPluginForRepository
    {
        private IGitUICommands? _gitUiCommands;

        public Plugin()
            : base(true)
        {
            SetNameAndDescription("BuildServer");
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

            yield return new Setting("Setting", _gitUiCommands.GitModule);
        }
    }
}
