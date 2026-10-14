using System.ComponentModel.Design;
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
    private Form _form = null!;
    private FileStatusList _fileStatusList = null!;
    private IGitModule _module = null!;

    [SetUp]
    public void SetUp()
    {
        ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        _module = Substitute.For<IGitModule>();
        GitUICommands commands = new(serviceContainer, _module);
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
    public void Large_tree_filters_discard_obsolete_results_and_preserve_selection()
    {
        _fileStatusList.Bind(() => { }, isFileTreeMode: true);
        GitItemStatus[] items = Enumerable.Range(0, 6000).Select(index => new GitItemStatus($"dir/file{index:D5}.txt")).ToArray();
        _fileStatusList.SetDiffs(null, new GitRevision(ObjectId.Random()), items);
        WaitForTree();
        _fileStatusList.SelectedGitItems = [items[1]];

        _fileStatusList.SetFilter("no matching file");
        _fileStatusList.SetFilter("file0000[12]");
        WaitForTree();

        _fileStatusList.AllItems.Select(item => item.Item).Should().Equal(items[1], items[2]);
        _fileStatusList.SelectedItems.Select(item => item.Item).Should().Equal(items[1]);
    }

    [Test]
    public void Clearing_or_disposing_a_list_cancels_pending_tree_publication()
    {
        _fileStatusList.Bind(() => { }, isFileTreeMode: true);
        GitItemStatus[] items = Enumerable.Range(0, 6000).Select(index => new GitItemStatus($"dir/file{index:D5}.txt")).ToArray();
        _fileStatusList.SetDiffs(null, new GitRevision(ObjectId.Random()), items);
        Task pending = _fileStatusList.TreeLoading;
        _fileStatusList.ClearDiffs();
        UITest.ProcessUntil("obsolete tree", () => pending.IsCompleted, maxMilliseconds: 10000);
        pending.IsCompletedSuccessfully.Should().BeTrue();
        _fileStatusList.AllItems.Should().BeEmpty();

        _fileStatusList.SetDiffs(null, new GitRevision(ObjectId.Random()), items);
        pending = _fileStatusList.TreeLoading;
        _fileStatusList.Dispose();
        UITest.ProcessUntil("disposed tree", () => pending.IsCompleted, maxMilliseconds: 10000);
        pending.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Test]
    public void A_filter_during_revision_loading_is_applied_to_the_new_revision()
    {
        _fileStatusList.Bind(() => { }, isFileTreeMode: true);
        _fileStatusList.SetDiffs(null, new GitRevision(ObjectId.Random()), [new GitItemStatus("previous.txt")]);
        GitRevision revision = new(ObjectId.Random());
        GitItemStatus[] items = Enumerable.Range(0, 6000).Select(index => new GitItemStatus($"new/file{index:D5}.txt")).ToArray();
        using ManualResetEventSlim started = new();
        using ManualResetEventSlim release = new();
        _module.IsValidGitWorkingDir().Returns(true);
        _module.GetTreeFiles(revision.ObjectId, true, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            started.Set();
            release.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
            return items;
        });

        Task loading = _fileStatusList.SetDiffsAsync([revision], revision.ObjectId, CancellationToken.None);
        try
        {
            UITest.ProcessUntil("loading revision", () => started.IsSet, maxMilliseconds: 10000);
            _fileStatusList.SetFilter("file00001");
            _fileStatusList.AllItems.Should().BeEmpty();
        }
        finally
        {
            release.Set();
        }

        UITest.ProcessUntil("new revision", () => loading.IsCompleted, maxMilliseconds: 10000);
        loading.IsCompletedSuccessfully.Should().BeTrue();
        _fileStatusList.AllItems.Select(item => item.Item).Should().Equal(items[1]);
    }

    private void WaitForTree()
    {
        UITest.ProcessUntil("file tree", () => _fileStatusList.TreeLoading.IsCompleted, maxMilliseconds: 10000);
        _fileStatusList.TreeLoading.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Test]
    public void SelectAll_should_expand_folders_and_focus_the_first_node()
    {
        const int folderCount = 4;
        const int filesPerFolder = 50;
        GitItemStatus[] items = Enumerable.Range(0, folderCount * filesPerFolder)
            .Select(index => new GitItemStatus($"folder{index / filesPerFolder}/subfolder/file{index:D3}"))
            .ToArray();
        _fileStatusList.SetDiffs(null, new GitRevision(ObjectId.Random()), items);
        MultiSelectTreeView tree = _fileStatusList.GetTestAccessor().FileStatusListView;
        tree.CollapseAll();

        _fileStatusList.SelectAll();

        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(items);
        tree.Items().Where(node => node.Nodes.Count > 0).Should().OnlyContain(node => node.IsExpanded);
        tree.FocusedNode.Should().BeSameAs(tree.Nodes[0]);
        tree.Nodes[0].IsVisible.Should().BeTrue();
        tree.UpdateSuspended.Should().BeFalse();
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

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(0);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt0);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

        _fileStatusList.SelectedGitItem.Should().BeSameAs(_fileStatusList.SelectedItem!.Item);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(_fileStatusList.SelectedItems.Items());

        // SelectedIndex

        _fileStatusList.SelectedGitItems = [itemAt1];

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedItems = [];

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = [itemNotInList]; // clears the selection

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = [itemAt1];

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedGitItem

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedGitItem = null;

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt2;
        _fileStatusList.SelectedGitItem = itemNotInList; // clears the selection

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItem = itemAt1;

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (up to one item)

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        _fileStatusList.SelectedItems = null!;

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems!.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { };

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2 };
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemNotInList }; // clears the selection

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(2); // unchanged
        _fileStatusList.SelectedItem.Should().BeNull(); // empty
        _fileStatusList.SelectedGitItem.Should().BeNull(); // empty
        _fileStatusList.SelectedItems.Items().Should().BeEmpty();

        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt1 };

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt1);
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt1);
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1 });

        // SelectedItems.Items() (multiple items)

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt0, itemNotInList };

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        accessor.FileStatusListView.FocusedNode = accessor.FileStatusListView.Nodes[1];

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt2 });

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectedGitItems = new List<GitItemStatus> { itemAt2, itemAt1, itemNotInList };

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(1);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt1, itemAt2 });

        accessor.FileStatusListView.FocusedNode = accessor.FileStatusListView.Nodes[0];

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(0);
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

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(0);
        _fileStatusList.SelectedItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedGitItem.Should().BeNull(); // due to multi-selection
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0, itemAt1, itemAt2 });

        // SelectFirstVisibleItem

        _fileStatusList.SelectedGitItems = [itemAt2];
        _fileStatusList.SelectFirstVisibleItem();

        accessor.FileStatusListView.FocusedNode!.Index.Should().Be(0);
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt0); // first item
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // first item
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });

        // no focus
        accessor.FileStatusListView.FocusedNode = null;

        accessor.FileStatusListView.FocusedNode.Should().BeNull();
        _fileStatusList.SelectedItem!.Item.Should().Be(itemAt0); // unchanged
        _fileStatusList.SelectedGitItem.Should().BeSameAs(itemAt0); // unchanged
        _fileStatusList.SelectedItems.Items().Should().BeEquivalentTo(new List<GitItemStatus> { itemAt0 });
    }

    [Test]
    public void Test_FilterWatermarkLabelVisibility_on_FilterVisibleChange(
        [Values(null, "", "x")] string? filterText,
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
    public void Test_StoreFilter_valid(string? regex, bool active)
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();

        Color expectedColor = active ? accessor.ActiveInputColor : SystemColors.Window;
        string? expectedRegex = string.IsNullOrEmpty(regex) ? null : regex;

        _fileStatusList.SetFilter(regex!);
        accessor.SetFileStatusListVisibility(showNoFiles: false);

        CheckStoreFilter(expectedColor, expectedRegex!, accessor);

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
