using System;
namespace GitUIPluginInterfaces
{
    public interface IGitUIEventArgs
    {
        bool Cancel { get; set; }
        IGitUICommands GitUICommands { get; }
        string GitWorkingDir { get; }
        string GitDir { get; }
        string GitVersion { get; }
    }
}
