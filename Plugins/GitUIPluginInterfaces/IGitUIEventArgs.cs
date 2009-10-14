using System;
namespace GitUIPluginInterfaces
{
    public interface IGitUIEventArgs
    {
        bool Cancel { get; set; }
        IGitUICommands GitUICommands { get; }
        IGitCommands GitCommands { get; }
        string GitWorkingDir { get; }
        string GitDir { get; }
        string GitVersion { get; }
    }
}
