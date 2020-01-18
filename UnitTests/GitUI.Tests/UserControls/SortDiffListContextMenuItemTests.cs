using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls
{
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
            Assert.IsTrue(_itemUnderTest.HasDropDownItems);
            Assert.AreEqual(3, _itemUnderTest.DropDownItems.Count);

            AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[0], "File &Path", DiffListSortType.FilePath);
            AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[1], "File &Extension", DiffListSortType.FileExtension);
            AssertMenuItemTextAndLinkedSortType(_itemUnderTest.DropDownItems[2], "File &Status", DiffListSortType.FileStatus);
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
            _itemUnderTest.GetTestAccessor().SimulateOpeningEvent();

            AssertOnlyCheckedItemIs(sortType);
        }

        [Test]
        public void Clicking_an_item_sets_sort_in_service()
        {
            foreach (var item in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>())
            {
                item.PerformClick();
                _testingSortService.Received(1).DiffListSorting = (DiffListSortType)item.Tag;
            }
        }

        private void AssertOnlyCheckedItemIs(DiffListSortType sortType)
        {
            var matchingSubItem = _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Single(i => i.Tag.Equals(sortType));
            Assert.IsTrue(matchingSubItem.Checked);

            foreach (var otherItem in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Except(new[] { matchingSubItem }))
            {
                Assert.IsFalse(otherItem.Checked);
            }
        }

        private static void AssertMenuItemTextAndLinkedSortType(ToolStripItem menuItem, string expectedText, DiffListSortType expectedSortType)
        {
            Assert.AreEqual(expectedText, menuItem.Text);
            Assert.AreEqual(expectedSortType, menuItem.Tag);
        }
    }
}
