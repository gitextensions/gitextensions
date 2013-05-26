namespace GitUIPluginInterfaces
{
    public interface IGitPlugin
    {
        string Description { get; }

        IGitPluginSettingsContainer Settings { get; set; }

        void Register(IGitUICommands gitUiCommands);

        void Unregister(IGitUICommands gitUiCommands);

        bool Execute(GitUIBaseEventArgs gitUiCommands);
    }
}