using GitCommands;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitDataManagerTest
    {
        private CommitDataManager _commitDataManager;
        private IGitModule _module;
        private Func<IGitModule> _getModule;

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
}
