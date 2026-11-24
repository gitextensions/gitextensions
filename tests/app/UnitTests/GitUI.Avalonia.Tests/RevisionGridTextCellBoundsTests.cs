using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
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
using Font = GitExtensions.Shims.WinForms.Font;

namespace GitExtensionsTests;

/// <summary>
///  Checks the original providers' full-cell paint bounds separately from column widths.
/// </summary>
[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class RevisionGridTextCellBoundsTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void Text_columns_should_preserve_the_source_provider_specific_insets()
    {
        RevisionGridControl grid = new();
        foreach (ColumnProvider provider in grid.ColumnProviders)
        {
            if (provider.CreateCell() is not TextBlock cell)
            {
                continue;
            }

            // Only the author painter reduces its bounds by ColumnLeftMargin. Date,
            // notes and commit ID draw in e.CellBounds with NoPadding, with no right inset.
            double left = provider is AuthorNameColumnProvider ? provider.ColumnLeftMargin : 0;
            cell.Margin.Should().Be(new Thickness(left, 0, 0, 0), provider.Name);
        }

        Control message = grid.ColumnProviders.OfType<MessageColumnProvider>().Single().CreateCell();
        TextBlock[] captions = message.GetVisualDescendants().OfType<TextBlock>()
            .Where(block => block.Classes.Contains("revision-subject") || block.Classes.Contains("revision-body"))
            .ToArray();
        captions.Should().HaveCount(2);
        captions.Should().OnlyContain(block => block.Margin == default(Thickness),
            "the source subject/body painters do not reduce the right edge of the remaining message bounds");
    }

    [AvaloniaTest]
    [TestCase(32, 8.25f)]
    [TestCase(55, 8.25f)]
    [TestCase(60, 8.25f)]
    [TestCase(130, 8.25f)]
    [TestCase(54, 10f)]
    [TestCase(60, 10f)]
    [TestCase(32, 12f)]
    [TestCase(55, 12f)]
    [TestCase(60, 12f)]
    [TestCase(130, 12f)]
    public void Commit_ID_should_choose_its_character_count_from_the_whole_source_column(int width, float points)
    {
        Font original = AppSettings.MonospaceFont;
        try
        {
            AppSettings.MonospaceFont = new Font("Consolas", points);
            RevisionGridControl grid = new();
            CommitIdColumnProvider provider = new(grid);
            provider.Column.Width = new GridLength(width);
            provider.OnColumnWidthChanged();
            TextBlock cell = (TextBlock)provider.CreateCell();
            GitRevision revision = new(ObjectId.Parse("0123456789abcdef0123456789abcdef01234567"));
            provider.UpdateCell(cell, revision);

            TextBlock sourceFont = new()
            {
                FontFamily = new FontFamily(AppSettings.MonospaceFont.Name),
                FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(points),
            };
            int expected = Enumerable.Range(0, ObjectId.Sha1CharCount + 1)
                .TakeWhile(length => WinFormsTextMeasurer.MeasureTextRenderer(sourceFont, new string('8', length)).Width <= width)
                .Last();

            cell.Text.Should().Be(revision.ObjectId.ToShortString(expected));
            cell.Margin.Should().Be(default(Thickness));
            cell.TextTrimming.Should().Be(TextTrimming.None);
        }
        finally
        {
            AppSettings.MonospaceFont = original;
        }
    }
}
