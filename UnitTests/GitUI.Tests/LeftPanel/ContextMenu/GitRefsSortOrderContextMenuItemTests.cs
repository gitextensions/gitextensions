using GitCommands;
using GitCommands.Utils;
using GitUI.LeftPanel.ContextMenu;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class GitRefsSortOrderContextMenuItemTests
    {
        private Action _onSortOrderChanged;
        private GitRefsSortOrderContextMenuItem _itemUnderTest;

        [SetUp]
        public void Setup()
        {
            _onSortOrderChanged = Substitute.For<Action>();
            _itemUnderTest = new GitRefsSortOrderContextMenuItem(_onSortOrderChanged);
        }

        [Test]
        public void Should_show_all_sort_options()
        {
            Assert.IsTrue(_itemUnderTest.HasDropDownItems);
            Assert.AreEqual(EnumHelper.GetValues<GitRefsSortOrder>().Length, _itemUnderTest.DropDownItems.Count);
        }

        private static IEnumerable<TestCaseData> SortOrderOptions
        {
            get
            {
                foreach (GitRefsSortOrder order in EnumHelper.GetValues<GitRefsSortOrder>())
                {
                    yield return new TestCaseData(order);
                }
            }
        }

        [TestCaseSource(nameof(SortOrderOptions))]
        public void Only_the_current_sort_option_is_selected(GitRefsSortOrder order)
        {
            GitRefsSortOrder original = AppSettings.RefsSortOrder;
            try
            {
                AppSettings.RefsSortOrder = order;

                // invoke the requery method to reselect the proper sub item
                _itemUnderTest.GetTestAccessor().RaiseDropDownOpening();

                AssertOnlyCheckedItemIs(order);
            }
            finally
            {
                AppSettings.RefsSortOrder = original;
            }
        }

        [Test]
        public void Clicking_an_item_sets_sort_in_service()
        {
            GitRefsSortOrder original = AppSettings.RefsSortOrder;
            try
            {
                // Reset to the default
                AppSettings.RefsSortOrder = GitRefsSortOrder.Descending;

                foreach (var item in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>())
                {
                    item.PerformClick();
                    _onSortOrderChanged.Received(1).Invoke();
                    _onSortOrderChanged.ClearReceivedCalls();
                }
            }
            finally
            {
                AppSettings.RefsSortOrder = original;
            }
        }

        private void AssertOnlyCheckedItemIs(GitRefsSortOrder sortType)
        {
            var matchingSubItem = _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Single(i => i.Tag.Equals(sortType));
            Assert.IsTrue(matchingSubItem.Checked);

            foreach (var otherItem in _itemUnderTest.DropDownItems.Cast<ToolStripMenuItem>().Except(new[] { matchingSubItem }))
            {
                Assert.IsFalse(otherItem.Checked);
            }
        }
    }
}
