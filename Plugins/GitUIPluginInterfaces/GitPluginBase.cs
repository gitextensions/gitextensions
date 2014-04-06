using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public abstract class GitPluginBase : IGitPlugin
    {

        public abstract string Description { get; }

        //Store settings to use later
        public ISettingsSource Settings { get; set; }

        public virtual IEnumerable<ISetting> GetSettings()
        {
            return new List<ISetting>();
        }

        public virtual void Register(IGitUICommands gitUiCommands)
        {
        }

        public virtual void Unregister(IGitUICommands gitUiCommands)
        {
        }

        public abstract bool Execute(GitUIBaseEventArgs gitUiCommands);

    }
}
