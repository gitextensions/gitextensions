using GitCommands;

namespace GitCommandsTests;
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
        DiffListSortService service = new();

        service.DiffListSorting.Should().Be(persistedSetting);
    }

    [Test]
    [TestCase(DiffListSortType.FilePath)]
    [TestCase(DiffListSortType.FileExtension)]
    [TestCase(DiffListSortType.FileStatus)]
    public void Changing_the_sort_modifies_persistence(DiffListSortType sortType)
    {
        DiffListSortService service = new()
        {
            DiffListSorting = sortType
        };

        AppSettings.DiffListSorting.Should().Be(sortType);
    }

    [Test]
    public void Change_notifications_occur_when_sort_is_changed()
    {
        DiffListSortService service = new()
        {
            DiffListSorting = DiffListSortType.FilePath
        };

        int raisedCount = 0;
        service.DiffListSortingChanged += (s, e) => raisedCount++;

        service.DiffListSorting = DiffListSortType.FilePath;
        raisedCount.Should().Be(0, "Service sort did not change. It should not have raised a notification.");

        service.DiffListSorting = DiffListSortType.FileExtension;
        raisedCount.Should().Be(1, "Service sort changed. It should not have raised a single notification.");

        service.DiffListSorting = DiffListSortType.FileExtension;
        raisedCount.Should().Be(1, "Service sort did not change. It should not have raised another notification.");

        service.DiffListSorting = DiffListSortType.FileStatus;
        raisedCount.Should().Be(2, "Service sort changed. It should not have raised a second notification.");
    }

    [Test]
    public void Can_get_singleton()
    {
        DiffListSortService.Instance.Should().NotBeNull();
        DiffListSortService.Instance.Should().BeSameAs(DiffListSortService.Instance);
    }
}
