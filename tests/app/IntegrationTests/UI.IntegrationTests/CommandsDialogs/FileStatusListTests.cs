using System.ComponentModel.Design;
using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FileStatusListTests
{
    // Created once for each test
    private Form _form;
    private FileStatusList _fileStatusList;

    [SetUp]
    public void SetUp()
    {
        ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        IGitModule module = Substitute.For<IGitModule>();
        GitUICommands commands = new(serviceContainer, module);
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(x => commands);

        _form = new Form();
        _fileStatusList = new FileStatusList
        {
            Parent = _form,
            UICommandsSource = uiCommandsSource
        };
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

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

        _fileStatusList.SelectedGitItem.Should().BeSameAs(_fileStatusList.SelectedItem.Item);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(_fileStatusList.SelectedItems.Items());

        // SelectedIndex

        _fileStatusList.SelectedGitItems = [itemAt1];

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedItems = [];

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = [itemNotInList]; // clears the selection

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = [itemAt1];

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedGitItem

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedGitItem = null;

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt2;
        _fileStatusList.SelectedGitItem = itemNotInList; // clears the selection

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (up to one item)

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedItems = null;

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { };

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2 };
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemNotInList }; // clears the selection

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (multiple items)

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt0, itemNotInList };

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        accessor.FileStatusListView.FocusedNode = accessor.FileStatusListView.Nodes[1];

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt1, itemNotInList };

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

        accessor.FileStatusListView.FocusedNode = accessor.FileStatusListView.Nodes[0];

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

        // SelectAll

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectAll();

        foreach (TreeNode item in accessor.FileStatusListView.Items())
        {
            accessor.FileStatusListView.SelectedNodes.Contains(item).Should().BeTrue();
        }

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt1, itemAt2 });

        // SelectFirstVisibleItem

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectFirstVisibleItem();

        accessor.FileStatusListView.FocusedNode.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt0); // first item
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // first item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

        // no focus
        accessor.FileStatusListView.FocusedNode = null;

        accessor.FileStatusListView.FocusedNode.Should().BeNull();
        _fileStatusList.SelectedItem.Item.Should().Be(itemAt0); // unchanged
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // unchanged
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });
    }

    [Test]
    public void Test_FilterWatermarkLabelVisibility_on_FilterVisibleChange(
        [Values(null, "", "x")] string filterText,
        [Values(true, false)] bool filterFocused)
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        accessor.FilterComboBox.Text = filterText; // must be set first because it does not need to update the visibility
        accessor.SetFileStatusListVisibility(showNoFiles: false);

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
        accessor.SetFileStatusListVisibility(showNoFiles: false);
        accessor.FilterWatermarkLabelVisible.Should().BeTrue();

        accessor.FilterComboBox.Focus();
        accessor.SetFileStatusListVisibility(showNoFiles: false);
        accessor.FilterWatermarkLabelVisible.Should().BeFalse();

        accessor.FileStatusListView.Focus();
        accessor.SetFileStatusListVisibility(showNoFiles: false);
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
        accessor.SetFileStatusListVisibility(showNoFiles: false);

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
