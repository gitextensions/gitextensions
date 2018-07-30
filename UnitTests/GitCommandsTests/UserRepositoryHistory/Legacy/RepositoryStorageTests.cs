using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory.Legacy;
using NSubstitute;
using NUnit.Framework;
using Current = GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory.Legacy
{
    [TestFixture]
    public class RepositoryStorageTests
    {
        private Current.IRepositorySerialiser<RepositoryCategory> _repositoryCategorySerialiser;
        private RepositoryStorage _repositoryStorage;

        [SetUp]
        public void Setup()
        {
            _repositoryCategorySerialiser = Substitute.For<Current.IRepositorySerialiser<RepositoryCategory>>();
            _repositoryStorage = new RepositoryStorage(_repositoryCategorySerialiser);
        }

        [Test]
        public void LoadLegacy_should_return_empty_collection_if_settings_value_null()
        {
            var repositories = _repositoryStorage.Load();

            repositories.Should().BeEmpty();
        }

        [Test]
        public void LoadLegacy_should_return_empty_collection_if_failed_to_deserialise()
        {
            AppSettings.SetString("repositories", "repo");
            _repositoryCategorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => null);

            var repositories = _repositoryStorage.Load();

            repositories.Should().BeEmpty();
        }

        [Test]
        public void LoadLegacy_should_return_collection()
        {
            AppSettings.SetString("repositories", "repos");
            var history = new List<RepositoryCategory>
            {
                new RepositoryCategory
                {
                    Repositories = new List<Repository>(
                        new[]
                        {
                            new Repository { Path = "C:\\Development\\RibbonWinForms\\", Description = "Check it out!", Anchor = "None" },
                            new Repository { Path = "", Anchor = "None" },
                        }),
                    CategoryType = "Repositories",
                    Description = "3rd Party"
                },
                new RepositoryCategory
                {
                    Repositories = new List<Repository>(
                        new[]
                        {
                            new Repository { Title = "Git Extensions", Path = "C:\\Development\\gitextensions\\", Description = "Mega project!", Anchor = "MostRecent" }
                        }),
                    CategoryType = "Repositories",
                    Description = "Test"
                },
            };
            _repositoryCategorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => history);

            var repositories = _repositoryStorage.Load();

            repositories.Should().BeSameAs(history);
        }
    }
}