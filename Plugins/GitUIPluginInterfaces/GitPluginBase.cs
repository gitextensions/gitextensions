namespace GitUIPluginInterfaces
{
    public abstract class GitPluginBase : IGitPlugin
    {

        public abstract string Description { get; }

        private IGitPluginSettingsContainer _Settings;
        //Store settings to use later
        public IGitPluginSettingsContainer Settings
        {
            get
            {
                return _Settings;
            }

            set
            {
                _Settings = value;
                RegisterSettings();
            }
        }

        protected virtual void RegisterSettings()
        { 
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
