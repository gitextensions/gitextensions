using GitCommands.Git;

namespace GitCommandsTests.Git;

[TestFixture]
public class GitExecutorTests
{
    // Regression test for: WSL-routed git commands returned no revisions/diff output at all.
    //
    // wsl.exe, unless told to `--exec` (bypass the distro's default login shell), reconstructs
    // the received argv into a single command line and runs it through that shell (e.g. zsh).
    // The revision grid's own filter includes glob-like arguments such as
    // "--exclude=refs/sessions/**", which are meant to be interpreted by git itself, not by a
    // shell. Routed through a shell, they get glob-expanded against real files before git ever
    // sees them; when nothing matches, zsh's default "nomatch" option aborts the whole command
    // with a non-zero exit and zero output, which GitExtensions then rendered as "no commits".
    // Passing `--exec` makes wsl.exe invoke git directly, without a shell in between, matching
    // how the equivalent native Windows git invocation already behaves.
    //
    // The `bash -c` wrapper sources the user's `~/.bashrc` before running git, so that
    // shell-level configuration (e.g. an auto-launched ssh-agent) is available to the git
    // process. The git command and its arguments are passed as separate argv entries ($0 and
    // $@) rather than as a shell string, so they are not re-parsed or glob-expanded.
    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", "Ubuntu")]
    [TestCase(@"\\wsl.localhost\Ubuntu-20.04\home\user\repo\", "Ubuntu-20.04")]
    public void GitExecutable_for_wsl_working_dir_uses_exec_to_bypass_the_distro_shell(string workingDir, string expectedDistro)
    {
        GitExecutor executor = new(new GitDirectoryResolver(), workingDir);

        executor.WslDistro.Should().Be(expectedDistro);
        executor.GitExecutable.PrefixArguments.Should().Contain("--exec")
            .And.MatchRegex(@"--cd\s+\S+\s+--exec\s+bash\s+-c\s+.+\s+git\s*$");
    }

    [Test]
    public void GitExecutable_for_wsl_working_dir_sources_bashrc_before_running_git()
    {
        GitExecutor executor = new(new GitDirectoryResolver(), @"\\wsl$\Ubuntu\home\user\repo\");

        // The prefix must source ~/.bashrc so that shell-level config (e.g. an auto-launched
        // ssh-agent) is available to the git process, and must pass the git command and its
        // arguments as separate argv entries ($0 and $@) to avoid re-parsing/glob expansion.
        // The quotes around $0 and $@ are escaped (\"), so that Windows' command-line parser
        // passes them to bash as literal quotes.
        executor.GitExecutable.PrefixArguments.Should().Contain("source ~/.bashrc")
            .And.Contain("exec \\\"$0\\\" \\\"$@\\\"");
    }

    [Test]
    public void GitExecutable_for_non_wsl_working_dir_has_no_wsl_prefix()
    {
        GitExecutor executor = new(new GitDirectoryResolver(), @"c:\repo\");

        executor.WslDistro.Should().BeEmpty();
        executor.GitExecutable.PrefixArguments.Should().BeEmpty();
        executor.GitExecutable.Should().BeSameAs(executor.GitWindowsExecutable);
    }
}
