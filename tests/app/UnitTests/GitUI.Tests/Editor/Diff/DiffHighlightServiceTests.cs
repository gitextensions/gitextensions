using System.Collections.Immutable;
using AwesomeAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUITests.Editor.Diff;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class DiffHighlightServiceTests
{
    [Test]
    public void GetDifferenceMarkers_should_dim_identical_parts_at_begin_and_end()
    {
        const string identicalPartBefore = "identical_part_before_";
        const string identicalPartAfter = "_identical_part_after";
        const string differentRemoved = "RemovedX";
        const string differentAdded = "AddedY";
        const string removedLineText = $"-{identicalPartBefore}{differentRemoved}{identicalPartAfter}";
        const string addedLineText = $"+{identicalPartBefore}{differentAdded}{identicalPartAfter}";
        const string text = $"{removedLineText}\n{addedLineText}";
        ISegment removedLine = new Segment() { Offset = text.IndexOf(removedLineText), Length = removedLineText.Length };
        ISegment addedLine = new Segment() { Offset = text.IndexOf(addedLineText), Length = addedLineText.Length };
        const int beginOffset = 1;

        List<TextMarker> markers = [];
        DiffHighlightService.AddDifferenceMarkers(markers, GetText, removedLine, addedLine, beginOffset, dimBackground: true);
        IReadOnlyList<TextMarker> sortedMarkers = markers.ToImmutableSortedSet(new MarkerComparer());

        TextMarker[] expectedMarkers =
        [
            CreateDimmedMarker(removedLine, offset: 0, length: identicalPartBefore.Length),
            CreateDimmedMarker(removedLine, offset: identicalPartBefore.Length + differentRemoved.Length, length: identicalPartAfter.Length),
            CreateDimmedMarker(addedLine, offset: 0, length: identicalPartBefore.Length),
            CreateDimmedMarker(addedLine, offset: identicalPartBefore.Length + differentAdded.Length, length: identicalPartAfter.Length),
        ];
        sortedMarkers.Should().BeEquivalentTo(expectedMarkers);

        return;

        TextMarker CreateDimmedMarker(ISegment line, int offset, int length)
            => DiffHighlightServiceTests.CreateDimmedMarker(line.Offset + beginOffset + offset, length, isAdded: line == addedLine);

        string GetText(ISegment line) => (line == removedLine ? removedLineText : addedLineText)[beginOffset..];
    }

    [Test]
    public void GetDifferenceMarkers_should_add_anchor_markers()
    {
        const string deletion = nameof(deletion);
        const string insertion = nameof(insertion);
        const string identicalPartBefore = " identical_part_before ";
        const string identicalPartAfter = " identical_part_after ";
        const string differentRemoved = "RemovedX";
        const string differentAdded = "AddedY";
        const string removedLineText = $"-{deletion}{identicalPartBefore}{differentRemoved}{identicalPartAfter}";
        const string addedLineText = $"+{identicalPartBefore}{differentAdded}{identicalPartAfter}{insertion}";
        const string text = $"{removedLineText}\n{addedLineText}";
        ISegment removedLine = new Segment() { Offset = text.IndexOf(removedLineText), Length = removedLineText.Length };
        ISegment addedLine = new Segment() { Offset = text.IndexOf(addedLineText), Length = addedLineText.Length };
        const int beginOffset = 1;

        List<TextMarker> markers = [];
        DiffHighlightService.AddDifferenceMarkers(markers, GetText, removedLine, addedLine, beginOffset, dimBackground: true);
        IReadOnlyList<TextMarker> sortedMarkers = markers.ToImmutableSortedSet(new MarkerComparer());

        TextMarker[] expectedMarkers =
        [
            CreateDimmedMarker(removedLine, offset: deletion.Length, length: identicalPartBefore.Length),
            CreateDimmedMarker(removedLine, offset: deletion.Length + identicalPartBefore.Length + differentRemoved.Length, length: identicalPartAfter.Length),
            CreateAnchorMarker(removedLine, offset: removedLine.Length - 1),
            CreateAnchorMarker(addedLine, offset: 0),
            CreateDimmedMarker(addedLine, offset: 0, length: identicalPartBefore.Length),
            CreateDimmedMarker(addedLine, offset: identicalPartBefore.Length + differentAdded.Length, length: identicalPartAfter.Length),
        ];
        sortedMarkers.Should().BeEquivalentTo(expectedMarkers);

        return;

        TextMarker CreateAnchorMarker(ISegment line, int offset)
            => DiffHighlightServiceTests.CreateAnchorMarker(line.Offset + beginOffset + offset, isAdded: line == addedLine);

        TextMarker CreateDimmedMarker(ISegment line, int offset, int length)
            => DiffHighlightServiceTests.CreateDimmedMarker(line.Offset + beginOffset + offset, length, isAdded: line == addedLine);

        string GetText(ISegment line) => (line == removedLine ? removedLineText : addedLineText)[beginOffset..];
    }

    [Test]
    public async Task MarkInlineGap()
    {
        TextEditorControl textEditor = new();
        DiffViewerLineNumberControl diffViewerLineNumber = new(textEditor.ActiveTextAreaControl.TextArea);
        string testDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff");
        string text = File.ReadAllText(Path.Combine(testDataDir, "gaps.diff"));

        // ensure that the test doesn't fail if the local Git configuration has core.autocrlf enabled
        text = text.Replace("\r\n", "\n");

        _ = new PatchHighlightService(ref text, useGitColoring: true, diffViewerLineNumber);
        DiffLinesInfo result = diffViewerLineNumber.GetTestAccessor().Result;
        DiffLineInfo[] diffLines = [.. result.DiffLines.Values.OrderBy(l => l.LineNumInDiff)];

        int index = 0;
        List<(IReadOnlyList<ISegment> removed, IReadOnlyList<ISegment> added)> sections = [];
        while (index < diffLines.Length)
        {
            // git-diff presents the removed lines directly followed by the added in a "block"
            IReadOnlyList<ISegment> linesRemoved = DiffHighlightService.TestAccessor.GetBlockOfLines(diffLines, DiffLineType.Minus, ref index, found: false);
            if (linesRemoved.Count == 0)
            {
                continue;
            }

            IReadOnlyList<ISegment> linesAdded = DiffHighlightService.TestAccessor.GetBlockOfLines(diffLines, DiffLineType.Plus, ref index, found: true);
            if (linesAdded.Count == 0)
            {
                continue;
            }

            sections.Add((removed: linesRemoved, linesAdded));
        }

        await Verify(sections);
    }

    private static TextMarker CreateAnchorMarker(int offset, bool isAdded)
    {
        Color color = (isAdded ? AppColor.AnsiTerminalRedForeBold : AppColor.AnsiTerminalGreenForeBold).GetThemeColor();
        return new TextMarker(offset, length: 0, TextMarkerType.InterChar, color);
    }

    private static TextMarker CreateDimmedMarker(int offset, int length, bool isAdded)
    {
        Color color = (isAdded ? AppColor.AnsiTerminalGreenBackNormal : AppColor.AnsiTerminalRedBackNormal).GetThemeColor();
        Color dimmedColor = ColorHelper.DimColor(ColorHelper.DimColor(color));
        return new TextMarker(offset, length, TextMarkerType.SolidBlock, dimmedColor, ColorHelper.GetForeColorForBackColor(dimmedColor));
    }

    private sealed class MarkerComparer : IComparer<TextMarker>
    {
        public int Compare(TextMarker? left, TextMarker? right)
            => left.Offset < right.Offset ? -1
                : left.Offset > right.Offset ? 1
                : left.TextMarkerType == TextMarkerType.InterChar ? -1
                : right.TextMarkerType == TextMarkerType.InterChar ? 1
                : throw new InvalidOperationException("markers should not overlap");
    }
}
