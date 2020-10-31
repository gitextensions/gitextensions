namespace GitUIPluginInterfaces.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostRegisterPlugin.
    /// </summary>
    public interface IPostRegisterPluginHandler
    {
        void OnPostRegisterPlugin(GitUIEventArgs e);
    }
}
