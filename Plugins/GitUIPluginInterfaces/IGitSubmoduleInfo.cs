namespace GitUIPluginInterfaces
{
    public interface IGitSubmoduleInfo
    {
        string Branch { get; set; }
        string CurrentCommitGuid { get; set; }
        bool Initialized { get; set; }
        string LocalPath { get; set; }
        string Name { get; }
        string RemotePath { get; }
        string Status { get; }
        bool UpToDate { get; set; }
    }
}