namespace GitUIPluginInterfaces
{
    public interface IGitSubmoduleInfo
    {
        string Branch { get; }
        string CurrentCommitGuid { get; }
        bool IsInitialized { get; }
        string LocalPath { get; }
        string Name { get; }
        string RemotePath { get; }
        string Status { get; }
        bool IsUpToDate { get; }
    }
}