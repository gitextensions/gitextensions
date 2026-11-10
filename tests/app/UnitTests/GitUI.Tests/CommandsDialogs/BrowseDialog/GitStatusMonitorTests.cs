using GitUI.CommandsDialogs.BrowseDialog;

namespace GitUITests.CommandsDialogs.BrowseDialog;

public class GitStatusMonitorTests
{
    // the cookie file of the repo, of a linked worktree, of a submodule and of an old submodule having its own .git directory
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies\1234-1", true)]
    [TestCase(@"C:\repo\.git\worktrees\feature\fsmonitor--daemon\cookies\1234-1", true)]
    [TestCase(@"C:\repo\.git\modules\sub\fsmonitor--daemon\cookies\1234-1", true)]
    [TestCase(@"C:\repo\sub\.git\fsmonitor--daemon\cookies\1234-1", true)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies", true)]

    // other files of a git directory
    [TestCase(@"C:\repo\.git\index", false)]
    [TestCase(@"C:\repo\.git\HEAD", false)]
    [TestCase(@"C:\repo\.git\refs\heads\fsmonitor--daemon", false)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon.ipc", false)]

    // files of the working directory
    [TestCase(@"C:\repo\builtin\fsmonitor--daemon.c", false)]
    [TestCase(@"C:\repo\file.txt", false)]
    public void IsFsMonitorDaemonPath_should_detect_files_of_the_daemon(string path, bool expected)
    {
        GitStatusMonitor.IsFsMonitorDaemonPath(path).Should().Be(expected);
    }
}
