namespace GitUIPluginInterfaces
{
    public interface IGitSubmoduleInfo
    {
        string Branch { get; }
        ObjectId CurrentCommitId { get; }
        bool IsInitialized { get; }
        string LocalPath { get; }
        string Name { get; }
        string RemotePath { get; }
        string Status { get; }
        bool IsUpToDate { get; }
    }
}