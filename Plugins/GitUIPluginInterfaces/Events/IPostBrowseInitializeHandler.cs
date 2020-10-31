namespace GitUIPluginInterfaces.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostBrowseInitialize.
    /// </summary>
    public interface IPostBrowseInitializeHandler
    {
        void OnPostBrowseInitialize(GitUIEventArgs e);
    }
}
