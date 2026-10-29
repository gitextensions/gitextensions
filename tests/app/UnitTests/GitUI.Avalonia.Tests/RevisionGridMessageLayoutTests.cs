using System.Reflection;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using SkiaSharp;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;
using Font = GitExtensions.Shims.WinForms.Font;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridMessageLayoutTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [TestCase(9, "subject")]
    [TestCase(11, "subject")]
    [TestCase(9, "主题 _& café e\u0301")]
    [TestCase(11, "主题 _& café e\u0301")]
    public void Subject_then_body_should_use_source_NoPadding_advances_and_finite_ellipsis(int points, string subject)
    {
        using RowFixture fixture = new(points, subject, new string('W', 150));
        fixture.SetWidth(250);

        int subjectWidth = SourceWidth(fixture.Subject);
        fixture.Cell.Bounds.Width.Should().Be(250, "the painter owns the entire column, not a margin-reduced cell");
        Origin(fixture.Subject, fixture.Cell).X.Should().Be(6);
        Origin(fixture.Body, fixture.Cell).X.Should().Be(6 + subjectWidth);
        fixture.Subject.Bounds.Width.Should().Be(250 - 26 - 6);
        fixture.Body.Bounds.Width.Should().Be(250 - 26 - 6 - subjectWidth);
        fixture.Body.TextLayout.TextLines.Should().Contain(line => line.HasCollapsed,
            "EndEllipsis receives the actual remaining body span instead of infinite StackPanel width");
        fixture.Subject.Text.Should().Be(subject);
        fixture.Body.Text.Should().Be(" " + new string('W', 150));
        fixture.AssertIndicatorPainted();

        fixture.SetWidth(500);
        Origin(fixture.Body, fixture.Cell).X.Should().Be(6 + subjectWidth);
        fixture.Body.Bounds.Width.Should().Be(500 - 26 - 6 - subjectWidth);
        fixture.AssertIndicatorPainted();
        fixture.SetWidth(250);
        fixture.Body.Bounds.Width.Should().Be(250 - 26 - 6 - subjectWidth);
        fixture.AssertIndicatorPainted();
    }

    [AvaloniaTest]
    [TestCase(9, true)]
    [TestCase(11, true)]
    [TestCase(9, false)]
    [TestCase(11, false)]
    public void An_overlong_subject_should_ellipsis_without_body_or_indicator_paint(int points, bool showBody)
    {
        using RowFixture fixture = new(points, new string('W', 100), "body text", showBody: showBody);
        fixture.SetWidth(180);

        fixture.Subject.Bounds.Width.Should().Be(180 - 26 - 6);
        fixture.Subject.TextLayout.TextLines.Should().Contain(line => line.HasCollapsed);
        fixture.Body.Bounds.Width.Should().Be(0);
        fixture.Indicator.IsVisible.Should().BeTrue("the source still reserves its multiline span before painting the title");
        fixture.AssertIndicatorNotPainted();
        if (!showBody)
        {
            fixture.Body.Text.Should().BeEmpty();
        }
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    public void A_bare_subject_should_keep_its_literal_text_without_reserving_a_multiline_span(int points)
    {
        using RowFixture fixture = new(points, "literal & subject", body: null);
        fixture.SetWidth(250);

        fixture.Subject.Text.Should().Be("literal & subject");
        fixture.Subject.Bounds.Width.Should().Be(250 - 6);
        fixture.Subject.TextLayout.TextLines.Should().NotContain(line => line.HasCollapsed);
        fixture.Body.Text.Should().BeEmpty();
        fixture.Body.Bounds.Width.Should().Be(0);
        fixture.Indicator.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    public void A_source_fitting_subject_should_not_gain_an_ellipsis_from_a_different_portable_glyph_advance(int points)
    {
        using RowFixture fixture = new(points, "literal subject at the exact fitting boundary", body: null);
        int sourceWidth = SourceWidth(fixture.Subject);
        fixture.SetWidth(6 + sourceWidth);

        fixture.Subject.Bounds.Width.Should().Be(sourceWidth);
        fixture.Subject.TextTrimming.Should().Be(TextTrimming.None);
        fixture.Subject.TextLayout.TextLines.Should().NotContain(line => line.HasCollapsed);
        fixture.SetWidth(6 + sourceWidth - 1);
        fixture.Subject.TextTrimming.Should().Be(TextTrimming.CharacterEllipsis);
    }

    [AvaloniaTest]
    [TestCase(9, "main", 27)]
    [TestCase(9, "feature&parity", 77)]
    [TestCase(9, "&", 10)]
    [TestCase(9, "main&", 37)]
    [TestCase(11, "main", 33)]
    [TestCase(11, "feature&parity", 97)]
    [TestCase(11, "&", 12)]
    [TestCase(11, "main&", 45)]
    public void Literal_subject_advance_should_match_retained_native_NoPrefix_SingleLine_NoPadding_metrics(
        int points, string subject, int nativeWidth)
    {
        using RowFixture fixture = new(points, subject, "body");
        fixture.SetWidth(250);

        // These are original TextRenderer measurements, not authored dimensions.
        // The portable platform exercises its defined font-measurement substitute.
        int expectedWidth = OperatingSystem.IsWindows() ? nativeWidth : SourceWidth(fixture.Subject);
        fixture.Subject.Bounds.Width.Should().Be(250 - 26 - 6);
        Origin(fixture.Body, fixture.Cell).X.Should().Be(6 + expectedWidth);
        fixture.Subject.Text.Should().Be(subject, "NoPrefix paints ampersands literally");
    }

    [AvaloniaTest]
    [TestCase(25, false)]
    [TestCase(51, false)]
    [TestCase(52, true)]
    [TestCase(60, true)]
    public void Indicator_threshold_and_right_edge_should_use_the_full_column(int width, bool visible)
    {
        using RowFixture fixture = new(9, "i", "body");
        fixture.SetWidth(width);

        fixture.Cell.Bounds.Width.Should().Be(width);
        fixture.Indicator.IsVisible.Should().Be(visible);
        if (visible)
        {
            fixture.Indicator.Bounds.Width.Should().Be(26);
            Origin(fixture.Indicator, fixture.Cell).X.Should().Be(width - 26);
            fixture.AssertIndicatorPainted();
        }
        else
        {
            fixture.Subject.Bounds.Width.Should().Be(width - 6);
            Origin(fixture.Body, fixture.Cell).X.Should().Be(6 + SourceWidth(fixture.Subject));
        }
    }

    [AvaloniaTest]
    [TestCase("fixup! subject", 16, false)]
    [TestCase("fixup! subject", 17, true)]
    [TestCase("fixup! subject", 18, true)]
    [TestCase("fixup! subject", 19, true)]
    [TestCase("fixup! subject", 20, true)]
    [TestCase("squash! subject", 16, false)]
    [TestCase("squash! subject", 17, true)]
    [TestCase("amend! subject", 16, false)]
    [TestCase("amend! subject", 17, true)]
    public void Commit_marker_should_obey_the_source_strict_fit_and_four_pixel_advance(
        string subject, int remainingWidth, bool fits)
    {
        using RowFixture fixture = new(9, subject, body: null);
        IGitRef branch = fixture.CreateRef("refs/heads/b");
        branch.IsHead.Returns(true);
        fixture.Revision.Refs = [branch];
        fixture.Refresh();
        RevisionGridRefRenderer.RefLabelControl label = fixture.Cell.GetVisualDescendants()
            .OfType<RevisionGridRefRenderer.RefLabelControl>().Single();
        int prefixWidth = 6 + (int)label.CapsuleBounds.Width + 5;
        fixture.SetWidth(prefixWidth + remainingWidth);

        Image marker = fixture.Cell.GetVisualDescendants().OfType<Image>()
            .Single(image => image.Classes.Contains("revision-message-marker"));
        Origin(fixture.Subject, fixture.Cell).X.Should().Be(prefixWidth + (fits ? 16 + 4 : 0));
        if (fits)
        {
            marker.Bounds.Width.Should().Be(16,
                "the source paints all sixteen image pixels even when the trailing margin exceeds the remaining span");
            marker.Clip.Should().BeNull();
            fixture.AssertImagePainted(marker);
        }
        else
        {
            marker.Clip.Should().NotBeNull("a source marker that cannot fit must not leave painted pixels over the title");
            marker.Clip!.Bounds.Should().Be(default(Rect));
        }
    }

    [AvaloniaTest]
    [TestCase(false, 25)]
    [TestCase(false, 120)]
    [TestCase(true, 25)]
    [TestCase(true, 120)]
    public void Bisect_marker_should_share_the_source_four_pixel_advance(bool bad, int width)
    {
        using RowFixture fixture = new(9, "subject", body: null);
        IGitRef bisect = fixture.CreateRef(bad ? "refs/bisect/bad" : "refs/bisect/good");
        bisect.IsBisect.Returns(true);
        bisect.IsBisectBad.Returns(bad);
        bisect.IsBisectGood.Returns(!bad);
        fixture.Revision.Refs = [bisect];
        fixture.Refresh();
        fixture.SetWidth(width);

        Origin(fixture.Subject, fixture.Cell).X.Should().Be(6 + 16 + 4);
        Image marker = fixture.Cell.GetVisualDescendants().OfType<Image>()
            .Single(image => image.Classes.Contains("revision-bisect-marker"));
        marker.Clip.Should().BeNull("a 16-DIP bisect image fits the source minimum 25-DIP column");
        marker.Bounds.Width.Should().Be(16);
        fixture.AssertImagePainted(marker);
    }

    [AvaloniaTest]
    [Combinatorial]
    public void Markers_should_use_source_integer_centers_through_live_font_and_revision_changes(
        [Values("good", "bad", "fixup", "squash", "amend")] string kind,
        [Values("light", "dark", "custom")] string theme,
        [Values(false, true)] bool selected)
    {
        using RowFixture fixture = new(9, "subject", body: null, theme: theme, defaultStarColumn: true);
        fixture.Grid.GetTestAccessor().Revisions.SelectedIndex = selected ? 0 : -1;
        fixture.SetMarker(kind);
        object? itemsSource = fixture.Grid.GetTestAccessor().Revisions.ItemsSource;
        foreach ((string family, int nativeHeight) in new[] { ("Segoe UI", 26), ("Arial", 23), ("Consolas", 24) })
        {
            fixture.SetFont(family);
            fixture.Provider.Column.Width.IsStar.Should().BeTrue();
            fixture.Cell.Bounds.Height.Should().Be(OperatingSystem.IsWindows()
                ? nativeHeight
                : RevisionGridControl.GetRowHeight(fixture.Grid));
            fixture.Subject.FontFamily.Name.Should().Be(family);
            fixture.Grid.GetTestAccessor().Revisions.SelectedIndex.Should().Be(selected ? 0 : -1);
            fixture.Grid.GetTestAccessor().Revisions.ItemsSource.Should().BeSameAs(itemsSource);
            fixture.AssertMarkerPlacement();

            fixture.Revision.Body = fixture.Revision.Subject + "\n\nbody loaded on the same revision";
            fixture.Revision.HasMultiLineMessage = true;
            fixture.Refresh();
            fixture.AssertMarkerPlacement();
            fixture.Revision.Body = null;
            fixture.Refresh();
            fixture.AssertMarkerPlacement();
        }

        fixture.Revision.Subject = "ordinary subject";
        fixture.Revision.Refs = [];
        fixture.Refresh();
        fixture.Cell.GetVisualDescendants().OfType<Image>().Should().NotContain(image => image.IsVisible
            && (image.Classes.Contains("revision-message-marker") || image.Classes.Contains("revision-bisect-marker")));
        fixture.SetMarker(kind);
        fixture.AssertMarkerPlacement();
    }

    [AvaloniaTest]
    [Combinatorial]
    public void Under_height_marker_cells_should_preserve_source_truncation_and_clip_the_original_bitmap(
        [Values("good", "bad", "fixup", "squash", "amend")] string kind,
        [Values(13, 14, 15, 16)] int height)
    {
        using RowFixture fixture = new(9, "subject", body: null, defaultStarColumn: true);
        fixture.SetMarker(kind);
        fixture.Cell.Height = height;
        fixture.Cell.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        fixture.Refresh();

        fixture.Cell.Bounds.Height.Should().Be(height);
        fixture.AssertMarkerPlacement();
    }

    [AvaloniaTest]
    [TestCase(9)]
    [TestCase(11)]
    public void Ref_capsules_should_leave_only_their_finite_remaining_span_for_subject_and_body(int points)
    {
        using RowFixture fixture = new(points, "subject", new string('W', 100));
        IGitRef branch = fixture.CreateRef("refs/heads/long-local-branch-name");
        branch.IsHead.Returns(true);
        fixture.Revision.Refs = [branch];
        fixture.Refresh();
        fixture.SetWidth(100);

        RevisionGridRefRenderer.RefLabelControl label = fixture.Cell.GetVisualDescendants()
            .OfType<RevisionGridRefRenderer.RefLabelControl>().Single();
        label.CapsuleBounds.Width.Should().Be(100 - 26 - 6);
        fixture.Subject.Bounds.Width.Should().Be(0);
        fixture.Body.Bounds.Width.Should().Be(0);
        fixture.AssertIndicatorNotPainted();

        fixture.SetWidth(500);
        label = fixture.Cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>().Single();
        double sourceTitleX = 6 + label.CapsuleBounds.Width + 5;
        Origin(fixture.Subject, fixture.Cell).X.Should().Be(sourceTitleX);
        Origin(fixture.Body, fixture.Cell).X.Should().Be(sourceTitleX + SourceWidth(fixture.Subject));
        fixture.AssertIndicatorPainted();
        Point inside = label.TranslatePoint(label.CapsuleBounds.Center, fixture.Grid)!.Value;
        fixture.Provider.HitTest(0, inside).Should().BeSameAs(label);
    }

    [AvaloniaTest]
    [Combinatorial]
    public void Finite_body_should_preserve_live_source_theme_and_selected_focus_colors(
        [Values("light", "dark", "custom")] string theme,
        [Values(9, 11)] int points,
        [Values(false, true)] bool focused)
    {
        using RowFixture fixture = new(points, "subject", new string('W', 100), theme: theme);
        fixture.SetWidth(250);
        fixture.SetFocus(focused);

        fixture.AssertBrush(fixture.Subject.Foreground, focused
            ? "GitExtensionsRevisionSelectedSubjectBrush"
            : "GitExtensionsKnownColorControlTextBrush");
        fixture.AssertBrush(fixture.Body.Foreground, "GitExtensionsRevisionSelectedBodyBrush");
        fixture.Body.TextLayout.TextLines.Should().Contain(line => line.HasCollapsed);
        fixture.AssertIndicatorPainted();
    }

    [AvaloniaTest]
    [TestCase(9, false)]
    [TestCase(11, false)]
    [TestCase(9, true)]
    [TestCase(11, true)]
    public void Initially_unloaded_default_star_rows_should_keep_the_full_source_paint_span_through_detail_loading(
        int points, bool selected)
    {
        const string subject = "fix: align main-window viewport and popup rendering";
        using RowFixture fixture = new(points, subject, body: null,
            defaultStarColumn: true, initiallyMultiline: true);
        if (selected)
        {
            FieldInfo head = typeof(RevisionGridControl).GetField("_headId", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertionException("The existing current-checkout state must remain available to the renderer fixture.");
            head.SetValue(fixture.Grid, fixture.Revision.ObjectId);
            fixture.Grid.IsCurrentCheckout(fixture.Revision).Should().BeTrue();
        }

        fixture.Grid.GetTestAccessor().Revisions.SelectedIndex = selected ? 0 : -1;
        fixture.Refresh();
        fixture.Provider.Column.Width.IsStar.Should().BeTrue();
        fixture.Body.Text.Should().BeEmpty();
        AssertTitlePaintSpan("initial subject without details");
        double initialWidth = fixture.Cell.Bounds.Width;

        fixture.Revision.Body = subject + "\n\n" + new string('W', 200);
        fixture.Refresh();
        AssertTitlePaintSpan("details loaded on the same revision");
        Origin(fixture.Body, fixture.Cell).X.Should().Be(6 + SourceWidth(fixture.Subject));
        fixture.Body.Bounds.Width.Should().Be(fixture.Cell.Bounds.Width - 26 - 6 - SourceWidth(fixture.Subject));
        fixture.Body.TextLayout.TextLines.Should().Contain(line => line.HasCollapsed);

        fixture.Window.Width += 80;
        fixture.Refresh();
        fixture.Cell.Bounds.Width.Should().BeGreaterThan(initialWidth);
        AssertTitlePaintSpan("default star column after viewport growth");

        fixture.Revision.Body = null;
        fixture.Refresh();
        fixture.Body.Text.Should().BeEmpty();
        fixture.Body.Bounds.Width.Should().Be(0);
        AssertTitlePaintSpan("details retired without replacing the revision");

        void AssertTitlePaintSpan(string phase)
        {
            TestContext.Out.WriteLine($"{phase}: cell={fixture.Cell.Bounds}; subject={fixture.Subject.Bounds}; "
                + $"family={fixture.Subject.FontFamily}; size={fixture.Subject.FontSize}; "
                + $"style={fixture.Subject.FontStyle}; weight={fixture.Subject.FontWeight}; "
                + $"nativeAdvance={SourceWidth(fixture.Subject)}");
            fixture.Subject.FontFamily.Name.Should().Be("Segoe UI");
            fixture.Subject.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(points));
            fixture.Subject.FontWeight.Should().Be(selected ? FontWeight.Bold : FontWeight.Normal);
            fixture.Subject.Text.Should().Be(subject);
            Origin(fixture.Subject, fixture.Cell).X.Should().Be(6);
            fixture.Subject.Bounds.Width.Should().Be(fixture.Cell.Bounds.Width - 26 - 6,
                "DrawColumnText receives the full remaining cell rectangle, not the measured subject advance");
            fixture.Subject.Bounds.Width.Should().BeGreaterThan(SourceWidth(fixture.Subject));
            fixture.Subject.TextLayout.TextLines.Should().NotContain(line => line.HasCollapsed);
            fixture.AssertIndicatorPainted();
        }
    }

    private static Point Origin(Control control, Visual relativeTo)
        => control.TranslatePoint(default, relativeTo)
            ?? throw new AssertionException("The actual message renderer must remain attached to its row.");

    private static int SourceWidth(TextBlock text)
        => (int)Math.Ceiling(WinFormsTextMeasurer.MeasureSize(text, text.Text ?? string.Empty).Width);

    private sealed class RowFixture : IDisposable
    {
        private readonly Font _font = AppSettings.Font;
        private readonly bool _showBody = AppSettings.ShowCommitBodyInRevisionGrid;
        private readonly bool _notesColumn = AppSettings.ShowGitNotesColumn.Value;
        private readonly bool _avatars = AppSettings.ShowAuthorAvatarColumn;
        private readonly bool _nonRelatives = AppSettings.RevisionGraphDrawNonRelativesTextGray;
        private readonly ThemeSettings _themeSettings = ThemeModule.Settings;
        private readonly Dictionary<ThemeVariant, IThemeVariantProvider?> _themeResources = [];
        private readonly IGitModule _module = Substitute.For<IGitModule>();
        private readonly Button _focusTarget = new() { Content = "Move focus outside the grid" };

        public RowFixture(int points, string subject, string? body, bool showBody = true, string theme = "light",
            bool defaultStarColumn = false, bool initiallyMultiline = false)
        {
            AppSettings.Font = new Font("Segoe UI", points);
            AppSettings.ShowCommitBodyInRevisionGrid = showBody;
            AppSettings.ShowGitNotesColumn.Value = false;
            AppSettings.ShowAuthorAvatarColumn = false;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = false;
            AvaloniaFontSettings.ApplyAppSettings();
            Application application = Application.Current!;
            foreach (ThemeVariant variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                application.Resources.ThemeDictionaries.TryGetValue(variant, out IThemeVariantProvider? original);
                _themeResources.Add(variant, original);
                application.Resources.ThemeDictionaries[variant] = new ResourceDictionary();
            }

            bool dark = theme == "dark";
            Dictionary<AppColor, DrawingColor> colors = new()
            {
                [AppColor.PanelBackground] = dark ? DrawingColor.FromArgb(32, 32, 32) : DrawingColor.White,
            };
            Dictionary<KnownColor, DrawingColor> systemColors = [];
            if (theme == "custom")
            {
                systemColors[KnownColor.WindowText] = DrawingColor.FromArgb(39, 55, 24);
                systemColors[KnownColor.Highlight] = DrawingColor.FromArgb(34, 104, 55);
                systemColors[KnownColor.HighlightText] = DrawingColor.White;
            }

            Theme palette = new(colors, systemColors, new ThemeId("message-layout-" + theme));
            AvaloniaThemeResources.Apply(application,
                new ThemeSettings(palette, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true));
            _module.GitVersion.Returns(Substitute.For<IGitVersion>());
            _module.GetSelectedBranch().Returns("current");
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(_module);
            commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
            commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
            IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
            source.UICommands.Returns(commands);
            Grid = new RevisionGridControl { UICommandsSource = source };
            Provider = Grid.ColumnProviders.OfType<MessageColumnProvider>().Single();
            if (!defaultStarColumn)
            {
                Provider.Column.Width = new GridLength(500);
            }

            Revision = new GitRevision(ObjectId.Parse("abcdef1234567890abcdef1234567890abcdef12"))
            {
                Subject = subject,
                Body = body is null ? null : subject + "\n\n" + body,
                HasMultiLineMessage = initiallyMultiline || body is not null,
                Author = "Author",
            };
            Grid.GetTestAccessor().SetRevisions([Revision]);
            DockPanel.SetDock(_focusTarget, Dock.Top);
            Window = new Window
            {
                Width = 1400,
                Height = 200,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
                Content = new DockPanel { Children = { _focusTarget, Grid } },
            };
            Window.Show();
            Grid.GetTestAccessor().Revisions.SelectedIndex = 0;
            SetFocus(true);
        }

        public RevisionGridControl Grid { get; }

        public MessageColumnProvider Provider { get; }

        public GitRevision Revision { get; }

        public Window Window { get; }

        public Control Cell => Grid.GetVisualDescendants().OfType<Control>()
            .Single(control => control.Classes.Contains("revision-message-cell"));

        public TextBlock Subject => Cell.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Classes.Contains("revision-subject"));

        public TextBlock Body => Cell.GetVisualDescendants().OfType<TextBlock>()
            .Single(text => text.Classes.Contains("revision-body"));

        public MultilineIndicator Indicator => Cell.GetVisualDescendants().OfType<MultilineIndicator>().Single();

        public IGitRef CreateRef(string completeName)
        {
            IGitRef gitRef = Substitute.For<IGitRef>();
            gitRef.Module.Returns(_module);
            gitRef.ObjectId.Returns(Revision.ObjectId);
            gitRef.CompleteName.Returns(completeName);
            gitRef.Name.Returns(completeName[(completeName.LastIndexOf('/') + 1)..]);
            return gitRef;
        }

        public void Refresh()
        {
            Grid.RefreshRealizedRows();
            Layout();
        }

        public void SetFont(string family)
        {
            AppSettings.Font = new Font(family, 9);
            AvaloniaFontSettings.ApplyAppSettings();
            Refresh();
        }

        public void SetMarker(string kind)
        {
            bool bisect = kind is "good" or "bad";
            Revision.Subject = bisect ? "subject" : kind + "! subject";
            if (bisect)
            {
                IGitRef gitRef = CreateRef("refs/bisect/" + kind);
                gitRef.IsBisect.Returns(true);
                gitRef.IsBisectGood.Returns(kind == "good");
                gitRef.IsBisectBad.Returns(kind == "bad");
                Revision.Refs = [gitRef];
            }
            else
            {
                Revision.Refs = [];
            }

            Refresh();
        }

        public void SetWidth(int width)
        {
            Provider.Column.Width = new GridLength(width);
            Grid.ApplyColumnSettings();
            Layout();
        }

        public void SetFocus(bool focused)
        {
            if (focused)
            {
                Grid.GetTestAccessor().Revisions.Focus();
            }
            else
            {
                _focusTarget.Focus();
            }

            Refresh();
            Grid.GetTestAccessor().Revisions.IsKeyboardFocusWithin.Should().Be(focused);
        }

        public void AssertBrush(IBrush? brush, string key)
        {
            Window.TryFindResource(key, Window.ActualThemeVariant, out object? resource).Should().BeTrue();
            ((ISolidColorBrush)brush!).Color.Should().Be(((ISolidColorBrush)resource!).Color);
        }

        public void AssertIndicatorPainted()
        {
            Indicator.IsVisible.Should().BeTrue();
            Point origin = Origin(Indicator, Cell);
            origin.X.Should().Be(Cell.Bounds.Width - 26);
            (origin.X + Indicator.Bounds.Width).Should().Be(Cell.Bounds.Width);
            using SKBitmap pixels = Capture();
            Point backgroundPoint = IndicatorPoint(new Point(3, 2));
            Point fillPoint = IndicatorPoint(new Point(5, 2));
            Point dotPoint = IndicatorPoint(new Point(9, 6));
            SKColor background = Sample(pixels, backgroundPoint);
            SKColor fill = Sample(pixels, fillPoint);
            SKColor dot = Sample(pixels, dotPoint);
            SaveIndicatorDiagnostic(pixels, backgroundPoint, fillPoint, dotPoint, background, fill, dot);
            fill.Should().NotBe(background, "the native indicator rectangle must actually paint inside its reserved span");
            dot.Should().NotBe(fill, "the source three-dot foreground must paint, not merely an IsVisible control");
            Visual? hit = Window.InputHitTest(dotPoint) as Visual;
            hit.Should().NotBeNull();
            hit!.GetSelfAndVisualAncestors().Should().Contain(Cell,
                "the indicator remains noninteractive and its pixels belong to the real message cell");
        }

        private Point IndicatorPoint(Point localPoint)
            => Indicator.TranslatePoint(localPoint, Window)
                ?? throw new AssertionException("The indicator sample must use its actual visual transform into the window.");

        private static SKColor Sample(SKBitmap pixels, Point point)
            => pixels.GetPixel((int)Math.Floor(point.X), (int)Math.Floor(point.Y));

        private void SaveIndicatorDiagnostic(SKBitmap pixels, Point backgroundPoint, Point fillPoint, Point dotPoint,
            SKColor background, SKColor fill, SKColor dot)
        {
            string? requestedRoot = Environment.GetEnvironmentVariable("GITEXTENSIONS_MESSAGE_LAYOUT_EVIDENCE");
            if (string.IsNullOrEmpty(requestedRoot) && background != fill && fill != dot)
            {
                return;
            }

            string directory = Path.Combine(requestedRoot ?? Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "RevisionGridMessageLayoutProbe"), TestContext.CurrentContext.Test.ID + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string imagePath = Path.Combine(directory, "owner.png");
            using (SKData image = pixels.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.Create(imagePath))
            {
                image.SaveTo(stream);
            }

            Dictionary<string, string?> palette = [];
            foreach (string key in new[]
            {
                "GitExtensionsWindowTextBrush", "GitExtensionsDataGridViewSelectionBackgroundBrush",
                "GitExtensionsInactiveSelectionBackgroundBrush", "GitExtensionsControlForegroundBrush",
                "GitExtensionsKnownColorControlTextBrush", "GitExtensionsRevisionSelectedBodyBrush",
            })
            {
                palette[key] = Indicator.TryFindResource(key, Indicator.ActualThemeVariant, out object? value)
                    ? (value as ISolidColorBrush)?.Color.ToString() ?? value?.ToString()
                    : null;
            }

            object diagnostic = new
            {
                test = TestContext.CurrentContext.Test.Name,
                theme = Window.ActualThemeVariant?.ToString(),
                systemColorMode = ThemeModule.Settings.Theme.SystemColorMode.ToString(),
                Grid.GetTestAccessor().Revisions.SelectedIndex,
                Grid.GetTestAccessor().Revisions.IsKeyboardFocusWithin,
                palette,
                samples = new
                {
                    backgroundPoint = backgroundPoint.ToString(),
                    fillPoint = fillPoint.ToString(),
                    dotPoint = dotPoint.ToString(),
                    pixelSelection = "floor of each transformed interior coordinate",
                    background = background.ToString(),
                    fill = fill.ToString(),
                    dot = dot.ToString(),
                },
                cell = new { bounds = Cell.Bounds.ToString(), origin = Origin(Cell, Window).ToString() },
                indicator = new
                {
                    bounds = Indicator.Bounds.ToString(),
                    origin = Origin(Indicator, Window).ToString(),
                    Indicator.IsVisible,
                    canPaint = typeof(MultilineIndicator).GetField("_canPaint", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.GetValue(Indicator),
                },
                text = new
                {
                    subject = Subject.Text,
                    subjectBounds = Subject.Bounds.ToString(),
                    subjectOrigin = Origin(Subject, Cell).ToString(),
                    subjectSourceWidth = SourceWidth(Subject),
                    bodyBounds = Body.Bounds.ToString(),
                    bodyOrigin = Origin(Body, Cell).ToString(),
                },
                ancestors = Indicator.GetSelfAndVisualAncestors().Select(visual => new
                {
                    type = visual.GetType().FullName,
                    bounds = visual.Bounds.ToString(),
                    visual.ClipToBounds,
                    clip = visual.Clip?.Bounds.ToString(),
                    visual.IsVisible,
                    visual.Opacity,
                }),
                hit = Window.InputHitTest(dotPoint)?.GetType().FullName,
            };
            string diagnosticPath = Path.Combine(directory, "indicator.json");
            File.WriteAllText(diagnosticPath, JsonSerializer.Serialize(diagnostic, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(imagePath);
            TestContext.AddTestAttachment(diagnosticPath);
            TestContext.Out.WriteLine("Actual indicator diagnostic: " + diagnosticPath);
        }

        public void AssertIndicatorNotPainted()
        {
            using SKBitmap pixels = Capture();
            SKColor background = Sample(pixels, IndicatorPoint(new Point(3, 2)));
            Sample(pixels, IndicatorPoint(new Point(5, 2))).Should().Be(background);
            Sample(pixels, IndicatorPoint(new Point(9, 6))).Should().Be(background,
                "source DrawCommitMessage returns before indicator.Render when the subject consumed the message span");
        }

        public void AssertImagePainted(Image image)
        {
            using SKBitmap pixels = Capture();
            Point origin = Origin(image, Window);
            Point cellOrigin = Origin(Cell, Window);
            (origin.X + image.Bounds.Width).Should().BeLessThan(cellOrigin.X + Cell.Bounds.Width,
                "the strict source image-fit check leaves at least one pixel before the right edge");
            SKColor background = pixels.GetPixel((int)origin.X - 1, (int)origin.Y);
            bool hasInk = false;
            for (int y = (int)origin.Y; y < (int)(origin.Y + image.Bounds.Height); y++)
            {
                for (int x = (int)origin.X; x < (int)(origin.X + image.Bounds.Width); x++)
                {
                    hasInk |= pixels.GetPixel(x, y) != background;
                }
            }

            hasInk.Should().BeTrue("the actual attached marker must paint its source image, not only keep a layout slot");
        }

        public void AssertMarkerPlacement()
        {
            Image marker = Cell.GetVisualDescendants().OfType<Image>().Single(image => image.IsVisible
                && (image.Classes.Contains("revision-message-marker") || image.Classes.Contains("revision-bisect-marker")));
            AssertMarkerPixels(marker);
            marker.Bounds.Size.Should().Be(new Size(16, 16));
            marker.Clip.Should().BeNull();
            Cell.ClipToBounds.Should().BeTrue();
            Point origin = Origin(marker, Cell);
            origin.X.Should().Be(6);
            origin.Y.Should().Be(((int)Cell.Bounds.Height - (int)marker.Height) / 2,
                "DrawImage uses integer division, including truncation toward zero for a bitmap taller than the cell");
            Origin(Subject, Cell).X.Should().Be(6 + 16 + 4);
        }

        private void AssertMarkerPixels(Image marker)
        {
            using SKBitmap actual = Capture();
            Point cellOrigin = Origin(Cell, Window);
            Point imageOrigin = Origin(marker, Window);
            int left = (int)Math.Floor(imageOrigin.X) - 1;
            int top = (int)Math.Floor(Math.Min(cellOrigin.Y, imageOrigin.Y)) - 1;
            int width = (int)marker.Width + 2;
            int height = (int)Math.Ceiling(Math.Max(cellOrigin.Y + Cell.Bounds.Height,
                imageOrigin.Y + marker.Height)) - top + 1;
            using RenderTargetBitmap expectedFrame = new(new PixelSize(width, height), new Vector(96, 96));
            using (DrawingContext context = expectedFrame.CreateDrawingContext())
            {
                for (int y = 0; y < height; y++)
                {
                    SKColor backdrop = actual.GetPixel(left - 1, top + y);
                    context.FillRectangle(new SolidColorBrush(Color.FromArgb(backdrop.Alpha,
                        backdrop.Red, backdrop.Green, backdrop.Blue)), new Rect(0, y, width, 1));
                }

                using (context.PushClip(new Rect(cellOrigin.X - left, cellOrigin.Y - top,
                    Cell.Bounds.Width, Cell.Bounds.Height)))
                {
                    // This independent paint uses the source bitmap, cell clip and integer
                    // formula, preserving alpha without accepting framework half-DIP placement.
                    context.DrawImage(marker.Source ?? throw new AssertionException("The marker must retain its source bitmap."),
                        new Rect(cellOrigin.X + 6 - left,
                            cellOrigin.Y + (((int)Cell.Bounds.Height - (int)marker.Height) / 2) - top,
                            marker.Width, marker.Height));
                }
            }

            using MemoryStream stream = new();
            expectedFrame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            using SKBitmap expected = SKBitmap.Decode(stream)
                ?? throw new AssertionException("The independent source-shaped marker paint must decode.");
            int differences = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    differences += actual.GetPixel(left + x, top + y) == expected.GetPixel(x, y) ? 0 : 1;
                }
            }

            TestContext.Out.WriteLine($"marker={marker.Classes}; cell={Cell.Bounds}; local={Origin(marker, Cell)}; "
                + $"window={imageOrigin}; family={Subject.FontFamily}; differingPixels={differences}");
            Point localOrigin = Origin(marker, Cell);
            bool sourceGeometry = marker.Bounds.Size == new Size(16, 16) && marker.Clip is null && Cell.ClipToBounds
                && localOrigin.X == 6 && localOrigin.Y == ((int)Cell.Bounds.Height - (int)marker.Height) / 2
                && Origin(Subject, Cell).X == 6 + 16 + 4;
            SaveMarkerDiagnostic(actual, expected, marker, new Rect(left, top, width, height), differences, sourceGeometry);
            differences.Should().Be(0,
                "the attached image must preserve every source alpha pixel and the source cell clip at its integer center");
        }

        private void SaveMarkerDiagnostic(SKBitmap actual, SKBitmap expected, Image marker, Rect expectedRegion,
            int differences, bool sourceGeometry)
        {
            string? requestedRoot = Environment.GetEnvironmentVariable("GITEXT_MENU_PAINT_EVIDENCE");
            if (string.IsNullOrEmpty(requestedRoot) && differences == 0 && sourceGeometry)
            {
                return;
            }

            string directory = Path.Combine(requestedRoot ?? Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "RevisionGridMessageLayoutProbe"), "markers", TestContext.CurrentContext.Test.ID + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string actualPath = Path.Combine(directory, "actual-window.png");
            using (SKData image = actual.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.Create(actualPath))
            {
                image.SaveTo(stream);
            }

            using (SKData image = expected.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.Create(Path.Combine(directory, "expected-marker-region.png")))
            {
                image.SaveTo(stream);
            }

            Bitmap source = marker.Source as Bitmap
                ?? throw new AssertionException("The marker diagnostic must retain the actual shared source bitmap.");
            source.Save(Path.Combine(directory, "source-bitmap.png"), PngBitmapEncoderOptions.Default);
            object diagnostic = new
            {
                test = TestContext.CurrentContext.Test.Name,
                theme = Window.ActualThemeVariant?.ToString(),
                family = Subject.FontFamily.Name,
                Subject.FontSize,
                Grid.GetTestAccessor().Revisions.SelectedIndex,
                Revision.Subject,
                refs = Revision.Refs.Select(gitRef => gitRef.CompleteName),
                cellBounds = Cell.Bounds.ToString(),
                cellOriginInWindow = Origin(Cell, Window).ToString(),
                markerBounds = marker.Bounds.ToString(),
                markerOriginInCell = Origin(marker, Cell).ToString(),
                markerOriginInWindow = Origin(marker, Window).ToString(),
                Cell.ClipToBounds,
                clip = marker.Clip?.Bounds.ToString(),
                sourcePixelSize = source.PixelSize.ToString(),
                sourceDpi = source.Dpi.ToString(),
                expectedRegionInActualWindow = new { expectedRegion.X, expectedRegion.Y, expectedRegion.Width, expectedRegion.Height },
                differences,
                sourceGeometry,
                captureMethod = "actualHeadlessWindowAndIndependentDrawingContextSourceBitmapIntegerCenterWithCellClip",
                scope = "Exact same-backend marker placement/alpha/clip check; not GDI-versus-Skia raster parity or desktop evidence.",
            };
            string diagnosticPath = Path.Combine(directory, "marker.json");
            File.WriteAllText(diagnosticPath, JsonSerializer.Serialize(diagnostic, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddTestAttachment(actualPath);
            TestContext.AddTestAttachment(diagnosticPath);
            TestContext.Out.WriteLine("Actual marker diagnostic: " + diagnosticPath);
        }

        private void Layout()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            using SKBitmap pixels = Capture();
        }

        private SKBitmap Capture()
        {
            using WriteableBitmap frame = Window.CaptureRenderedFrame()
                ?? throw new AssertionException("The actual revision row must render a headless frame.");
            using MemoryStream stream = new();
            frame.Save(stream, PngBitmapEncoderOptions.Default);
            stream.Position = 0;
            return SKBitmap.Decode(stream)
                ?? throw new AssertionException("The actual revision row frame must decode to pixels.");
        }

        public void Dispose()
        {
            Window.Close();
            Grid.CancelBackgroundTasks();
            foreach ((ThemeVariant variant, IThemeVariantProvider? resources) in _themeResources)
            {
                if (resources is null)
                {
                    Application.Current!.Resources.ThemeDictionaries.Remove(variant);
                }
                else
                {
                    Application.Current!.Resources.ThemeDictionaries[variant] = resources;
                }
            }

            ColorHelper.ThemeSettings = _themeSettings;
            AppSettings.Font = _font;
            AppSettings.ShowCommitBodyInRevisionGrid = _showBody;
            AppSettings.ShowGitNotesColumn.Value = _notesColumn;
            AppSettings.ShowAuthorAvatarColumn = _avatars;
            AppSettings.RevisionGraphDrawNonRelativesTextGray = _nonRelatives;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }
}
