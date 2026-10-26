using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Compat;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using Font = GitExtensions.Shims.WinForms.Font;
using ShimFontStyle = GitExtensions.Shims.WinForms.FontStyle;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridFontMetricsTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(ConfiguredFonts))]
    public void GetRowHeight_should_use_the_configured_source_family_size_and_style(
        string family, float points, bool bold, bool italic, int nativeHeight)
    {
        Font original = AppSettings.Font;
        try
        {
            ShimFontStyle style = (bold ? ShimFontStyle.Bold : ShimFontStyle.Regular)
                | (italic ? ShimFontStyle.Italic : ShimFontStyle.Regular);
            AppSettings.Font = new Font(family, points, style);
            Button child = new() { FontFamily = new FontFamily("Consolas"), FontSize = 48, FontWeight = FontWeight.Bold };
            double actual = RevisionGridControl.GetRowHeight(child);
            if (OperatingSystem.IsWindows())
            {
                // Native original RevisionDataGridView probes cover all these actual
                // families/styles. They do not share Segoe UI's line/em-padding formula.
                actual.Should().Be(nativeHeight);
            }
            else
            {
                Size portable = WinFormsGraphicsTextMeasurer.MeasurePortableSize("By", new FontFamily(family),
                    italic ? FontStyle.Italic : FontStyle.Normal,
                    bold ? FontWeight.Bold : FontWeight.Normal,
                    AvaloniaFontSettings.ToDeviceIndependentPixels(points));
                actual.Should().Be(RevisionGridControl.CalculateRowHeight(portable.Height, 1));
            }
        }
        finally
        {
            AppSettings.Font = original;
        }
    }

    [AvaloniaTest]
    public void GetRowHeight_should_ignore_independently_styled_child_fonts_and_remeasure_changed_settings()
    {
        Font original = AppSettings.Font;
        try
        {
            AppSettings.Font = new Font("Arial", 9);
            Button child = new() { FontFamily = new FontFamily("Consolas"), FontSize = 32, FontStyle = FontStyle.Italic };
            double initial = RevisionGridControl.GetRowHeight(child);
            child.FontFamily = new FontFamily("Segoe UI");
            child.FontSize = 60;
            child.FontWeight = FontWeight.Bold;
            RevisionGridControl.GetRowHeight(child).Should().Be(initial);
            AppSettings.Font = new Font("Consolas", 22, ShimFontStyle.Bold | ShimFontStyle.Italic);
            double changed = RevisionGridControl.GetRowHeight(child);
            changed.Should().BeGreaterThan(initial);
            AppSettings.Font = new Font("Arial", 9);
            RevisionGridControl.GetRowHeight(child).Should().Be(initial);
        }
        finally
        {
            AppSettings.Font = original;
        }
    }

    [AvaloniaTest]
    [TestCase("Arial", false, false)]
    [TestCase("Arial", true, true)]
    [TestCase("Consolas", false, true)]
    [TestCase("Segoe UI", true, false)]
    public void Portable_measurement_should_use_actual_framework_shaping_without_native_metric_constants(
        string family, bool bold, bool italic)
    {
        FontFamily fontFamily = new(family);
        FontStyle style = italic ? FontStyle.Italic : FontStyle.Normal;
        FontWeight weight = bold ? FontWeight.Bold : FontWeight.Normal;
        const double fontSize = 24;
        FormattedText expected = new("By", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(fontFamily, style, weight), fontSize, foreground: null);
        Size actual = WinFormsGraphicsTextMeasurer.MeasurePortableSize("By", fontFamily, style, weight, fontSize);
        actual.Width.Should().Be(expected.Width);
        actual.Height.Should().Be(expected.Height);
        if (!OperatingSystem.IsWindows())
        {
            WinFormsGraphicsTextMeasurer.MeasureSize("By", fontFamily, style, weight, fontSize).Should().Be(actual);
        }
    }

    [AvaloniaTest]
    public void Published_font_changes_should_resize_realized_rows_and_their_avatar_column_without_replacing_selection()
    {
        Font originalFont = AppSettings.Font;
        bool originalShowAvatars = AppSettings.ShowAuthorAvatarColumn;
        Window? window = null;
        try
        {
            AppSettings.ShowAuthorAvatarColumn = true;
            AppSettings.Font = new Font("Segoe UI", 9);
            AvaloniaFontSettings.ApplyAppSettings();
            RevisionGridControl control = new() { UICommandsSource = CreateCommandsSource() };
            GitRevision revision = new(ObjectId.WorkTreeId) { Subject = "Working directory" };
            ListBox list = control.GetTestAccessor().Revisions;
            control.GetTestAccessor().SetRevisions([revision]);
            window = new Window { Width = 900, Height = 240, Content = control };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Grid row = GetRow(list);
            AvatarColumnProvider avatar = control.ColumnProviders.OfType<AvatarColumnProvider>().Single();
            double initialHeight = row.Bounds.Height;
            initialHeight.Should().Be(RevisionGridControl.GetRowHeight(control));
            avatar.Column.Width.Value.Should().Be(initialHeight);
            list.SelectedItem = revision;
            object? itemsSource = list.ItemsSource;

            AppSettings.Font = new Font("Arial", 22, ShimFontStyle.Bold | ShimFontStyle.Italic);
            AvaloniaFontSettings.ApplyAppSettings();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            Grid changed = GetRow(list);
            changed.Bounds.Height.Should().BeGreaterThan(initialHeight);
            changed.Bounds.Height.Should().Be(RevisionGridControl.GetRowHeight(control));
            avatar.Column.Width.Value.Should().Be(changed.Bounds.Height);
            changed.ColumnDefinitions[avatar.Index].Width.Value.Should().Be(changed.Bounds.Height);
            list.SelectedItem.Should().BeSameAs(revision);
            list.ItemsSource.Should().BeSameAs(itemsSource);
        }
        finally
        {
            window?.Close();
            AppSettings.Font = originalFont;
            AppSettings.ShowAuthorAvatarColumn = originalShowAvatars;
            AvaloniaFontSettings.ApplyAppSettings();
        }
    }

    [AvaloniaTest]
    public void Graphics_measurement_should_preserve_empty_text_and_cached_float_metrics()
    {
        Button owner = new() { FontFamily = new FontFamily("Arial"), FontSize = 12 };
        WinFormsGraphicsTextMeasurer.MeasureSize(owner, string.Empty).Should().Be(default(Size));
        Size initial = WinFormsGraphicsTextMeasurer.MeasureSize(owner, "By");
        WinFormsGraphicsTextMeasurer.MeasureSize(owner, "By").Should().Be(initial);
        WinFormsGraphicsTextMeasurer.ClearCache();
        WinFormsGraphicsTextMeasurer.MeasureSize(owner, "By").Should().Be(initial);
        owner.FontSize = 24;
        Size enlarged = WinFormsGraphicsTextMeasurer.MeasureSize(owner, "By");
        enlarged.Height.Should().BeGreaterThan(initial.Height);
        enlarged.Width.Should().BeGreaterThan(initial.Width);
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(CellStyles))]
    public void Author_cell_should_keep_the_configured_normal_style_and_replace_it_for_emphasis(
        bool bold, bool italic, bool emphasized)
    {
        Font original = AppSettings.Font;
        try
        {
            AppSettings.Font = CreateFont(bold, italic);
            RevisionGridControl grid = new();
            AuthorRevisionHighlighting highlighting = new();
            GitRevision revision = new(ObjectId.Random()) { Author = "Author", AuthorEmail = "author@example.invalid" };
            if (emphasized)
            {
                highlighting.ProcessRevisionSelectionChange(Substitute.For<IGitModule>(), [revision]).Should().BeTrue();
            }

            AuthorNameColumnProvider provider = new(grid, highlighting);
            TextBlock cell = (TextBlock)provider.CreateCell();
            provider.UpdateCell(cell, revision);
            cell.FontWeight.Should().Be(emphasized || bold ? FontWeight.Bold : FontWeight.Normal);
            cell.FontStyle.Should().Be(!emphasized && italic ? FontStyle.Italic : FontStyle.Normal);
        }
        finally
        {
            AppSettings.Font = original;
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(CellStyles))]
    public void Subject_and_body_should_share_the_source_normal_or_style_replacing_bold_font(
        bool bold, bool italic, bool emphasized)
    {
        Font original = AppSettings.Font;
        bool originalShowBody = AppSettings.ShowCommitBodyInRevisionGrid;
        try
        {
            AppSettings.Font = CreateFont(bold, italic);
            AppSettings.ShowCommitBodyInRevisionGrid = true;
            RevisionGridControl grid = new();
            GitRevision revision = new(ObjectId.Random()) { Subject = "subject", Body = "subject\n\nbody", HasMultiLineMessage = true };
            if (emphasized)
            {
                typeof(RevisionGridControl).GetField("_headId", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(grid, revision.ObjectId);
                grid.IsCurrentCheckout(revision).Should().BeTrue();
            }

            MessageColumnProvider provider = grid.ColumnProviders.OfType<MessageColumnProvider>().Single();
            Control cell = provider.CreateCell();
            provider.UpdateCell(cell, revision);
            TextBlock subject = cell.GetVisualDescendants().OfType<TextBlock>().Single(block => block.Classes.Contains("revision-subject"));
            TextBlock body = cell.GetVisualDescendants().OfType<TextBlock>().Single(block => block.Classes.Contains("revision-body"));
            subject.Text.Should().NotBeNullOrEmpty();
            body.Text.Should().NotBeNullOrEmpty();
            subject.FontWeight.Should().Be(emphasized || bold ? FontWeight.Bold : FontWeight.Normal);
            subject.FontStyle.Should().Be(!emphasized && italic ? FontStyle.Italic : FontStyle.Normal);
            body.FontWeight.Should().Be(subject.FontWeight);
            body.FontStyle.Should().Be(subject.FontStyle);
        }
        finally
        {
            AppSettings.Font = original;
            AppSettings.ShowCommitBodyInRevisionGrid = originalShowBody;
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(CellStyles))]
    public void Ref_capsules_should_keep_normal_styles_but_replace_italic_on_selected_refs(
        bool bold, bool italic, bool emphasized)
    {
        Font original = AppSettings.Font;
        try
        {
            AppSettings.Font = CreateFont(bold, italic);
            GitRef gitRef = new(Substitute.For<IGitModule>(), ObjectId.Random(), "refs/heads/branch") { IsSelected = emphasized };
            RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(gitRef, "branch", RefLabelShape.Rect, fill: false);
            label.FontWeight.Should().Be(emphasized || bold ? FontWeight.Bold : FontWeight.Normal);
            label.FontStyle.Should().Be(!emphasized && italic ? FontStyle.Italic : FontStyle.Normal);
        }
        finally
        {
            AppSettings.Font = original;
        }
    }

    [AvaloniaTest]
    public void Explicit_normal_ref_font_should_keep_the_configured_bold_italic_style()
    {
        Font original = AppSettings.Font;
        try
        {
            AppSettings.Font = CreateFont(bold: true, italic: true);
            GitRef gitRef = new(Substitute.For<IGitModule>(), ObjectId.Random(), "refs/heads/branch") { IsSelected = true };
            RevisionGridRefRenderer.RefLabelControl label = RevisionGridRefRenderer.CreateLabel(gitRef, "branch", RefLabelShape.Rect,
                fill: false, fontWeight: FontWeight.Normal);
            label.FontWeight.Should().Be(FontWeight.Bold);
            label.FontStyle.Should().Be(FontStyle.Italic);
        }
        finally
        {
            AppSettings.Font = original;
        }
    }

    private static IEnumerable<TestCaseData> ConfiguredFonts()
    {
        foreach ((string family, int[] heights) in new[]
        {
            ("Segoe UI", new[] { 26, 30, 43, 51 }),
            ("Arial", new[] { 23, 27, 38, 45 }),
            ("Consolas", new[] { 24, 28, 40, 47 }),
        })
        {
            float[] points = [9, 11, 18, 22];
            for (int index = 0; index < points.Length; index++)
            {
                foreach (bool bold in new[] { false, true })
                {
                    foreach (bool italic in new[] { false, true })
                    {
                        yield return new TestCaseData(family, points[index], bold, italic, heights[index]);
                    }
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> CellStyles()
    {
        foreach (bool bold in new[] { false, true })
        {
            foreach (bool italic in new[] { false, true })
            {
                foreach (bool emphasized in new[] { false, true })
                {
                    yield return new TestCaseData(bold, italic, emphasized);
                }
            }
        }
    }

    private static Font CreateFont(bool bold, bool italic)
        => new("Arial", 11, (bold ? ShimFontStyle.Bold : ShimFontStyle.Regular) | (italic ? ShimFontStyle.Italic : ShimFontStyle.Regular));

    private static IGitUICommandsSource CreateCommandsSource()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return source;
    }

    private static Grid GetRow(ListBox list)
        => list.GetVisualDescendants().OfType<Grid>().Single(row => row.Classes.Contains("revision-row"));
}
