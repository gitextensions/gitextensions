using System;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class DiffListSortServiceTests
    {
        /// <summary>
        /// DO NOT change this during the test. It is used to put your normal setting back the way it was.
        /// </summary>
        private DiffListSortType _storedSetting;

        [SetUp]
        public void Setup()
        {
            _storedSetting = AppSettings.DiffListSorting;
        }

        [TearDown]
        public void Teardown()
        {
            AppSettings.DiffListSorting = _storedSetting;
        }

        [Test]
        [TestCase(DiffListSortType.FilePath)]
        [TestCase(DiffListSortType.FileExtension)]
        [TestCase(DiffListSortType.FileStatus)]
        public void Constructor_loads_persisted_setting(DiffListSortType persistedSetting)
        {
            AppSettings.DiffListSorting = persistedSetting;
            var service = new DiffListSortService();

            Assert.AreEqual(persistedSetting, service.DiffListSorting);
        }

        [Test]
        [TestCase(DiffListSortType.FilePath)]
        [TestCase(DiffListSortType.FileExtension)]
        [TestCase(DiffListSortType.FileStatus)]
        public void Changing_the_sort_modifies_persistence(DiffListSortType sortType)
        {
            var service = new DiffListSortService();

            service.DiffListSorting = sortType;

            Assert.AreEqual(sortType, AppSettings.DiffListSorting);
        }

        [Test]
        public void Change_notifications_occur_when_sort_is_changed()
        {
            var service = new DiffListSortService();
            service.DiffListSorting = DiffListSortType.FilePath;

            var raisedCount = 0;
            service.DiffListSortingChanged += (s, e) => raisedCount++;

            service.DiffListSorting = DiffListSortType.FilePath;
            Assert.AreEqual(0, raisedCount, "Service sort did not change. It should not have raised a notification.");

            service.DiffListSorting = DiffListSortType.FileExtension;
            Assert.AreEqual(1, raisedCount, "Service sort changed. It should not have raised a single notification.");

            service.DiffListSorting = DiffListSortType.FileExtension;
            Assert.AreEqual(1, raisedCount, "Service sort did not change. It should not have raised another notification.");

            service.DiffListSorting = DiffListSortType.FileStatus;
            Assert.AreEqual(2, raisedCount, "Service sort changed. It should not have raised a second notification.");
        }

        [Test]
        public void Can_get_singleton()
        {
            Assert.IsNotNull(DiffListSortService.Instance);
            Assert.AreSame(DiffListSortService.Instance, DiffListSortService.Instance);
        }
    }
}
