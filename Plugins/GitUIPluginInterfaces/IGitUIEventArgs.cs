using System;
namespace GitUIPluginInterfaces
{
    public interface IGitUIEventArgs
    {
        bool Cancel { get; set; }
        IGitUICommands GitUICommands { get; }
        IGitCommands GitCommands { get; }
        string GitWorkingDir { get; }
        bool IsValidGitWorkingDir (string workingDir);
        string GitCommand { get; }
        string GitVersion { get; }
    }
}
