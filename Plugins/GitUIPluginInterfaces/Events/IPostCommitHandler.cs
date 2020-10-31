namespace GitUIPluginInterfaces.Events
{
    /// <summary>
    /// Interface to implement if you wish to receive OnPostCommit.
    /// </summary>
    public interface IPostCommitHandler
    {
        void OnPostCommit(GitUIPostActionEventArgs e);
    }
}
