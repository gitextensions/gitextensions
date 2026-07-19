using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Blame;
using GitUI.Editor;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[SetCulture("en-US")]
[SetUICulture("en-US")]
public sealed class BlameControlTests
{
    private GitBlameLine _gitBlameLine = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        GitBlameCommit blameCommit = new(
            ObjectId.Random(),
            "author1",
            "author1@mail.fake",
            new DateTime(2010, 3, 22, 12, 01, 02),
            "authorTimeZone",
            "committer1",
            "committer1@authormail.com",
            new DateTime(2010, 3, 22, 13, 01, 02),
            "committerTimeZone",
            "test summary commit1",
            "fileName.txt");
        _gitBlameLine = new GitBlameLine(blameCommit, 1, 1, "line1");
    }

    [AvaloniaTest]
    public void BlameControl_should_construct()
    {
        BlameControl control = new();

        control.BlameFile.Should().NotBeNull();
        control.BlameFile.TextEditor.TextArea.LeftMargins.Should().Contain(control.BlameAuthor,
            "the author gutter is a margin of the file editor, replacing the second WinForms editor");
        control.blameRevisionToolStripMenuItem.Should().NotBeNull("the context menu items keep their WinForms names");
        control.showChangesToolStripMenuItem.IsVisible.Should().BeTrue("FormCommitDiff is available");
        control.FindControl<GitUI.CommitInfo.CommitInfo>("CommitInfo").Should().NotBeNull();
    }

    [AvaloniaTest]
    public void BlameControl_should_use_existing_translation_keys_once()
    {
        BlameControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(BlameControl), "blameRevisionToolStripMenuItem", "Text", "Blame &this revision");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "blamePreviousRevisionToolStripMenuItem", "Text", "Blame previous revision");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "showChangesToolStripMenuItem", "Text", "&Show changes");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "copyToClipboardToolStripMenuItem", "Text", "&Copy to clipboard");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "commitHashToolStripMenuItem", "Text", "Commit &hash");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "commitMessageToolStripMenuItem", "Text", "Commit &message");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "allCommitInfoToolStripMenuItem", "Text", "&All commit info");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "_blameActualPreviousRevision", "Text", "&Blame previous revision");
        translation.Received(1).AddTranslationItem(nameof(BlameControl), "_blameVisiblePreviousRevision", "Text", "&Blame previous visible revision");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void BuildAuthorLine_should_format_like_WinForms()
    {
        BlameControl.TestAccessor accessor = new BlameControl().GetTestAccessor();

        // The cases of the WinForms BlameControlTests, including a differing file path.
        (bool ShowAuthorDate, bool ShowAuthor, bool ShowFilePath, bool DisplayAuthorFirst, string ExpectedResult)[] cases =
        [
            (true, true, true, true, "author1 - 3/22/2010 - fileName.txt"),
            (true, true, true, false, "3/22/2010 - author1 - fileName.txt"),
            (false, true, true, false, "author1 - fileName.txt"),
            (true, false, true, false, "3/22/2010 - fileName.txt"),
            (true, true, false, false, "3/22/2010 - author1"),
        ];

        foreach ((bool showAuthorDate, bool showAuthor, bool showFilePath, bool displayAuthorFirst, string expectedResult) in cases)
        {
            StringBuilder line = new();

            accessor.BuildAuthorLine(_gitBlameLine, line, expectedResult.Length + 10, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                "fileName_different.txt", showAuthor, showAuthorDate, showFilePath, displayAuthorFirst);

            line.ToString().Should().StartWith(expectedResult);
        }
    }

    [AvaloniaTest]
    public void BuildAuthorLine_should_omit_an_identical_file_path()
    {
        StringBuilder line = new();

        new BlameControl().GetTestAccessor().BuildAuthorLine(_gitBlameLine, line, 50, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
            "fileName.txt", true, true, true, false);

        line.ToString().Should().StartWith("3/22/2010 - author1");
        line.ToString().Should().NotContain("fileName.txt");
    }

    [AvaloniaTest]
    public void CalculateBlameGutterData_should_put_old_and_recent_commits_in_the_outer_age_buckets()
    {
        BlameControl.TestAccessor accessor = new BlameControl().GetTestAccessor();
        GitBlameLine oldLine = new(CreateCommitWithAuthorTime(DateTime.Now.AddYears(-5)), 1, 1, "old");
        GitBlameLine recentLine = new(CreateCommitWithAuthorTime(DateTime.Now), 2, 2, "recent");

        List<GitBlameEntry> blameEntries = accessor.CalculateBlameGutterData([oldLine, recentLine]);

        blameEntries[0].AgeBucketIndex.Should().Be(0);
        blameEntries[1].AgeBucketIndex.Should().Be(6);
    }

    [AvaloniaTest]
    public void CalculateBlameGutterData_should_put_DateMin_in_the_first_age_bucket()
    {
        BlameControl.TestAccessor accessor = new BlameControl().GetTestAccessor();
        GitBlameLine line = new(CreateCommitWithAuthorTime(DateTime.MinValue), 1, 1, "old");

        List<GitBlameEntry> blameEntries = accessor.CalculateBlameGutterData([line]);

        blameEntries[0].AgeBucketIndex.Should().Be(0);
    }

    private static GitBlameCommit CreateCommitWithAuthorTime(DateTime authorTime)
    {
        return new GitBlameCommit(
            ObjectId.Random(),
            "author1",
            "author1@mail.fake",
            authorTime,
            "authorTimeZone",
            "committer1",
            "committer1@authormail.com",
            authorTime,
            "committerTimeZone",
            "test summary",
            "fileName.txt");
    }
}
