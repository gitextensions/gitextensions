using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommitInfo;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager.CommitDataRenders;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class CaptureFixtureClockTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void Fixed_capture_clock_should_drive_the_actual_headers_first_settled_render_and_later_refresh()
    {
        WithHeader((header, window) =>
        {
            DateTime revisionUtc = DateTime.UtcNow.Date.AddDays(-10).AddHours(12);
            DateTime fixtureNowUtc = revisionUtc.AddHours(4);
            GitRevision first = CreateRevision(revisionUtc);
            using CaptureFixtureClock clock = CaptureFixtureClock.Apply([header], fixtureNowUtc);
            header.ShowCommitInfo(first, null);
            window.Show();
            Settle(window);
            AssertHeaderDate(header, first, new DateFormatter(() => fixtureNowUtc));

            GitRevision later = CreateRevision(revisionUtc.AddHours(-2));
            Dispatcher.UIThread.Post(() => header.ShowCommitInfo(later, null));
            Settle(window);
            AssertHeaderDate(header, later, new DateFormatter(() => fixtureNowUtc));
            header.GetTestAccessor().RevisionHeader.GetPlainText().Should().NotContain(
                new DateFormatter(() => fixtureNowUtc).FormatDateAsRelativeLocal(DateTimeOffset.FromUnixTimeSeconds(first.CommitUnixTime)),
                "the later actual header must render the refreshed revision, not retain its first captured date");
        });
    }

    [AvaloniaTest]
    public void Null_capture_clock_should_keep_the_actual_header_live_and_restore_live_refresh_after_a_fixed_scope()
    {
        WithHeader((header, window) =>
        {
            DateTime revisionUtc = DateTime.UtcNow.Date.AddDays(-10).AddHours(12);
            DateTime fixtureNowUtc = revisionUtc.AddHours(4);
            GitRevision revision = CreateRevision(revisionUtc);
            using CaptureFixtureClock live = CaptureFixtureClock.Apply([header], null);
            header.ShowCommitInfo(revision, null);
            window.Show();
            Settle(window);
            AssertHeaderDate(header, revision, new DateFormatter());
            string liveText = header.GetTestAccessor().RevisionHeader.GetPlainText();

            using (CaptureFixtureClock fixedClock = CaptureFixtureClock.Apply([header], fixtureNowUtc))
            {
                header.ShowCommitInfo(revision, null);
                Settle(window);
                AssertHeaderDate(header, revision, new DateFormatter(() => fixtureNowUtc));
                header.GetTestAccessor().RevisionHeader.GetPlainText().Should().NotBe(liveText);
            }

            Dispatcher.UIThread.Post(() => header.ShowCommitInfo(revision, null));
            Settle(window);
            AssertHeaderDate(header, revision, new DateFormatter());
            header.GetTestAccessor().RevisionHeader.GetPlainText().Should().Be(liveText,
                "disposing the capture input must restore the actual renderer's live formatter for subsequent refreshes");
        });
    }

    private static void AssertHeaderDate(CommitInfoHeader header, GitRevision revision, IDateFormatter formatter)
    {
        var block = header.GetTestAccessor().RevisionHeader;
        block.GetPlainText().Should().Contain(formatter.FormatDateAsRelativeLocal(DateTimeOffset.FromUnixTimeSeconds(revision.CommitUnixTime)));
        block.Bounds.Height.Should().BeGreaterThan(0);
        block.TextLayout.TextLines.Should().NotBeEmpty();
    }

    private static GitRevision CreateRevision(DateTime revisionUtc)
        => new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Author = "Capture author",
            Committer = "Capture author",
            AuthorUnixTime = new DateTimeOffset(revisionUtc).ToUnixTimeSeconds(),
            CommitUnixTime = new DateTimeOffset(revisionUtc).ToUnixTimeSeconds(),
            ParentIds = [],
        };

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static void WithHeader(Action<CommitInfoHeader, Window> verify)
    {
        bool originalShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        Window? window = null;
        try
        {
            AppSettings.ShowAuthorAvatarInCommitInfo = false;
            CommitInfoHeader header = new();
            window = new Window { Width = 800, Height = 240, Content = header };
            verify(header, window);
        }
        finally
        {
            window?.Close();
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatar;
        }
    }
}
