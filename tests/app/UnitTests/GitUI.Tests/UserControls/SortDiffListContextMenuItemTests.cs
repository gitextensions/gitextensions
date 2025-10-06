using GitCommands;
using GitUI.UserControls;
using NSubstitute;

namespace GitUITests.UserControls;

[SetCulture("en-US")]
[SetUICulture("en-US")]
[TestFixture]
public class SortDiffListContextMenuItemTests
{
    private SortDiffListContextMenuItem _itemUnderTest;
    private IDiffListSortService _testingSortService;

    [SetUp]
    public void Setup()
    {
        _testingSortService = Substitute.For<IDiffListSortService>();
        _testingSortService.DiffListSorting.Returns(DiffListSortType.FilePath);
        _itemUnderTest = new SortDiffListContextMenuItem(_testingSortService);
    }

    [Test]
    public void Should_show_all_sort_options()
    {
        ClassicAssert.IsTrue(_itemUnderTest.HasDropDownItems);
        ClassicAssert.AreEqual(6, _itemUnderTest.DropDownItems.Count);

        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[0], "File &path - tree", DiffListSortType.FilePath);
        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[1], "&File path - flat", DiffListSortType.FilePathFlat);
        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[2], "File &extension - tree", DiffListSortType.FileExtension);
        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[3], "File e&xtension - flat", DiffListSortType.FileExtensionFlat);
        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[4], "File &status - tree", DiffListSortType.FileStatus);
        AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[5], "File s&tatus - flat", DiffListSortType.FileStatusFlat);
    }

    [Test]
    [TestCase(DiffListSortType.FilePath)]
    [TestCase(DiffListSortType.FileExtension)]
    [TestCase(DiffListSortType.FileStatus)]
    public void Only_the_current_sort_option_is_selected(DiffListSortType sortType)
    {
        // assert the default setup is selected and then change it.
        AssertOnlyCheckedItemIs(DiffListSortType.FilePath);

        _testingSortService.DiffListSorting.Returns(sortType);

        // invoke the requery method to reselect the proper sub item
        _itemUnderTest.GetTestAccessor().RaiseDropDownOpening();

        AssertOnlyCheckedItemIs(sortType);
    }

    [Test]
    public void Clicking_an_item_sets_sort_in_service()
    {
        foreach (ToolStripMenuItem item in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>())
        {
            item.PerformClick();
            _testingSortService.Received(1).DiffListSorting = (DiffListSortType)item.Tag;
        }
    }

    private void AssertOnlyCheckedItemIs(DiffListSortType sortType)
    {
        ToolStripMenuItem matchingSubItem = _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Single(i => i.Tag.Equals(sortType));
        ClassicAssert.IsTrue(matchingSubItem.Checked);

        foreach (ToolStripMenuItem otherItem in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Except(new[] { matchingSubItem }))
        {
            ClassicAssert.IsFalse(otherItem.Checked);
        }
    }

    private static void AssertMenuItemTextAndLinkedSortType(ToolStripItem menuItem, string expectedText, DiffListSortType expectedSortType)
    {
        ClassicAssert.AreEqual(expectedText, menuItem.Text);
        ClassicAssert.AreEqual(expectedSortType, menuItem.Tag);
    }
}
