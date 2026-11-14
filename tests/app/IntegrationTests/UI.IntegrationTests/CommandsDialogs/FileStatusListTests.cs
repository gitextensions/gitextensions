using System.ComponentModel.Design;
using GitCommands;
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
    private Func<IGitModule, GitUICommands> _createCommands = null!;
    private GitUICommands _commands = null!;

    [SetUp]
    public void SetUp()
    {
        ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        _createCommands = module => new(serviceContainer, module);
        _commands = _createCommands(Substitute.For<IGitModule>());
        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(x => _commands);

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

    [Test]
    public void SetDiffs_should_keep_the_expanded_state_of_the_folders()
    {
        MultiSelectTreeView treeView = _fileStatusList.GetTestAccessor().FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());

        _fileStatusList.SetDiffs(firstRev, secondRev, [new("a/1"), new("a/2"), new("b/3"), new("b/4")]);
        GetFolder("b").Collapse();

        // refresh with one more file in the collapsed folder and a new folder
        _fileStatusList.SetDiffs(firstRev, secondRev, [new("a/1"), new("a/2"), new("b/3"), new("b/4"), new("b/5"), new("c/6"), new("c/7")]);

        GetFolder("a").IsExpanded.Should().BeTrue();
        GetFolder("b").IsExpanded.Should().BeFalse();
        GetFolder("c").IsExpanded.Should().BeTrue();

        return;

        TreeNode GetFolder(string path) => treeView.Nodes.Cast<TreeNode>().Single(node => node.Tag is RelativePath folder && folder.Value == path);
    }

    [Test]
    public void SetDiffs_should_keep_the_expanded_state_of_a_folder_if_its_parent_folders_change()
    {
        MultiSelectTreeView treeView = _fileStatusList.GetTestAccessor().FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());

        _fileStatusList.SetDiffs(firstRev, secondRev, [new("a/1"), new("a/2"), new("b/c/3"), new("b/c/4")]);
        GetFolder("b/c").Collapse();

        // The sibling folder b/d inserts the common parent folder b
        _fileStatusList.SetDiffs(firstRev, secondRev, [new("a/1"), new("a/2"), new("b/c/3"), new("b/c/4"), new("b/d/5"), new("b/d/6")]);

        GetFolder("b").IsExpanded.Should().BeTrue();
        GetFolder("b/c").IsExpanded.Should().BeFalse();
        GetFolder("b/d").IsExpanded.Should().BeTrue();

        return;

        TreeNode GetFolder(string path) => treeView.Items().Single(node => node.Tag is RelativePath folder && folder.Value == path);
    }

    [Test]
    public void SetDiffs_should_keep_the_expanded_state_of_the_folders_collapsed_by_the_git_grep_workaround()
    {
        FileStatusList.TestAccessor accessor = _fileStatusList.GetTestAccessor();
        MultiSelectTreeView treeView = accessor.FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());
        GitItemStatus[] items = [new("a/1"), new("a/2"), new("b/3"), new("b/4")];

        _fileStatusList.SetDiffs(firstRev, secondRev, items);
        GetFolder("b").Collapse();

        // Starting or clearing a git-grep search collapses the displayed tree before it is replaced
        accessor.WorkaroundTooEarlyDrawing();
        _fileStatusList.SetDiffs(firstRev, secondRev, items);

        GetFolder("a").IsExpanded.Should().BeTrue();
        GetFolder("b").IsExpanded.Should().BeFalse();

        return;

        TreeNode GetFolder(string path) => treeView.Nodes.Cast<TreeNode>().Single(node => node.Tag is RelativePath folder && folder.Value == path);
    }

    [Test]
    public void SetDiffs_should_not_apply_the_expanded_state_of_the_folders_to_another_repo()
    {
        MultiSelectTreeView treeView = _fileStatusList.GetTestAccessor().FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());
        GitItemStatus[] items = [new("a/1"), new("a/2"), new("b/3"), new("b/4")];
        _commands.Module.WorkingDir.Returns("repo1");

        _fileStatusList.SetDiffs(firstRev, secondRev, items);
        treeView.Nodes[1].Collapse();

        // UICommands are replaced before the list is refreshed for the other repo
        IGitModule otherModule = Substitute.For<IGitModule>();
        otherModule.WorkingDir.Returns("repo2");
        _commands = _createCommands(otherModule);
        _fileStatusList.SetDiffs(firstRev, secondRev, items);

        treeView.Nodes.Cast<TreeNode>().Should().OnlyContain(node => node.IsExpanded);
    }

    [Test]
    public void SetFilter_should_restore_the_expanded_state_of_the_folders_when_the_filter_is_cleared()
    {
        MultiSelectTreeView treeView = _fileStatusList.GetTestAccessor().FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());
        GitItemStatus[] items = [new("a/1"), new("a/2"), new("b/3"), new("b/4")];

        _fileStatusList.SetDiffs(firstRev, secondRev, items);
        GetFolder("b").Collapse();

        _fileStatusList.SetFilter("3");
        _fileStatusList.SetFilter("34");
        _fileStatusList.SetDiffs(firstRev, secondRev, items); // refresh while filtered
        _fileStatusList.SetFilter("");

        GetFolder("a").IsExpanded.Should().BeTrue();
        GetFolder("b").IsExpanded.Should().BeFalse();

        return;

        TreeNode GetFolder(string path) => treeView.Nodes.Cast<TreeNode>().Single(node => node.Tag is RelativePath folder && folder.Value == path);
    }

    [TestCase(DiffListSortType.FileExtension)]
    [TestCase(DiffListSortType.FileStatus)]
    [TestCase(DiffListSortType.FilePathFlat)]
    public void Changing_the_grouping_should_keep_the_collapsed_folders_collapsed(DiffListSortType other)
    {
        MultiSelectTreeView treeView = _fileStatusList.GetTestAccessor().FileStatusListView;
        GitRevision firstRev = new(ObjectId.Random());
        GitRevision secondRev = new(ObjectId.Random());
        GitItemStatus[] items = [new("a/1.cs"), new("a/2.txt"), new("b/3.cs"), new("b/4.txt")];
        DiffListSortType original = DiffListSortService.Instance.DiffListSorting;
        try
        {
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;
            _fileStatusList.SetDiffs(firstRev, secondRev, items);
            treeView.Items().Single(node => node.Tag is RelativePath folder && folder.Value == "b").Collapse();

            DiffListSortService.Instance.DiffListSorting = other;
            DiffListSortService.Instance.DiffListSorting = DiffListSortType.FilePath;

            treeView.Items().Single(node => node.Tag is RelativePath folder && folder.Value == "b").IsExpanded.Should().BeFalse();
        }
        finally
        {
            DiffListSortService.Instance.DiffListSorting = original;
        }
    }

    private static void CheckStoreFilter(Color expectedColor, string expectedRegex, FileStatusList.TestAccessor accessor)
    {
        accessor.FilterComboBox.BackColor.Should().Be(expectedColor);
        accessor.Filter?.ToString().Should().Be(expectedRegex);
    }
}
