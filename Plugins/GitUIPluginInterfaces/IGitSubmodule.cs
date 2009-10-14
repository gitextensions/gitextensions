using System;

namespace GitUIPluginInterfaces
{
    public interface IGitSubmodule
    {
        string Branch { get; set; }
        string CurrentCommitGuid { get; set; }
        bool Initialized { get; set; }
        string LocalPath { get; set; }
        string Name { get; set; }
        string RemotePath { get; set; }
        string Status { get; }
        bool UpToDate { get; set; }
    }
}
