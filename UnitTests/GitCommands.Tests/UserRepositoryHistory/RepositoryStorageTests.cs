using System;
using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using NSubstitute;
using NUnit.Framework;

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
            var repositories = _repositoryStorage.Load("a");

            repositories.Should().BeEmpty();
        }

        [Test]
        public void Load_should_return_empty_collection_if_failed_to_deserialise()
        {
            _repositorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => null);

            var repositories = _repositoryStorage.Load("a");

            repositories.Should().BeEmpty();
        }

        [Test]
        public void Load_should_return_collection()
        {
            AppSettings.SetString("a", "repos");
            var history = new List<Repository>
            {
                new Repository(@"C:\Development\gitextensions\"),
                new Repository(@"C:\Development\gitextensions\Externals\NBug\")
                {
                    Anchor = Repository.RepositoryAnchor.MostRecent,
                },
                new Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
                {
                    Anchor = Repository.RepositoryAnchor.LessRecent,
                }
            };
            _repositorySerialiser.Deserialize(Arg.Any<string>()).Returns(x => history);

            var repositories = _repositoryStorage.Load("a");

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