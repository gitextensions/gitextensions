using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using NSubstitute;

namespace GitCommandsTests.UserRepositoryHistory
{
    [TestFixture]
    public class RepositoryStorageTests
    {
        private IRepositorySerialiser<Repository> _repositorySerialiser;
        private RepositoryStorage _repositoryStorage;

        [SetUp]
        public void Setup()
        {
            _repositorySerialiser = Substitute.For<IRepositorySerialiser<Repository>>();
            _repositoryStorage = new RepositoryStorage(_repositorySerialiser);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Load_should_throw_if_key_null(string key)
        {
            ((Action)(() => _repositoryStorage.Load(key))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Load_should_return_empty_collection_if_settings_value_null()
        {
            IReadOnlyList<Repository> repositories = _repositoryStorage.Load("a");

            repositories.Should().BeEmpty();
        }

        [Test]
        public void Load_should_return_empty_collection_if_failed_to_deserialise()
        {
            _repositorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => null);

            IReadOnlyList<Repository> repositories = _repositoryStorage.Load("a");

            repositories.Should().BeEmpty();
        }

        [Test]
        public void Load_should_return_collection()
        {
            AppSettings.SetString("a", "repos");
            List<Repository> history =
            [
                new Repository(@"C:\Development\gitextensions\"),
                new Repository(@"C:\Development\gitextensions\Externals\NBug\")
                {
                    Anchor = Repository.RepositoryAnchor.AnchoredInTop,
                },
                new Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
                {
                    Anchor = Repository.RepositoryAnchor.AnchoredInRecent,
                }

            ];
            _repositorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => history);

            IReadOnlyList<Repository> repositories = _repositoryStorage.Load("a");

            repositories.Should().BeSameAs(history);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Save_should_throw_if_key_null(string key)
        {
            ((Action)(() => _repositoryStorage.Save(key, null))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Save_should_throw_if_repositories_null()
        {
            ((Action)(() => _repositoryStorage.Save("a", null))).Should().Throw<ArgumentNullException>();
        }
    }
}
