namespace GitUIPluginInterfaces.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostSettings.
    /// </summary>
    public interface IPostSettingsHandler
    {
        void OnPostSettings(GitUIPostActionEventArgs e);
    }
}
