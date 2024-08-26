using System.Collections.Immutable;
using FluentAssertions;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUITests.Editor.Diff;

[TestFixture]
public class DiffHighlightServiceTests
{
    [Test]
    public void GetDifferenceMarkers_should_dim_identical_parts_at_begin_and_end()
    {
        // LineSegment is hard to create. Use TextMarker as implementation type of ISegment for this test.
        const TextMarkerType dontCare = TextMarkerType.SolidBlock;

        const string identicalPartBefore = "identical_part_before_";
        const string identicalPartAfter = "_identical_part_after";
        const string differentRemoved = "RemovedX";
        const string differentAdded = "AddedY";
        const string removedLineText = $"-{identicalPartBefore}{differentRemoved}{identicalPartAfter}";
        const string addedLineText = $"+{identicalPartBefore}{differentAdded}{identicalPartAfter}";
        const string text = $"{removedLineText}\n{addedLineText}";
        TextMarker removedLine = new(offset: text.IndexOf(removedLineText), removedLineText.Length, textMarkerType: dontCare);
        TextMarker addedLine = new(offset: text.IndexOf(addedLineText), addedLineText.Length, textMarkerType: dontCare);
        const int beginOffset = 1;

        IEnumerable<TextMarker> markers = DiffHighlightService.GetDifferenceMarkers(GetCharAt, removedLine, addedLine, beginOffset);
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

        char GetCharAt(int offset) => text[offset];
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
