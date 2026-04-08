using GitCommands;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests;

[TestFixture]
public class CommitDataManagerTest
{
    private CommitDataManager _commitDataManager = null!;
    private IGitModule _module = null!;
    private Func<IGitModule> _getModule = null!;

    [SetUp]
    public void Setup()
    {
        _module = Substitute.For<IGitModule>();
        _module.ReEncodeStringFromLossless(Arg.Any<string>()).Returns(x => x[0]);
        _module.ReEncodeCommitMessage(Arg.Any<string>()).Returns(x => x[0]);

        _getModule = () => _module;
        _commitDataManager = new CommitDataManager(_getModule);
    }
}
