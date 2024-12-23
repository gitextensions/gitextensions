﻿using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace UITests.UserControls;

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
        _fileStatusList.Dispose();
        _form.Dispose();
    }

    [Test]
    public void ItemSelections()
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        GitItemStatus itemNotInList = new(name: "not in list");
        GitItemStatus item0 = new(name: "z.0");
        GitItemStatus item1 = new(name: "x.1");
        GitItemStatus item2 = new(name: "y.2");
        List<GitItemStatus> items = [item0, item1, item2];

        // alphabetical order
        GitItemStatus itemAt0 = item1;
        GitItemStatus itemAt1 = item2;
        GitItemStatus itemAt2 = item0;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());
        _fileStatusList.SetDiffs(firstRev: firstRev, secondRev: secondRev, items: items);

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
        _fileStatusList.SelectedIndex.Should().Be(0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

        _fileStatusList.SelectedGitItem.Should().BeSameAs(_fileStatusList.SelectedItem.Item);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(_fileStatusList.SelectedItems.Items());

        // SelectedIndex

        _fileStatusList.SelectedIndex = 1;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedIndex = -1;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedIndex = 2;
        _fileStatusList.SelectedIndex = 42; // clears the selection

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedIndex = 1;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedGitItem

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedGitItem = null;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt2;
        _fileStatusList.SelectedGitItem = itemNotInList; // clears the selection

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (up to one item)

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedItems = null;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { };

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2 };
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemNotInList }; // clears the selection

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedIndex.Should().Be(-1);
        _fileStatusList.SelectedGitItem.Should().BeNull();
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (multiple items)

        _fileStatusList.SelectedIndex = 2;
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt0, itemNotInList };

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
        _fileStatusList.SelectedIndex.Should().Be(0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // focused item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        accessor.FileStatusListView.Items[1].Focused = true;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt2); // LastSelectedItem
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        _fileStatusList.SelectedIndex = 2;
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt1, itemNotInList };

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(1);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1); // focused item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

        accessor.FileStatusListView.Items[0].Focused = true;

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
        _fileStatusList.SelectedIndex.Should().Be(1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt2); // LastSelectedItem
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

        // SelectAll

        _fileStatusList.SelectedIndex = 2;
        _fileStatusList.SelectAll();

        foreach (ListViewItem item in accessor.FileStatusListView.Items())
        {
            item.Selected.Should().BeTrue();
        }

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
        _fileStatusList.SelectedIndex.Should().Be(0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // focused item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt1, itemAt2 });

        // SelectFirstVisibleItem

        _fileStatusList.SelectedIndex = 2;
        _fileStatusList.SelectFirstVisibleItem();

        accessor.FileStatusListView.FocusedItem.Index.Should().Be(0);
        _fileStatusList.SelectedIndex.Should().Be(0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // focused item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });
    }

    [Test]
    public void Test_FilterWatermarkLabelVisibility_on_FilterVisibleChange(
        [Values(null, "", "x")] string filterText,
        [Values(true, false)] bool filterFocused)
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        accessor.FilterComboBox.Text = filterText; // must be set first because it does not need to update the visibility
        accessor.SetFileStatusListVisibility(filesPresent: true);

        if (filterFocused)
        {
            accessor.FilterComboBox.Focus(); // must be done after FilterVisible = true
        }

        accessor.FilterWatermarkLabelVisible.Should().Be(!filterFocused && string.IsNullOrEmpty(filterText));
    }

    [Test]
    public void Test_FilterWatermarkLabelVisibility_on_Focus()
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        accessor.FilterComboBox.Text = "";
        accessor.SetFileStatusListVisibility(filesPresent: true);
        accessor.FilterWatermarkLabelVisible.Should().BeTrue();

        accessor.FilterComboBox.Focus();
        accessor.SetFileStatusListVisibility(filesPresent: true);
        accessor.FilterWatermarkLabelVisible.Should().BeFalse();

        accessor.FileStatusListView.Focus();
        accessor.SetFileStatusListVisibility(filesPresent: true);
        accessor.FilterWatermarkLabelVisible.Should().BeTrue();
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("\\.cs", true)]
    public void Test_StoreFilter_valid(string regex, bool active)
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        Color expectedColor = active ? accessor.ActiveInputColor : SystemColors.Window;
        string expectedRegex = string.IsNullOrEmpty(regex) ? null : regex;

        _fileStatusList.SetFilter(regex);
        accessor.SetFileStatusListVisibility(filesPresent: true);

        CheckStoreFilter(expectedColor, expectedRegex, accessor);

        accessor.DeleteFilterButton.Visible.Should().Be(active);
    }

    [Test]
    public void Test_StoreFilter_invalid()
    {
        const string validRegex = "\\.cs";
        const string invalidRegex = "(";

        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

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
