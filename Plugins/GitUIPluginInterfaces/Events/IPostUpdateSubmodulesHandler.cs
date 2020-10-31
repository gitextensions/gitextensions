namespace GitUIPluginInterfaces.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostUpdateSubmodules.
    /// </summary>
    public interface IPostUpdateSubmodulesHandler
    {
        void OnPostUpdateSubmodules(GitUIPostActionEventArgs e);
    }
}
