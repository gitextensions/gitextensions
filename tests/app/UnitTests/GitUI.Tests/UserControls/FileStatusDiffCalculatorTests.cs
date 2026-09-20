using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls;

public class FileStatusDiffCalculatorTests
{
    private IGitModule _module;
    private FileStatusDiffCalculator _calculator;

    [SetUp]
    public void Setup()
    {
        _module = Substitute.For<IGitModule>();
        _calculator = new FileStatusDiffCalculator(() => _module);
        _calculator.SetDiff([new GitRevision(ObjectId.Random())], ObjectId.Random(), allowMultiDiff: false);
    }

    [Test]
    public void Calculate_must_not_run_git_if_the_repository_was_closed()
    {
        // The module is resolved when the calculation runs, so it may belong to a repository which
        // was closed or switched while the calculation was queued
        _module.IsValidGitWorkingDir().Returns(false);

        ((Action)(() => _calculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, CancellationToken.None)))
            .Should().Throw<OperationCanceledException>();

        _module.DidNotReceiveWithAnyArgs().GetDiffFilesWithSubmodulesStatus(default, default, default, default, default);
    }

    [Test]
    public void Calculate_must_run_git_for_a_valid_repository()
    {
        _module.IsValidGitWorkingDir().Returns(true);

        ((Action)(() => _calculator.Calculate(prevList: [], refreshDiff: true, refreshGrep: false, CancellationToken.None)))
            .Should().NotThrow<OperationCanceledException>();
    }
}
