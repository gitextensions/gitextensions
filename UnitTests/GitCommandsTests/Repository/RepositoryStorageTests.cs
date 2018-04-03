using System;
using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitCommands.Repository;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Repository
{
    [TestFixture]
    public class RepositoryStorageTests
    {
        private IRepositorySerialiser _repositorySerialiser;
        private RepositoryStorage _repositoryStorage;

        [SetUp]
        public void Setup()
        {
            _repositorySerialiser = Substitute.For<IRepositorySerialiser>();
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
            var history = new List<GitCommands.Repository.Repository>
            {
                new GitCommands.Repository.Repository(@"C:\Development\gitextensions\"),
                new GitCommands.Repository.Repository(@"C:\Development\gitextensions\Externals\NBug\")
                {
                    Anchor = GitCommands.Repository.Repository.RepositoryAnchor.MostRecent,
                },
                new GitCommands.Repository.Repository(@"C:\Development\gitextensions\GitExtensionsDoc\")
                {
                    Anchor = GitCommands.Repository.Repository.RepositoryAnchor.LessRecent,
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