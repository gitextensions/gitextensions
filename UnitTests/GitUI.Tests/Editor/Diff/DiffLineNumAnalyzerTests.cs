using FluentAssertions;
using GitUI;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;

namespace GitUITests.Editor.Diff;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class DiffLineNumAnalyzerTests
{
    private static readonly string TestDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff");
    private readonly string _sampleDiff;
    private readonly string _sampleCombinedDiff;
    private readonly TextEditorControl _textEditor;
    private readonly DiffLineNumAnalyzer _lineNumAnalyzer;

    public DiffLineNumAnalyzerTests()
    {
        // File copied from https://github.com/libgit2/libgit2sharp/pull/1034/files
        _sampleDiff = File.ReadAllText(Path.Combine(TestDataDir, "Sample.diff"));
        _sampleCombinedDiff = File.ReadAllText(Path.Combine(TestDataDir, "SampleCombined.diff"));
        _textEditor = new TextEditorControl();
        _lineNumAnalyzer = new DiffLineNumAnalyzer();
    }

    [TearDown]
    public void TearDown()
    {
        _textEditor.Dispose();
    }

    [Test]
    public void CanGetHeaders()
    {
        _textEditor.Text = _sampleDiff;
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: true);
        List<int> headerLines = [5, 17];
        foreach (int header in headerLines)
        {
            result.DiffLines[header].LineType.Should().Be(DiffLineType.Header);
            result.DiffLines[header].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
            result.DiffLines[header].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        }
    }

    [Test]
    public void CanGetContextLines()
    {
        _textEditor.Text = _sampleDiff;
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

        result.DiffLines[6].LineNumInDiff.Should().Be(6);
        result.DiffLines[6].LineType.Should().Be(DiffLineType.Context);
        result.DiffLines[6].LeftLineNumber.Should().Be(9);
        result.DiffLines[6].RightLineNumber.Should().Be(9);

        result.DiffLines[14].LineNumInDiff.Should().Be(14);
        result.DiffLines[14].LineType.Should().Be(DiffLineType.Context);
        result.DiffLines[14].LeftLineNumber.Should().Be(15);
        result.DiffLines[14].RightLineNumber.Should().Be(16);

        result.DiffLines[18].LineNumInDiff.Should().Be(18);
        result.DiffLines[18].LineType.Should().Be(DiffLineType.Context);
        result.DiffLines[18].LeftLineNumber.Should().Be(33);
        result.DiffLines[18].RightLineNumber.Should().Be(34);

        result.DiffLines[25].LineNumInDiff.Should().Be(25);
        result.DiffLines[25].LineType.Should().Be(DiffLineType.Context);
        result.DiffLines[25].LeftLineNumber.Should().Be(39);
        result.DiffLines[25].RightLineNumber.Should().Be(40);
    }

    [Test]
    public void CanGetMinusLines()
    {
        _textEditor.Text = _sampleDiff;
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

        result.DiffLines[9].LineNumInDiff.Should().Be(9);
        result.DiffLines[9].LineType.Should().Be(DiffLineType.Minus);
        result.DiffLines[9].LeftLineNumber.Should().Be(12);
        result.DiffLines[9].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);

        result.DiffLines[21].LineNumInDiff.Should().Be(21);
        result.DiffLines[21].LineType.Should().Be(DiffLineType.Minus);
        result.DiffLines[21].LeftLineNumber.Should().Be(36);
        result.DiffLines[21].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
    }

    [Test]
    public void CanGetPlusLines()
    {
        _textEditor.Text = _sampleDiff;
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

        result.DiffLines[12].LineNumInDiff.Should().Be(12);
        result.DiffLines[12].LineType.Should().Be(DiffLineType.Plus);
        result.DiffLines[12].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[12].RightLineNumber.Should().Be(14);

        result.DiffLines[13].LineNumInDiff.Should().Be(13);
        result.DiffLines[13].LineType.Should().Be(DiffLineType.Plus);
        result.DiffLines[13].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[13].RightLineNumber.Should().Be(15);

        result.DiffLines[22].LineNumInDiff.Should().Be(22);
        result.DiffLines[22].LineType.Should().Be(DiffLineType.Plus);
        result.DiffLines[22].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[22].RightLineNumber.Should().Be(37);
    }

    [Test]
    public void CanGetLineNumbersForCombinedDiff()
    {
        _textEditor.Text = _sampleCombinedDiff;
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: true);

        result.DiffLines[6].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[6].RightLineNumber.Should().Be(70);
        result.DiffLines[6].LineType.Should().Be(DiffLineType.Context);

        result.DiffLines[9].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[9].RightLineNumber.Should().Be(73);
        result.DiffLines[9].LineType.Should().Be(DiffLineType.Plus);

        result.DiffLines[19].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[19].RightLineNumber.Should().Be(83);
        result.DiffLines[19].LineType.Should().Be(DiffLineType.Plus);

        result.DiffLines[34].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[34].RightLineNumber.Should().Be(100);
        result.DiffLines[34].LineType.Should().Be(DiffLineType.Context);

        result.DiffLines[37].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[37].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[37].LineType.Should().Be(DiffLineType.Minus);

        result.DiffLines[38].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[38].RightLineNumber.Should().Be(103);
        result.DiffLines[38].LineType.Should().Be(DiffLineType.Plus);
    }

    [Test]
    public void CanGetGitWordDiffInfo()
    {
        DiffLinesInfo result = _lineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);
/* TBD
        result[12].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result[12].RightLineNumber.Should().Be(14);

        result[13].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result[13].RightLineNumber.Should().Be(15);

        result[22].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result[22].RightLineNumber.Should().Be(37);
*/
    }
}
