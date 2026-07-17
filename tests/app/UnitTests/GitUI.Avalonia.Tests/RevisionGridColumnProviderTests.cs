using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGridColumnProviderTests
{
    [Test]
    public void Revision_grid_should_register_the_WinForms_column_provider_order()
    {
        RevisionGridControl control = new();

        control.ColumnProviders.Select(provider => provider.GetType()).Should().Equal(
            typeof(RevisionGraphColumnProvider),
            typeof(MessageColumnProvider),
            typeof(NotesColumnProvider),
            typeof(AvatarColumnProvider),
            typeof(AuthorNameColumnProvider),
            typeof(DateColumnProvider),
            typeof(CommitIdColumnProvider),
            typeof(BuildStatusColumnProvider));
        control.ColumnProviders.Select(provider => provider.Name).Should().Equal(
            "Graph",
            "Message",
            "Notes",
            "Avatar",
            "Author Name",
            "Date",
            "Commit ID",
            "Build Status");
        control.ColumnProviders.Select(provider => provider.Index).Should().Equal(Enumerable.Range(0, 8));
        control.ColumnProviders[3].Column.IsAvailable.Should().BeFalse();
        control.ColumnProviders[7].Column.IsAvailable.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Revision_grid_should_apply_column_visibility_and_width_settings()
    {
        ColumnSettings original = ColumnSettings.Capture();
        try
        {
            AppSettings.ShowRevisionGridGraphColumn = true;
            AppSettings.ShowGitNotesColumn.Value = true;
            AppSettings.ShowAuthorAvatarColumn = true;
            AppSettings.ShowAuthorNameColumn = false;
            AppSettings.ShowDateColumn = true;
            AppSettings.ShowObjectIdColumn = true;

            RevisionGridControl control = new();
            ListBox revisions = control.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("The revision list was not created.");
            revisions.ItemsSource = new[] { CreateRevision() };
            Window window = new()
            {
                Width = 900,
                Height = 160,
                Content = control,
            };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                Grid row = control.GetVisualDescendants()
                    .OfType<Grid>()
                    .Single(grid => grid.Classes.Contains("revision-row"));
                row.ColumnDefinitions.Select(column => column.Width).Should().Equal(
                    new GridLength(22),
                    new GridLength(1, GridUnitType.Star),
                    new GridLength(50),
                    new GridLength(0),
                    new GridLength(0),
                    new GridLength(130),
                    new GridLength(60),
                    new GridLength(0));
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-avatar-cell"))
                    .IsVisible.Should().BeFalse();
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-author-cell"))
                    .IsVisible.Should().BeFalse();

                AppSettings.ShowGitNotesColumn.Value = false;
                AppSettings.ShowAuthorNameColumn = true;
                control.ApplyColumnSettings();

                row.ColumnDefinitions[2].Width.Should().Be(new GridLength(0));
                row.ColumnDefinitions[4].Width.Should().Be(new GridLength(130));
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-notes-cell"))
                    .IsVisible.Should().BeFalse();
                control.GetVisualDescendants().OfType<Control>()
                    .Single(cell => cell.Classes.Contains("revision-author-cell"))
                    .IsVisible.Should().BeTrue();
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            original.Restore();
        }
    }

    [Test]
    public void Revision_grid_column_providers_should_format_dates_notes_ids_and_tooltips()
    {
        GitRevision revision = CreateRevision();
        DateTime now = revision.CommitDate.AddHours(3);

        DateColumnProvider.GetDate(revision, showAuthorDate: true).Should().Be(revision.AuthorDate);
        DateColumnProvider.GetDate(revision, showAuthorDate: false).Should().Be(revision.CommitDate);
        DateColumnProvider.FormatDate(revision.CommitDate, now, relative: true).Should().Be(
            LocalizationHelpers.GetRelativeDateString(now, revision.CommitDate, displayWeeks: false));
        DateColumnProvider.FormatDate(revision.CommitDate, now, relative: false).Should().Be(revision.CommitDate.ToString("G"));
        NotesColumnProvider.FirstLine(revision.Notes).Should().Be("First note");
        CommitIdColumnProvider.GetCharLengthForColumnWidth(width: 55, characterWidth: 7).Should().Be(7);

        AuthorNameColumnProvider authorProvider = new(new AuthorRevisionHighlighting());
        authorProvider.TryGetToolTip(revision, out string? authorToolTip).Should().BeTrue();
        authorToolTip.Should().Contain("Author <author@example.com>");
        authorToolTip.Should().Contain("Committer <committer@example.com>");

        CommitIdColumnProvider idProvider = new();
        idProvider.TryGetToolTip(revision, out string? idToolTip).Should().BeTrue();
        idToolTip.Should().Be(revision.Guid);
    }

    [AvaloniaTest]
    public void Message_provider_should_apply_ref_filters_fill_and_virtual_ahead_behind_labels()
    {
        bool originalFill = AppSettings.FillRefLabels;
        bool originalShowRemoteBranches = AppSettings.ShowRemoteBranches;
        bool originalShowTags = AppSettings.ShowTags;
        bool originalDrawNonRelativesGray = AppSettings.RevisionGraphDrawNonRelativesGray;
        try
        {
            AppSettings.FillRefLabels = true;
            AppSettings.ShowRemoteBranches = false;
            AppSettings.ShowTags = false;
            AppSettings.RevisionGraphDrawNonRelativesGray = false;

            IGitModule module = Substitute.For<IGitModule>();
            GitRevision revision = CreateRevision();
            revision.Refs =
            [
                CreateRef(module, revision.ObjectId, "main", "refs/heads/main", isHead: true),
                CreateRef(module, revision.ObjectId, "origin/main", "refs/remotes/origin/main", isRemote: true),
                CreateRef(module, revision.ObjectId, "v1", "refs/tags/v1", isTag: true),
            ];
            IAheadBehindDataProvider aheadBehindProvider = Substitute.For<IAheadBehindDataProvider>();
            aheadBehindProvider.GetData(Arg.Any<string>()).Returns(
                new Dictionary<string, AheadBehindData>
                {
                    ["main"] = new("main", "refs/remotes/origin/main", AheadCount: "1", BehindCount: string.Empty),
                });

            RevisionGridControl control = new();
            control.SetAheadBehindDataProvider(aheadBehindProvider);
            control.ApplyColumnSettings();
            ListBox revisions = control.FindControl<ListBox>("lstRevisions")
                ?? throw new InvalidOperationException("The revision list was not created.");
            revisions.ItemsSource = new[] { revision };
            Window window = new()
            {
                Width = 900,
                Height = 160,
                Content = control,
            };
            window.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();

                RevisionGridRefRenderer.RefLabelControl[] labels =
                [
                    .. control.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>(),
                ];
                labels.Select(label => label.Label).Should().Equal("main", "↓");
                labels.Should().OnlyContain(label => label.Fill);
                labels.Single(label => label.Label == "↓").IsDashed.Should().BeTrue();
                ((RevisionGraphColumnProvider)control.ColumnProviders[0]).RevisionGraphDrawStyle
                    .Should().Be(RevisionGraphDrawStyle.Normal);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.FillRefLabels = originalFill;
            AppSettings.ShowRemoteBranches = originalShowRemoteBranches;
            AppSettings.ShowTags = originalShowTags;
            AppSettings.RevisionGraphDrawNonRelativesGray = originalDrawNonRelativesGray;
        }
    }

    [AvaloniaTest]
    public void Message_provider_should_render_superproject_checkout_and_additional_refs()
    {
        GitRevision revision = CreateRevision();
        IGitModule module = Substitute.For<IGitModule>();
        IGitRef superprojectRef = CreateRef(
            module,
            revision.ObjectId,
            "release",
            "refs/heads/release");
        IGitRef existingRef = CreateRef(
            module,
            revision.ObjectId,
            "main",
            "refs/heads/main",
            isHead: true);
        revision.Refs = [existingRef];
        SuperProjectInfo superProjectInfo = new()
        {
            CurrentCommit = revision.ObjectId,
            Refs = new Dictionary<ObjectId, IReadOnlyList<IGitRef>>
            {
                [revision.ObjectId] = [existingRef, superprojectRef],
            },
        };

        RevisionGridRefRenderer.RefLabelControl[] labels =
        [
            .. MessageColumnProvider.CreateSuperprojectLabels(revision, superProjectInfo)
                .Cast<RevisionGridRefRenderer.RefLabelControl>(),
        ];

        labels.Should().HaveCount(2);
        labels[0].Icon.Should().Be(RefLabelIcon.Head);
        labels[0].Label.Should().BeEmpty();
        labels[1].Label.Should().Be("release");
        labels[1].IsDashed.Should().BeTrue();

        RevisionGridRefRenderer.RefLabelControl existingLabel = RevisionGridRefRenderer.CreateLabels(
                revision.Refs,
                showTags: true,
                showRemoteBranches: true,
                fill: false,
                getVirtualRef: null,
                superprojectRefs: new HashSet<string>(StringComparer.Ordinal) { existingRef.CompleteName })
            .Should().ContainSingle()
            .Which.Should().BeOfType<RevisionGridRefRenderer.RefLabelControl>().Subject;
        existingLabel.IsDashed.Should().BeTrue();
    }

    private static GitRevision CreateRevision()
        => new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Provider-shaped revision row",
            Author = "Author",
            AuthorEmail = "author@example.com",
            AuthorUnixTime = 1_700_000_000,
            Committer = "Committer",
            CommitterEmail = "committer@example.com",
            CommitUnixTime = 1_700_003_600,
            Notes = "First note\nSecond note",
        };

    private static IGitRef CreateRef(
        IGitModule module,
        ObjectId objectId,
        string name,
        string completeName,
        bool isHead = false,
        bool isRemote = false,
        bool isTag = false)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Module.Returns(module);
        gitRef.ObjectId.Returns(objectId);
        gitRef.Guid.Returns(objectId.ToString());
        gitRef.Name.Returns(name);
        gitRef.LocalName.Returns(name.Split('/')[^1]);
        gitRef.CompleteName.Returns(completeName);
        gitRef.IsHead.Returns(isHead);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.IsTag.Returns(isTag);
        return gitRef;
    }

    private readonly record struct ColumnSettings(
        bool ShowGraph,
        bool ShowNotes,
        bool ShowAvatar,
        bool ShowAuthor,
        bool ShowDate,
        bool ShowObjectId)
    {
        public static ColumnSettings Capture()
            => new(
                AppSettings.ShowRevisionGridGraphColumn,
                AppSettings.ShowGitNotesColumn.Value,
                AppSettings.ShowAuthorAvatarColumn,
                AppSettings.ShowAuthorNameColumn,
                AppSettings.ShowDateColumn,
                AppSettings.ShowObjectIdColumn);

        public void Restore()
        {
            AppSettings.ShowRevisionGridGraphColumn = ShowGraph;
            AppSettings.ShowGitNotesColumn.Value = ShowNotes;
            AppSettings.ShowAuthorAvatarColumn = ShowAvatar;
            AppSettings.ShowAuthorNameColumn = ShowAuthor;
            AppSettings.ShowDateColumn = ShowDate;
            AppSettings.ShowObjectIdColumn = ShowObjectId;
        }
    }
}
