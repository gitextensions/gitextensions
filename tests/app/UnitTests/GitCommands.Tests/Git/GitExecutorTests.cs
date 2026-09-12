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
    [TestCase(@"\\wsl$\Ubuntu\home\user\repo\", "Ubuntu")]
    [TestCase(@"\\wsl.localhost\Ubuntu-20.04\home\user\repo\", "Ubuntu-20.04")]
    public void GitExecutable_for_wsl_working_dir_uses_exec_to_bypass_the_distro_shell(string workingDir, string expectedDistro)
    {
        GitExecutor executor = new(new GitDirectoryResolver(), workingDir);

        executor.WslDistro.Should().Be(expectedDistro);
        executor.GitExecutable.PrefixArguments.Should().Contain("--exec")
            .And.MatchRegex(@"--cd\s+\S+\s+--exec\s+git\s*$");
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
