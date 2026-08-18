using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RevisionFilterTests
{
    private string[] _revisionFilterDropdowns = null!;
    private bool _branchFilterEnabled;
    private bool _showCurrentBranchOnly;
    private bool _showOnlyFirstParent;
    private bool _showReflogReferences;
    private bool _showSimplifyByDecoration;
    private bool _hideMergeCommits;
    private bool _showFullHistory;
    private bool _showSimplifyMerges;

    [SetUp]
    public void SetUp()
    {
        _revisionFilterDropdowns = AppSettings.RevisionFilterDropdowns;
        _branchFilterEnabled = AppSettings.BranchFilterEnabled;
        _showCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
        _showOnlyFirstParent = AppSettings.ShowOnlyFirstParent;
        _showReflogReferences = AppSettings.ShowReflogReferences;
        _showSimplifyByDecoration = AppSettings.ShowSimplifyByDecoration;
        _hideMergeCommits = AppSettings.HideMergeCommits;
        _showFullHistory = AppSettings.FullHistoryInFileHistory;
        _showSimplifyMerges = AppSettings.SimplifyMergesInFileHistory;

        AppSettings.RevisionFilterDropdowns = ["older"];
        AppSettings.BranchFilterEnabled.Value = false;
        AppSettings.ShowCurrentBranchOnly.Value = false;
        AppSettings.ShowOnlyFirstParent = false;
        AppSettings.ShowReflogReferences.Value = false;
        AppSettings.ShowSimplifyByDecoration = false;
        AppSettings.HideMergeCommits = false;
        AppSettings.FullHistoryInFileHistory = false;
        AppSettings.SimplifyMergesInFileHistory = false;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.RevisionFilterDropdowns = _revisionFilterDropdowns;
        AppSettings.BranchFilterEnabled.Value = _branchFilterEnabled;
        AppSettings.ShowCurrentBranchOnly.Value = _showCurrentBranchOnly;
        AppSettings.ShowOnlyFirstParent = _showOnlyFirstParent;
        AppSettings.ShowReflogReferences.Value = _showReflogReferences;
        AppSettings.ShowSimplifyByDecoration = _showSimplifyByDecoration;
        AppSettings.HideMergeCommits = _hideMergeCommits;
        AppSettings.FullHistoryInFileHistory = _showFullHistory;
        AppSettings.SimplifyMergesInFileHistory = _showSimplifyMerges;
    }

    [AvaloniaTest]
    public void Quick_filter_should_include_presets_and_promote_applied_history()
    {
        FilterToolBar toolbar = CreateToolbar(out IRevisionGridFilter revisionGridFilter, out _);
        FilterToolBar.TestAccessor accessor = toolbar.GetTestAccessor();

        accessor.RevisionFilters.Should().ContainInOrder(
            "older",
            @"--invert-grep --grep=""EXCLUDE_COMMIT_MESSAGE_REGEX_PATTERN""",
            @"--perl-regexp --author=""^(?!.*EXCLUDE_AUTHOR_REGEX_PATTERN)""",
            @"--exclude=refs/remotes/EXCLUDE_REMOTE_REGEX_PATTERN");

        FilterInfo filter = new()
        {
            ByMessage = true,
            Message = "new message",
        };
        revisionGridFilter.FilterChanged += Raise.EventWith(
            revisionGridFilter,
            new FilterChangedEventArgs(filter));

        accessor.RevisionFilters[0].Should().Be("new message");
        AppSettings.RevisionFilterDropdowns[0].Should().Be("new message");
        accessor.RevisionFilter.Text.Should().Be("new message");
        accessor.CommitFilter.IsChecked.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Branch_completion_should_filter_cached_refs_and_report_no_results()
    {
        FilterToolBar toolbar = CreateToolbar(out _, out IGitModule module);
        FilterToolBar.TestAccessor accessor = toolbar.GetTestAccessor();
        IReadOnlyList<IGitRef> refs =
        [
            new GitRef(module, ObjectId.Random(), "refs/heads/feature/filters"),
            new GitRef(module, ObjectId.Random(), "refs/heads/main"),
        ];
        toolbar.RefreshRevisionFunction(_ => refs);

        accessor.BranchFilter.Text = "filter";
        accessor.UpdateBranchFilterItems();

        accessor.BranchFilter.ItemsSource!.Cast<string>().Should().Equal("feature/filters");

        accessor.BranchFilter.Text = "absent";
        accessor.UpdateBranchFilterItems();

        accessor.BranchFilter.ItemsSource!.Cast<string>().Should().Equal(TranslatedStrings.NoResultsFound);
        accessor.BranchFilter.Text.Should().Be("absent");
    }

    [AvaloniaTest]
    public void Branch_validation_should_keep_valid_tokens_and_ignore_missing_revisions()
    {
        FilterToolBar toolbar = CreateToolbar(out IRevisionGridFilter revisionGridFilter, out IGitModule module);
        FilterToolBar.TestAccessor accessor = toolbar.GetTestAccessor();
        IReadOnlyList<IGitRef> refs =
        [
            new GitRef(module, ObjectId.Random(), "refs/heads/main"),
        ];
        toolbar.RefreshRevisionFunction(_ => refs);
        List<string> invalidReferences = [];
        accessor.SetInvalidReferenceHandler(invalidReferences.Add);
        accessor.BranchFilter.Text = "main missing feature/* --first-parent main..topic";

        accessor.ApplyCustomBranchFilter(checkBranch: true);

        invalidReferences.Should().Equal("missing");
        revisionGridFilter.Received(1).SetAndApplyBranchFilter("main feature/* --first-parent main..topic");
        accessor.IsApplyingFilter.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Advanced_filter_primary_action_should_open_dialog_when_inactive()
    {
        FilterToolBar toolbar = CreateToolbar(out IRevisionGridFilter revisionGridFilter, out _);

        toolbar.GetTestAccessor().AdvancedFilter.RaiseEvent(new RoutedEventArgs(SplitButton.ClickEvent));

        revisionGridFilter.Received(1).ShowRevisionFilterDialog();
    }

    [AvaloniaTest]
    public void Advanced_filter_dialog_should_load_enable_and_save_the_original_filter_matrix()
    {
        FilterInfo filter = new()
        {
            ByDateFrom = true,
            DateFrom = new DateTime(2026, 7, 1),
            ByAuthor = true,
            Author = " Nikola ",
            ByMessage = true,
            Message = "message",
            ByCommitsLimit = true,
            CommitsLimit = 120,
            ByPathFilter = true,
            PathFilter = "src/",
            ByBranchFilter = true,
            BranchFilter = "main",
            ShowOnlyFirstParent = true,
            HideMergeCommits = true,
        };
        FormRevisionFilter form = new(Substitute.For<IGitUICommands>(), filter);
        FormRevisionFilter.TestAccessor accessor = form.GetTestAccessor();

        accessor.LoadFilters();

        accessor.SinceCheck.IsChecked.Should().BeTrue();
        accessor.Since.SelectedDate?.Date.Should().Be(new DateTime(2026, 7, 1));
        accessor.Author.IsEnabled.Should().BeTrue();
        accessor.Message.IsEnabled.Should().BeTrue();
        accessor.PathFilter.IsEnabled.Should().BeTrue();
        accessor.BranchFilter.IsEnabled.Should().BeTrue();
        accessor.ReflogCheck.IsChecked = true;
        accessor.CurrentBranchOnlyCheck.IsEnabled.Should().BeFalse();
        accessor.BranchFilterCheck.IsEnabled.Should().BeFalse();

        accessor.ReflogCheck.IsChecked = false;
        accessor.Author.Text = "  updated author  ";
        accessor.DiffContentCheck.IsChecked = true;
        accessor.DiffContent.Text = "  changed line  ";
        accessor.FullHistoryCheck.IsChecked = true;
        accessor.SaveFilters();

        FilterInfo raw = filter with { IsRaw = true };
        raw.Author.Should().Be("updated author");
        raw.ByDiffContent.Should().BeTrue();
        raw.DiffContent.Should().Be("changed line");
        raw.ShowFullHistory.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Filter_surfaces_should_reuse_the_original_translation_identities()
    {
        FilterToolBar toolbar = new();
        ITranslation toolbarTranslation = Substitute.For<ITranslation>();
        toolbar.AddTranslationItems(toolbarTranslation);

        toolbarTranslation.Received(1).AddTranslationItem(
            "FormBrowse",
            "tsmiAdvancedFilter",
            "Text",
            "&Advanced filter");
        toolbarTranslation.Received(1).AddTranslationItem(
            "FormBrowse",
            "tsmiResetAllFilters",
            "Text",
            "&Reset revision filters");

        FormRevisionFilter form = new();
        ITranslation formTranslation = Substitute.For<ITranslation>();
        form.AddTranslationItems(formTranslation);

        formTranslation.Received(1).AddTranslationItem(
            nameof(FormRevisionFilter),
            "$this",
            "Text",
            "Filter");
        formTranslation.Received(1).AddTranslationItem(
            nameof(FormRevisionFilter),
            "_since",
            "Text",
            "&Since");
        formTranslation.Received(1).AddTranslationItem(
            nameof(FormRevisionFilter),
            "_diffContentToolTip",
            "Text",
            "SLOW");
        formTranslation.Received(1).AddTranslationItem(
            nameof(FormRevisionFilter),
            "SimplifyMergesCheck",
            "Text",
            "Simplify mer&ges");
    }

    private static FilterToolBar CreateToolbar(
        out IRevisionGridFilter revisionGridFilter,
        out IGitModule module)
    {
        IGitModule gitModule = Substitute.For<IGitModule>();
        gitModule.IsValidGitWorkingDir().Returns(true);
        revisionGridFilter = Substitute.For<IRevisionGridFilter>();
        FilterToolBar toolbar = new();
        toolbar.Bind(() => gitModule, revisionGridFilter);
        module = gitModule;
        return toolbar;
    }
}
