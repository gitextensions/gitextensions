namespace GitUIPluginInterfaces
{
    public interface IGitUIEventArgs
    {
        bool Cancel { get; set; }
        IGitUICommands GitUICommands { get; }
        IGitCommands GitCommands { get; }
        string GitWorkingDir { get; }
        string GitCommand { get; }
        string GitVersion { get; }
        bool IsValidGitWorkingDir(string workingDir);
    }
}