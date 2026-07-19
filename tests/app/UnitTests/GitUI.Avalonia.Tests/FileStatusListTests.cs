using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Properties;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FileStatusListTests
{
    [AvaloniaTest]
    public void FileStatusList_should_filter_names_and_old_names_and_retain_results_for_invalid_regex()
    {
        FileStatusList control = new();
        GitItemStatus renamed = new("src/new-name.cs")
        {
            IsRenamed = true,
            IsTracked = true,
            OldName = "legacy/old-name.cs",
            RenameCopyPercentage = "100",
        };
        GitItemStatus documentation = new("docs/readme.md") { IsChanged = true, IsTracked = true };
        GitItemStatus rangeDiff = new("range-diff") { IsRangeDiff = true };
        control.SetDiffs([renamed, documentation, rangeDiff]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();

        control.SetFilter("old-name").Should().Be(2, "the renamed file matches its old name and range-diff markers stay visible");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.CountLabel.Text.Should().Be("2 / 3 files");
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-active");

        control.SetFilter("[").Should().Be(2, "an invalid expression keeps the last valid result");
        control.GitItemFilteredStatuses.Should().Equal(renamed, rangeDiff);
        accessor.FilterComboBox.Classes.Should().Contain("file-filter-invalid");
        ToolTip.GetTip(accessor.FilterComboBox).Should().BeOfType<string>().Which.Should().NotBeEmpty();

        control.SetFilter(string.Empty).Should().Be(3);
        control.GitItemFilteredStatuses.Should().Equal(renamed, documentation, rangeDiff);
        accessor.CountLabel.Text.Should().Be("3 files");
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-active");
        accessor.FilterComboBox.Classes.Should().NotContain("file-filter-invalid");
    }

    [AvaloniaTest]
    public void FileStatusList_should_expose_multi_selection_and_invoke_only_bound_context_actions()
    {
        FileStatusList control = new() { SelectionMode = SelectionMode.Multiple };
        GitItemStatus first = new("first.txt") { IsChanged = true, IsTracked = true };
        GitItemStatus second = new("second.txt") { IsChanged = true, IsTracked = true };
        control.SetDiffs([first, second]);
        FileStatusList.TestAccessor accessor = control.GetTestAccessor();
        Window window = new() { Width = 320, Height = 180, Content = control };
        int stageCalls = 0;
        control.BindContextMenu(() => { }, canAutoRefresh: true, () => stageCalls++, unstage: null);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            accessor.List.SelectedItems!.Clear();
            accessor.List.SelectedItems.Add(first);
            accessor.List.SelectedItems.Add(second);

            control.SelectedGitItems.Should().Equal(first, second);
            accessor.UpdateContextMenu();
            accessor.StageMenuItem.IsVisible.Should().BeTrue();
            accessor.StageMenuItem.IsEnabled.Should().BeTrue();
            accessor.UnstageMenuItem.IsVisible.Should().BeFalse();
            accessor.CherryPickMenuItem.IsVisible.Should().BeFalse();

            accessor.StageMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            stageCalls.Should().Be(1);
        }
        finally
        {
            accessor.ContextMenu.Close();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_use_the_complete_primary_status_icon_matrix()
    {
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("added") { IsNew = true })
            .Should().BeSameAs(Images.FileStatusAdded);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("removed") { IsDeleted = true })
            .Should().BeSameAs(Images.FileStatusRemoved);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("modified") { IsChanged = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusModified);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("renamed") { IsRenamed = true, IsTracked = true, RenameCopyPercentage = "100" })
            .Should().BeSameAs(Images.FileStatusRenamed);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("copied") { IsCopied = true, IsTracked = true })
            .Should().BeSameAs(Images.FileStatusCopied);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("unmerged") { IsUnmerged = true, IsTracked = true })
            .Should().BeSameAs(Images.Unmerged);
        FileStatusList.TestAccessor.GetItemImage(new GitItemStatus("range") { IsRangeDiff = true })
            .Should().BeSameAs(Images.DiffR);

        GitItemStatus branchDiff = new("branch-diff")
        {
            IsChanged = true,
            IsTracked = true,
            DiffStatus = DiffBranchStatus.UnequalChange,
        };
        FileStatusList.TestAccessor.GetItemImage(branchDiff).Should().BeSameAs(Images.FileStatusModifiedUnequal);
    }

    [AvaloniaTest]
    public void FileStatusList_should_use_the_complete_resolved_submodule_icon_matrix()
    {
        GitItemStatus item = new("submodule") { IsSubmodule = true, IsTracked = true };
        (SubmoduleStatus Status, bool Dirty, object Expected)[] cases =
        [
            (SubmoduleStatus.FastForward, false, Images.SubmoduleRevisionUp),
            (SubmoduleStatus.FastForward, true, Images.SubmoduleRevisionUpDirty),
            (SubmoduleStatus.Rewind, false, Images.SubmoduleRevisionDown),
            (SubmoduleStatus.Rewind, true, Images.SubmoduleRevisionDownDirty),
            (SubmoduleStatus.NewerTime, false, Images.SubmoduleRevisionSemiUp),
            (SubmoduleStatus.NewerTime, true, Images.SubmoduleRevisionSemiUpDirty),
            (SubmoduleStatus.OlderTime, false, Images.SubmoduleRevisionSemiDown),
            (SubmoduleStatus.OlderTime, true, Images.SubmoduleRevisionSemiDownDirty),
            (SubmoduleStatus.SameCommit, false, Images.FolderSubmodule),
            (SubmoduleStatus.SameCommit, true, Images.SubmoduleDirty),
        ];

        foreach ((SubmoduleStatus status, bool dirty, object expected) in cases)
        {
            GitSubmoduleStatus resolved = new(
                item.Name,
                oldName: null,
                dirty,
                commit: default,
                oldCommit: default,
                addedCommits: null,
                removedCommits: null,
                getCommitData: null,
                _ => status);
            FileStatusList.TestAccessor.GetSubmoduleImage(item, resolved).Should().BeSameAs(expected);
        }
    }

    [AvaloniaTest]
    public void FileStatusList_should_emit_existing_translation_keys_once()
    {
        FileStatusList control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "cboFilterComboBox", "Watermark", "Filter files using a regular expression...");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "NoFiles", "Text", "No changes");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "tsmiStageFile", "Text", "&Stage selected");
        translation.Received(1).AddTranslationItem(nameof(FileStatusList), "_collapseAll", "Text", "C&ollapse all");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }
}
