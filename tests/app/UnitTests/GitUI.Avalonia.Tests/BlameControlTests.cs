using System.Drawing;
using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Avatars;
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

    [AvaloniaTest]
    public async Task BlameControl_should_load_one_avatar_request_per_email_and_render_each_commit_run()
    {
        bool originalShowAvatar = AppSettings.BlameShowAuthorAvatar;
        BlameControl? control = null;
        try
        {
            AppSettings.BlameShowAuthorAvatar = true;
            InitialsAvatarProvider imageProvider = new();
            IAvatarProvider provider = Substitute.For<IAvatarProvider>();
            provider.GetAvatarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int>())
                .Returns(call => imageProvider.GetAvatarAsync(
                    call.ArgAt<string>(0),
                    call.ArgAt<string?>(1),
                    call.ArgAt<int>(2)));

            control = new BlameControl(provider);
            GitBlameCommit firstCommit = _gitBlameLine.Commit;
            GitBlameCommit secondCommit = CreateCommitWithAuthorTime(DateTime.Now);
            BlameControl.TestAccessor accessor = control.GetTestAccessor();
            accessor.Blame = new GitBlame(
            [
                _gitBlameLine,
                new GitBlameLine(firstCommit, 2, 2, "line2"),
                new GitBlameLine(secondCommit, 3, 3, "line3"),
            ]);

            (string gutter, _, List<GitBlameEntry> entries) = accessor.BuildBlameContents("fileName.txt");
            int contentVersion = control.BlameAuthor.Initialize(gutter, entries, showAvatars: true);
            await accessor.LoadAvatarsAsync(entries, contentVersion);

            _ = provider.Received(1).GetAvatarAsync("author1@mail.fake", "author1", control.BlameAuthor.AvatarSize);
            control.BlameAuthor.GetAvatarPixelSize(0).Should().Be(new Avalonia.PixelSize(control.BlameAuthor.AvatarSize, control.BlameAuthor.AvatarSize));
            control.BlameAuthor.GetAvatarPixelSize(1).Should().BeNull("continuation lines keep the original empty avatar slot");
            control.BlameAuthor.GetAvatarPixelSize(2).Should().Be(new Avalonia.PixelSize(control.BlameAuthor.AvatarSize, control.BlameAuthor.AvatarSize));
        }
        finally
        {
            control?.CancelBackgroundTasks();
            AppSettings.BlameShowAuthorAvatar = originalShowAvatar;
        }
    }

    [AvaloniaTest]
    public async Task BlameAuthorMargin_should_reject_stale_avatars_and_clear_owned_images()
    {
        BlameControl control = new();
        BlameAuthorMargin margin = control.BlameAuthor;
        byte[] imageData = (await new InitialsAvatarProvider().GetAvatarAsync("author1@mail.fake", "author1", margin.AvatarSize))!;
        GitBlameEntry entry = new() { AgeBucketColor = Color.Green };

        int staleVersion = margin.Initialize("first", [entry], showAvatars: true);
        int currentVersion = margin.Initialize("second", [new GitBlameEntry { AgeBucketColor = Color.Green }], showAvatars: true);

        margin.SetAvatar(0, imageData, staleVersion);
        margin.GetAvatarPixelSize(0).Should().BeNull();

        margin.SetAvatar(0, imageData, currentVersion);
        margin.GetAvatarPixelSize(0).Should().Be(new Avalonia.PixelSize(margin.AvatarSize, margin.AvatarSize));

        margin.Clear();
        margin.GetAvatarPixelSize(0).Should().BeNull();
    }

    [AvaloniaTest]
    public void BuildBlameContents_should_omit_avatar_and_age_entries_when_avatars_are_disabled()
    {
        bool originalShowAvatar = AppSettings.BlameShowAuthorAvatar;
        try
        {
            AppSettings.BlameShowAuthorAvatar = false;
            BlameControl control = new();
            BlameControl.TestAccessor accessor = control.GetTestAccessor();
            accessor.Blame = new GitBlame([_gitBlameLine]);

            (string gutter, string body, List<GitBlameEntry> entries) = accessor.BuildBlameContents("fileName.txt");

            gutter.Should().NotBeEmpty("the combined author text margin remains visible");
            body.Should().Contain("line1");
            entries.Should().BeEmpty("WinForms hides the avatar margin and its age marker together");
        }
        finally
        {
            AppSettings.BlameShowAuthorAvatar = originalShowAvatar;
        }
    }

    [AvaloniaTest]
    public void BlameControl_should_resolve_age_and_highlight_colors_per_theme()
    {
        (IReadOnlyList<Color> Ages, Color Highlight) light = GetThemeColors(ThemeVariant.Light);
        (IReadOnlyList<Color> Ages, Color Highlight) dark = GetThemeColors(ThemeVariant.Dark);

        light.Ages.Should().HaveCount(7);
        dark.Ages.Should().HaveCount(7);
        light.Ages[0].Should().Be(Color.FromArgb(247, 252, 245));
        light.Ages[6].Should().Be(Color.FromArgb(0, 68, 27));
        dark.Ages[0].Should().Be(Color.FromArgb(38, 58, 44));
        dark.Ages[6].Should().Be(Color.FromArgb(120, 227, 145));
        light.Highlight.Should().Be(Color.FromArgb(227, 227, 227));
        dark.Highlight.Should().Be(Color.FromArgb(65, 65, 65));
    }

    [AvaloniaTest]
    public void BlameControl_splitters_should_restore_the_commit_height_and_resizable_author_width()
    {
        Dictionary<string, string?> values = [];
        IConfigValueStore store = Substitute.For<IConfigValueStore>();
        store.GetValue(Arg.Any<string>()).Returns(call => values.GetValueOrDefault(call.Arg<string>()));
        store.When(config => config.SetValue(Arg.Any<string>(), Arg.Any<string?>()))
            .Do(call => values[call.ArgAt<string>(0)] = call.ArgAt<string?>(1));
        SettingsSource settings = new SettingsSource<IConfigValueStore>(store);

        BlameControl first = new() { Name = nameof(BlameControl) };
        Window firstWindow = new() { Width = 800, Height = 600, Content = first };
        firstWindow.Show();
        try
        {
            SplitterManager manager = new(settings);
            first.InitSplitterManager(new NestedSplitterManager(manager, "revisionDiff"));
            manager.RestoreSplitters();
            first.GetTestAccessor().SplitContainer.RowDefinitions[0].Height = new GridLength(120);
            first.GetTestAccessor().AuthorMarginWidth = 140;
            Dispatcher.UIThread.RunJobs();
            manager.SaveSplitters();
        }
        finally
        {
            firstWindow.Close();
        }

        values["revisionDiff.BlameControl.splitContainer1_Distance"].Should().Be("120");
        values["revisionDiff.BlameControl.splitContainer2_Distance"].Should().Be("140");

        BlameControl restored = new() { Name = nameof(BlameControl) };
        SplitterManager restoredManager = new(settings);
        restored.InitSplitterManager(new NestedSplitterManager(restoredManager, "revisionDiff"));
        restoredManager.RestoreSplitters();

        restored.GetTestAccessor().SplitContainer.RowDefinitions[0].Height.Should().Be(new GridLength(120));
        restored.GetTestAccessor().AuthorMarginWidth.Should().Be(140);

        BlameControl embedded = new() { Name = nameof(BlameControl) };
        embedded.HideCommitInfo();
        SplitterManager embeddedManager = new(settings);
        embedded.InitSplitterManager(new NestedSplitterManager(embeddedManager, "revisionDiff"));
        embeddedManager.RestoreSplitters();

        embedded.GetTestAccessor().SplitContainer.RowDefinitions[0].Height.Should().Be(new GridLength(0),
            "restoring a standalone Blame splitter must not reveal blank commit-info space in RevisionDiffControl");
    }

    private static (IReadOnlyList<Color> Ages, Color Highlight) GetThemeColors(ThemeVariant theme)
    {
        BlameControl control = new();
        Window window = new()
        {
            RequestedThemeVariant = theme,
            Content = control,
        };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            BlameControl.TestAccessor accessor = control.GetTestAccessor();
            return (accessor.GetAgeBucketGradientColors(), accessor.GetCommitHighlightColor());
        }
        finally
        {
            window.Close();
        }
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
