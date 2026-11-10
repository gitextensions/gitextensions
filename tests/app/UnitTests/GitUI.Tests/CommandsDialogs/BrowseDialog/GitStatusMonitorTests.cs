using GitUI.CommandsDialogs.BrowseDialog;

namespace GitUITests.CommandsDialogs.BrowseDialog;

public class GitStatusMonitorTests
{
    // the cookie file and the cookies directory of the repo, of a linked worktree, of a submodule and of their combinations
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git\", true)]
    [TestCase(@"C:\repo\.git\worktrees\feature\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\worktrees\feature\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git\worktrees\feature", true)]
    [TestCase(@"C:\repo\.git\modules\sub\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\modules\libs\sub\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\modules\sub\modules\nested\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]
    [TestCase(@"C:\repo\.git\worktrees\feature\modules\sub\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", true)]

    // an ancestor, a linked worktree, a submodule and a branch which are named like the directory of the daemon
    [TestCase(@"C:\fsmonitor--daemon\cookies\.git\index", @"C:\fsmonitor--daemon\cookies\.git", false)]
    [TestCase(@"C:\fsmonitor--daemon\repo\.git\index", @"C:\fsmonitor--daemon\repo\.git", false)]
    [TestCase(@"C:\repo\.git\worktrees\fsmonitor--daemon\index", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\worktrees\fsmonitor--daemon\cookies\index", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\modules\fsmonitor--daemon\index", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\modules\fsmonitor--daemon\cookies\index", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\refs\heads\fsmonitor--daemon", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\refs\heads\fsmonitor--daemon\cookies", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\refs\heads\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\logs\refs\heads\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", false)]

    // other files of a git directory
    [TestCase(@"C:\repo\.git\index", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\HEAD", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon.ipc", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git\fsmonitor--daemon\cookies\1234-1\file", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\.git", @"C:\repo\.git", false)]

    // files outside of the git directory
    [TestCase(@"C:\repo\.github\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", false)]
    [TestCase(@"C:\other\.git\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", false)]
    [TestCase(@"C:\repo\fsmonitor--daemon\cookies\1234-1", @"C:\repo\.git", false)]
    public void IsFsMonitorDaemonPath_should_detect_cookies_of_the_daemon(string path, string gitDir, bool expected)
    {
        GitStatusMonitor.IsFsMonitorDaemonPath(path, gitDir).Should().Be(expected);
    }

    // the cookie file of an old submodule having its own .git directory
    [TestCase(@"C:\repo\sub\.git\fsmonitor--daemon\cookies\1234-1", @"C:\repo\", true)]
    [TestCase(@"C:\repo\sub\.git\fsmonitor--daemon\cookies\1234-1", @"C:\repo", true)]
    [TestCase(@"C:\repo\libs\sub\.git\fsmonitor--daemon\cookies", @"C:\repo\", true)]
    [TestCase(@"C:\repo\sub\.git\modules\nested\fsmonitor--daemon\cookies\1234-1", @"C:\repo\", true)]

    // other files of an old submodule
    [TestCase(@"C:\repo\sub\.git\index", @"C:\repo\", false)]
    [TestCase(@"C:\repo\sub\.git\refs\heads\fsmonitor--daemon\cookies\1234-1", @"C:\repo\", false)]
    [TestCase(@"C:\repo\sub\.git\modules\fsmonitor--daemon\index", @"C:\repo\", false)]

    // files of the working directory, also if an ancestor is named like the directory of the daemon
    [TestCase(@"C:\repo\builtin\fsmonitor--daemon.c", @"C:\repo\", false)]
    [TestCase(@"C:\repo\fsmonitor--daemon\cookies\1234-1", @"C:\repo\", false)]
    [TestCase(@"C:\repo\sub\fsmonitor--daemon\cookies\1234-1", @"C:\repo\", false)]
    [TestCase(@"C:\repo\file.txt", @"C:\repo\", false)]
    [TestCase(@"C:\x\.git\fsmonitor--daemon\cookies\file.txt", @"C:\x\.git\fsmonitor--daemon\cookies\", false)]
    [TestCase(@"C:\x\.git\fsmonitor--daemon\repo\cookies\file.txt", @"C:\x\.git\fsmonitor--daemon\repo\", false)]
    [TestCase(@"C:\repo", @"C:\repo\", false)]
    public void IsNestedFsMonitorDaemonPath_should_detect_cookies_of_the_daemon_of_an_old_submodule(string path, string workTreeDir, bool expected)
    {
        GitStatusMonitor.IsNestedFsMonitorDaemonPath(path, workTreeDir).Should().Be(expected);
    }
}
