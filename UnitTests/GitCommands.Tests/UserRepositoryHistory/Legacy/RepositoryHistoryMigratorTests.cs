using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.UserRepositoryHistory.Legacy;
using NSubstitute;
using NUnit.Framework;
using Current = GitCommands.UserRepositoryHistory;

namespace GitCommandsTests.UserRepositoryHistory.Legacy
{
    [TestFixture]
    public class RepositoryHistoryMigratorTests
    {
        private IRepositoryStorage _repositoryStorage;
        private RepositoryHistoryMigrator _historyMigrator;

        [SetUp]
        public void Setup()
        {
            _repositoryStorage = Substitute.For<IRepositoryStorage>();
            _historyMigrator = new RepositoryHistoryMigrator(_repositoryStorage);
        }

        [Test]
        public void MigrateAsync_should_throw_if_currentHistory_null()
        {
            Func<Task> f = async () => { await _historyMigrator.MigrateAsync(null); };
            f.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task MigrateSettings_should_migrate_old_categorised_settings()
        {
            var xml = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.CategorisedRepositories02.xml");
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new FileFormatException("Unexpected data");
            }

            _repositoryStorage.Load().Returns(x => new RepositoryCategoryXmlSerialiser().Deserialize(xml));

            var (currentHistory, migrated) = await _historyMigrator.MigrateAsync(new List<Current.Repository>());

            currentHistory.Count.Should().Be(8);
            currentHistory.Count(r => r.Category == "Git Extensions").Should().Be(1);
            currentHistory.Count(r => r.Category == "3rd Party").Should().Be(2);
            currentHistory.Count(r => r.Category == "Tests").Should().Be(5);

            migrated.Should().BeTrue();
        }
    }
}