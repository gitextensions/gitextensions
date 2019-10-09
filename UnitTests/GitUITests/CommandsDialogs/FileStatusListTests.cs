using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitUI;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FileStatusListTests
    {
        // Created once for each test
        private Form _form;
        private FileStatusList _fileStatusList;

        [SetUp]
        public void SetUp()
        {
            _form = new Form();
            _fileStatusList = new FileStatusList();
            _fileStatusList.Parent = _form;
            _form.Show(); // must be visible to be able to change the focus
        }

        [TearDown]
        public void TearDown()
        {
            _fileStatusList?.Dispose();
            _fileStatusList = null;
            _form?.Dispose();
            _form = null;
        }

        [Test]
        public void ItemSelections()
        {
            var accessor = _fileStatusList.GetTestAccessor();

            var itemNotInList = new GitItemStatus { Name = "not in list" };
            var item0 = new GitItemStatus { Name = "z.0" };
            var item1 = new GitItemStatus { Name = "x.1" };
            var item2 = new GitItemStatus { Name = "y.2" };
            var items = new List<GitItemStatus> { item0, item1, item2 };

            // alphabetical order
            var itemAt0 = item1;
            var itemAt1 = item2;
            var itemAt2 = item0;
            _fileStatusList.SetDiffs(items: items);

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
            _fileStatusList.SelectedIndex.Should().Be(0);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt0);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

            // SelectedIndex

            _fileStatusList.SelectedIndex = 1;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            _fileStatusList.SelectedIndex = -1;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedIndex = 2;
            _fileStatusList.SelectedIndex = 42; // clears the selection

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedIndex = 1;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            // SelectedItem

            _fileStatusList.SelectedItem = itemAt1;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            _fileStatusList.SelectedItem = null;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedItem = itemAt2;
            _fileStatusList.SelectedItem = itemNotInList; // clears the selection

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedItem = itemAt1;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            // SelectedItems (up to one item)

            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemAt1 };

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            _fileStatusList.SelectedItems = null;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedItems = new List<GitItemStatus> { };

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemAt2 };
            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemNotInList }; // clears the selection

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
            _fileStatusList.SelectedIndex.Should().Be(-1);
            _fileStatusList.SelectedItem.Should().BeNull();
            _fileStatusList.SelectedItems.Should().BeEmpty();

            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemAt1 };

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1);
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

            // SelectedItems (multiple items)

            _fileStatusList.SelectedIndex = 2;
            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemAt2, itemAt0, itemNotInList };

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
            _fileStatusList.SelectedIndex.Should().Be(0);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt0); // focused item
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

            accessor.FileStatusListView.Items[1].Focused = true;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(0);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt2); // LastSelectedItem
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

            _fileStatusList.SelectedIndex = 2;
            _fileStatusList.SelectedItems = new List<GitItemStatus> { itemAt2, itemAt1, itemNotInList };

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt1); // focused item
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

            accessor.FileStatusListView.Items[0].Focused = true;

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
            _fileStatusList.SelectedIndex.Should().Be(1);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt2); // LastSelectedItem
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

            // SelectAll

            _fileStatusList.SelectedIndex = 2;
            _fileStatusList.SelectAll();

            foreach (var item in accessor.FileStatusListView.Items())
            {
                item.Selected.Should().BeTrue();
            }

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
            _fileStatusList.SelectedIndex.Should().Be(0);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt0); // focused item
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt1, itemAt2 });

            // SelectFirstVisibleItem

            _fileStatusList.SelectedIndex = 2;
            _fileStatusList.SelectFirstVisibleItem();

            accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
            _fileStatusList.SelectedIndex.Should().Be(0);
            _fileStatusList.SelectedItem.Should().BeSameAs(itemAt0); // focused item
            _fileStatusList.SelectedItems.Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });
        }

        [Test]
        public void Test_FilterWatermarkLabelVisibility_on_FilterVisibleChange(
            [Values(null, "", "x")] string filterText,
            [Values(true, false)] bool filterFocused,
            [Values(true, false)] bool filterVisible)
        {
            var accessor = _fileStatusList.GetTestAccessor();

            accessor.FilterComboBox.Text = filterText; // must be set first because it does not need to update the visibility

            _fileStatusList.FilterVisible = !filterVisible; // force a change
            _fileStatusList.FilterVisible = filterVisible;

            if (filterFocused)
            {
                accessor.FilterComboBox.Focus(); // must be done after FilterVisible = true
            }

            accessor.FilterWatermarkLabelVisible.Should().Be(filterVisible && !filterFocused && string.IsNullOrEmpty(filterText));
        }

        [Test]
        public void Test_FilterWatermarkLabelVisibility_on_Focus()
        {
            var accessor = _fileStatusList.GetTestAccessor();

            accessor.FilterComboBox.Text = "";
            _fileStatusList.FilterVisible = true;

            accessor.FilterWatermarkLabelVisible.Should().BeTrue();

            accessor.FilterComboBox.Focus();

            accessor.FilterWatermarkLabelVisible.Should().BeFalse();

            accessor.FileStatusListView.Focus();

            accessor.FilterWatermarkLabelVisible.Should().BeTrue();
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("\\.cs", true)]
        public void Test_StoreFilter_valid(string regex, bool active)
        {
            var accessor = _fileStatusList.GetTestAccessor();

            var expectedColor = active ? accessor.ActiveInputColor : SystemColors.Window;
            string expectedRegex = string.IsNullOrEmpty(regex) ? null : regex;

            accessor.StoreFilter(regex);

            CheckStoreFilter(expectedColor, expectedRegex, accessor);
        }

        [Test]
        public void Test_StoreFilter_invalid()
        {
            const string validRegex = "\\.cs";
            const string invalidRegex = "(";

            var accessor = _fileStatusList.GetTestAccessor();

            // set a valid Filter that must not change
            accessor.StoreFilter(validRegex);

            ((Action)(() => accessor.StoreFilter(invalidRegex))).Should().Throw<ArgumentException>();

            CheckStoreFilter(accessor.InvalidInputColor, validRegex, accessor);
        }

        private static void CheckStoreFilter(Color expectedColor, string expectedRegex, FileStatusList.TestAccessor accessor)
        {
            accessor.FilterComboBox.BackColor.Should().Be(expectedColor);
            accessor.Filter?.ToString().Should().Be(expectedRegex);
        }
    }
}