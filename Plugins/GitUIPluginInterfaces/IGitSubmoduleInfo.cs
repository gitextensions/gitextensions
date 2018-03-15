namespace GitUIPluginInterfaces
{
    public interface IGitSubmoduleInfo
    {
        string Branch { get; }
        string CurrentCommitGuid { get; }
        bool Initialized { get; }
        string LocalPath { get; }
        string Name { get; }
        string RemotePath { get; }
        string Status { get; }
        bool UpToDate { get; }
    }
}