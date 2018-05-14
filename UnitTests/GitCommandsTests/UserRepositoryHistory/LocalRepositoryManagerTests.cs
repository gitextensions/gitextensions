using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitCommands.UserRepositoryHistory.Legacy;
using NSubstitute;
using NUnit.Framework;
using IRepositoryStorage = GitCommands.UserRepositoryHistory.IRepositoryStorage;
using Repository = GitCommands.UserRepositoryHistory.Repository;

namespace GitCommandsTests.UserRepositoryHistory
{
    [TestFixture]
    public class LocalRepositoryManagerTests
    {
        private const string KeyRecentHistory = "history";
        private const string KeyFavouriteHistory = "history-favourite";
        private IRepositoryStorage _repositoryStorage;
        private IRepositoryHistoryMigrator _repositoryHistoryMigrator;
        private LocalRepositoryManager _manager;
        private int _userSetting;

        [SetUp]
        public void Setup()
        {
            // backup the user setting, will restore it at the end of the test run
            _userSetting = AppSettings.RecentRepositoriesHistorySize;
            AppSettings.RecentRepositoriesHistorySize = 30;

            _repositoryStorage = Substitute.For<IRepositoryStorage>();
            _repositoryHistoryMigrator = Substitute.For<IRepositoryHistoryMigrator>();
            _manager = new LocalRepositoryManager(_repositoryStorage, _repositoryHistoryMigrator);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.RecentRepositoriesHistorySize = _userSetting;
        }

        [Test]
        public async Task AddAsMostRecentAsync_should_add_new_path_as_top_entry()
        {
            const string repoToAdd = "path to add\\";
            var history = new List<Repository>
            {
                new Repository("path1\\"),
                new Repository("path3\\"),
                new Repository("path4\\"),
                new Repository("path5\\"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.AddAsMostRecentAsync(repoToAdd);

            newHistory.Count.Should().Be(5);
            newHistory[0].Path.Should().Be(repoToAdd);
        }

        [Test]
        public async Task AddAsMostRecentAsync_should_move_existing_path_as_top_entry()
        {
            const string repoToAdd = "path to add\\";
            var history = new List<Repository>
            {
                new Repository("path1\\"),
                new Repository("path3\\"),
                new Repository("path4\\"),
                new Repository(repoToAdd),
                new Repository("path5\\"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.AddAsMostRecentAsync(repoToAdd);

            newHistory.Count.Should().Be(5);
            newHistory[0].Path.Should().Be(repoToAdd);
        }

        [Test]
        public async Task AddAsMostRecentAsync_should_move_only_first_existing_path_as_top_entry()
        {
            const string repoToAdd = "path to add\\";
            var history = new List<Repository>
            {
                new Repository("path1\\"),
                new Repository("path3\\"),
                new Repository(repoToAdd),
                new Repository("path4\\"),
                new Repository(repoToAdd),
                new Repository("path5\\"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.AddAsMostRecentAsync(repoToAdd);

            newHistory.Count.Should().Be(6);
            newHistory[0].Path.Should().Be(repoToAdd);
            newHistory[4].Path.Should().Be(repoToAdd);
        }

        [Test]
        public async Task AddAsMostRecentAsync_should_not_move_if_path_already_as_top_entry()
        {
            const string repoToAdd = "path to add\\";
            var history = new List<Repository>
            {
                new Repository(repoToAdd),
                new Repository("path1\\"),
                new Repository("path3\\"),
                new Repository("path4\\"),
                new Repository("path5\\"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.AddAsMostRecentAsync(repoToAdd);

            newHistory.Count.Should().Be(5);
            newHistory[0].Path.Should().Be(repoToAdd);
            _repositoryStorage.DidNotReceive().Save(KeyRecentHistory, Arg.Any<IList<Repository>>());
        }

        [Test]
        public void AssignCategoryAsync_should_throw_if_key_null()
        {
            Func<Task> f = async () => { await _manager.AssignCategoryAsync(null, null); };
            f.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task LoadFavouriteHistoryAsync_should_return_empty_list_if_nothing_loaded()
        {
            _repositoryStorage.Load(KeyFavouriteHistory).Returns(x => null);

            var history = await _manager.LoadFavouriteHistoryAsync();

            history.Should().BeEmpty();
        }

        [Test]
        public async Task LoadFavouriteHistoryAsync_should_migrate_old_categorised_repositories()
        {
            _repositoryStorage.Load(KeyFavouriteHistory).Returns(x => new List<Repository>());

            await _manager.LoadFavouriteHistoryAsync();

#pragma warning disable 4014
            _repositoryHistoryMigrator.Received(1).MigrateAsync(Arg.Any<IList<Repository>>());
#pragma warning restore 4014
        }

        [Test]
        public async Task LoadRecentHistoryAsync_should_return_empty_list_if_nothing_loaded()
        {
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => null);

            var history = await _manager.LoadRecentHistoryAsync();

            history.Should().BeEmpty();
        }

        [Test]
        public async Task LoadRecentHistoryAsync_should_trim_history_per_settings()
        {
            const int size = 3;
            AppSettings.RecentRepositoriesHistorySize = size;
            var history = new List<Repository>
            {
                new Repository("path1") { Category = "my" },
                new Repository("path2"),
                new Repository("path3"),
                new Repository("path4") { Category = "another" },
                new Repository("path5"),
                new Repository("path6"),
                new Repository("path7"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var repositories = await _manager.LoadRecentHistoryAsync();

            repositories.Count.Should().Be(size);
            repositories.Select(r => r.Path).Should().ContainInOrder("path1", "path2", "path3");
        }

        [Test]
        public async Task RemoveRecentAsync_should_remove_if_exists()
        {
            const string repoToDelete = "path to delete";
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository(repoToDelete),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.RemoveRecentAsync(repoToDelete);

            newHistory.Count.Should().Be(4);
            newHistory.Should().NotContain(repoToDelete);

            _repositoryStorage.Received(1).Load(KeyRecentHistory);
            _repositoryStorage.Received(1).Save(KeyRecentHistory, Arg.Is<IEnumerable<Repository>>(h => h.All(r => r.Path != repoToDelete)));
        }

        [Test]
        public async Task RemoveFavouriteAsync_should_not_crash_if_not_exists()
        {
            const string repoToDelete = "path to delete";
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository("path2"),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };
            _repositoryStorage.Load(KeyFavouriteHistory).Returns(x => history);
            _repositoryHistoryMigrator.MigrateAsync(Arg.Any<List<Repository>>()).Returns(x => (history, false));

            var newHistory = await _manager.RemoveFavouriteAsync(repoToDelete);

            newHistory.Count.Should().Be(5);
            newHistory.Should().NotContain(repoToDelete);

            _repositoryStorage.Received(1).Load(KeyFavouriteHistory);
            _repositoryStorage.DidNotReceive().Save(KeyFavouriteHistory, Arg.Any<IEnumerable<Repository>>());
#pragma warning disable 4014
            _repositoryHistoryMigrator.Received(1).MigrateAsync(history);
#pragma warning restore 4014
        }

        [Test]
        public async Task RemoveFavouriteAsync_should_remove_if_exists()
        {
            const string repoToDelete = "path to delete";
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository(repoToDelete),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };
            _repositoryStorage.Load(KeyFavouriteHistory).Returns(x => history);
            _repositoryHistoryMigrator.MigrateAsync(Arg.Any<List<Repository>>()).Returns(x => (history, false));

            var newHistory = await _manager.RemoveFavouriteAsync(repoToDelete);

            newHistory.Count.Should().Be(4);
            newHistory.Should().NotContain(repoToDelete);

            _repositoryStorage.Received(1).Load(KeyFavouriteHistory);
            _repositoryStorage.Received(1).Save(KeyFavouriteHistory, Arg.Is<IEnumerable<Repository>>(h => h.All(r => r.Path != repoToDelete)));
#pragma warning disable 4014
            _repositoryHistoryMigrator.Received(1).MigrateAsync(history);
#pragma warning restore 4014
        }

        [Test]
        public async Task RemoveRecentAsync_should_not_crash_if_not_exists()
        {
            const string repoToDelete = "path to delete";
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository("path2"),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };
            _repositoryStorage.Load(KeyRecentHistory).Returns(x => history);

            var newHistory = await _manager.RemoveRecentAsync(repoToDelete);

            newHistory.Count.Should().Be(5);
            newHistory.Should().NotContain(repoToDelete);

            _repositoryStorage.Received(1).Load(KeyRecentHistory);
            _repositoryStorage.DidNotReceive().Save(KeyRecentHistory, Arg.Any<IEnumerable<Repository>>());
        }

        [Test]
        public void SaveFavouriteHistoryAsync_should_throw_if_repositories_null()
        {
            Func<Task> action = async () => await _manager.SaveFavouriteHistoryAsync(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task SaveFavouriteHistoryAsync_should_save()
        {
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository("path2"),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };

            await _manager.SaveFavouriteHistoryAsync(history);

            _repositoryStorage.Received(1).Save(KeyFavouriteHistory, history);
        }

        [Test]
        public void SaveRecentHistoryAsync_should_throw_if_repositories_null()
        {
            Func<Task> action = async () => await _manager.SaveRecentHistoryAsync(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task SaveRecentHistoryAsync_should_trim_history_size()
        {
            const int size = 3;
            AppSettings.RecentRepositoriesHistorySize = size;
            var history = new List<Repository>
            {
                new Repository("path1"),
                new Repository("path2"),
                new Repository("path3"),
                new Repository("path4"),
                new Repository("path5"),
            };

            await _manager.SaveRecentHistoryAsync(history);

            _repositoryStorage.Received(1).Save(KeyRecentHistory, Arg.Is<IEnumerable<Repository>>(h => h.Count() == size));
        }
    }
}