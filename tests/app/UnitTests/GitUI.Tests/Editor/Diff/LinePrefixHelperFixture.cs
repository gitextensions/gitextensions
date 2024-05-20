using FluentAssertions;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor.Document;
using NSubstitute;

namespace GitUITests.Editor.Diff;

[TestFixture]
internal class LinePrefixHelperFixture
{
    [Test]
    public void CanFindAddedLines()
    {
        const string diffText = @"+added line 1
+added line 2
-removed line
-removed line";

        LineSegmentGetter lineSegmentGetter = PrepareLineSegmentGetter(diffText);

        IDocument doc = PreDocumentForDiffText(diffText);

        int beginIndex = 0;
        bool found = false;

        List<ISegment> lines = new LinePrefixHelper(lineSegmentGetter)
            .GetLinesStartingWith(doc, ref beginIndex, "+", ref found);

        lines.Count.Should().Be(2);
        beginIndex.Should().Be(2);
        found.Should().BeTrue();
    }

    [Test]
    public void CanFindDeletedLines()
    {
        const string diffText = @"+added line 1
+added line 2
-removed line1
-removed line2";

        LineSegmentGetter lineSegmentGetter = PrepareLineSegmentGetter(diffText);

        IDocument doc = PreDocumentForDiffText(diffText);

        int beginIndex = 0;
        bool found = false;

        List<ISegment> lines = new LinePrefixHelper(lineSegmentGetter)
            .GetLinesStartingWith(doc, ref beginIndex, "-", ref found);

        lines.Count.Should().Be(2);
        beginIndex.Should().Be(4);
        found.Should().BeTrue();
    }

    [TestCase("++ diffline", "++")]
    [TestCase("+  diffline", "+ ")]
    [TestCase(" + diffline", " +")]

    [TestCase("-- diffline", "--")]
    [TestCase("-  diffline", "- ")]
    [TestCase(" - diffline", " -")]

    [TestCase("+ diffline", "+")]
    [TestCase("- diffline", "-")]
    public void CanCheckIfTheLineStartsWithSpecificPrefix(string diffText, string prefix)
    {
        LineSegmentGetter lineSegmentGetter = PrepareLineSegmentGetter(diffText);

        IDocument doc = PreDocumentForDiffText(diffText);

        LinePrefixHelper helper = new(lineSegmentGetter);
        helper.DoesLineStartWith(doc, 0, prefix).Should().BeTrue();
    }

    [TestCase("+")]
    [TestCase("-")]
    public void GivenThatTheDocDoesNotHaveEnoughChars_ShouldReturnFalseWhenCheckPrefix(string diffText)
    {
        LineSegmentGetter lineSegmentGetter = PrepareLineSegmentGetter(diffText);

        IDocument doc = PreDocumentForDiffText(diffText);

        LinePrefixHelper helper = new(lineSegmentGetter);
        helper.DoesLineStartWith(doc, 0, "++").Should().BeFalse();
    }

    private static IDocument PreDocumentForDiffText(string diffText)
    {
        IDocument doc = Substitute.For<IDocument>();
        doc.GetCharAt(Arg.Any<int>()).Returns(args => diffText[(int)args[0]]);
        doc.TotalNumberOfLines.Returns(diffText.Split('\n').Length);
        doc.TextLength.Returns(diffText.Length);
        return doc;
    }

    private static LineSegmentGetter PrepareLineSegmentGetter(string diffText)
    {
        List<ISegment> lineSegments = GetSegmentsForDiffText(diffText);
        LineSegmentGetter lineSegmentGetter = Substitute.For<LineSegmentGetter>();
        lineSegmentGetter.GetSegment(Arg.Any<IDocument>(), Arg.Any<int>())
            .Returns(args => lineSegments[(int)args[1]]);
        return lineSegmentGetter;
    }

    private static List<ISegment> GetSegmentsForDiffText(string diffText)
    {
        List<ISegment> lineSegments = [];
        foreach (string diffLine in diffText.Split('\n'))
        {
            ISegment seg = Substitute.For<ISegment>();
            seg.Length.Returns(diffLine.Length);
            if (!lineSegments.Any())
            {
                seg.Offset = 0;
            }
            else
            {
                ISegment lastSegment = lineSegments[^1];
                seg.Offset = lastSegment.Offset + lastSegment.Length + 1;
            }

            lineSegments.Add(seg);
        }

        return lineSegments;
    }
}
