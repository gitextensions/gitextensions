using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
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

        AuthorNameColumnProvider authorProvider = new();
        authorProvider.TryGetToolTip(revision, out string? authorToolTip).Should().BeTrue();
        authorToolTip.Should().Contain("Author <author@example.com>");
        authorToolTip.Should().Contain("Committer <committer@example.com>");

        CommitIdColumnProvider idProvider = new();
        idProvider.TryGetToolTip(revision, out string? idToolTip).Should().BeTrue();
        idToolTip.Should().Be(revision.Guid);
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
