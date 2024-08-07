using FluentAssertions;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.Editor.Diff;
using GitUI.Theming;
using ICSharpCode.TextEditor;

namespace GitUITests.Editor.Diff;

[Apartment(ApartmentState.STA)]
[TestFixture]
public class DiffLineNumAnalyzerTests
{
    private static readonly string _testDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff");
    private readonly string _sampleDiff;
    private readonly string _sampleCombinedDiff;
    private readonly string _sampleGitWordDiff;
    private readonly string _sampleDifftastic;
    private readonly TextEditorControl _textEditor;

    public DiffLineNumAnalyzerTests()
    {
        // File copied from https://github.com/libgit2/libgit2sharp/pull/1034/files
        _sampleDiff = File.ReadAllText(Path.Combine(_testDataDir, "Sample.diff"));
        _sampleCombinedDiff = File.ReadAllText(Path.Combine(_testDataDir, "SampleCombined.diff"));

        // Adjust the colors to match the current theme. (See code comments for limitations.)
        Color added = AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
        Color removed = AppColor.AnsiTerminalRedBackNormal.GetThemeColor();
        _sampleGitWordDiff = File.ReadAllText(Path.Combine(_testDataDir, "SampleGitWord.diff"))
            .Replace(@";200;255;200m", $";{added.R};{added.G};{added.B}m")
            .Replace(@";255;200;200m", $";{removed.R};{removed.G};{removed.B}m");
        _sampleDifftastic = File.ReadAllText(Path.Combine(_testDataDir, "SampleDifftastic.diff"));
        _textEditor = new TextEditorControl();
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
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: true);
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
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

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
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

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
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false);

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
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: true);

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
        string text = _sampleGitWordDiff;
        DiffHighlightService diffHighlightService = new PatchHighlightService(ref text, useGitColoring: true);
        _textEditor.Text = text;
        diffHighlightService.AddTextHighlighting(_textEditor.Document);
        DiffLinesInfo result = DiffLineNumAnalyzer.Analyze(_textEditor, isCombinedDiff: false, isGitWordDiff: true);

        result.DiffLines[5].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[5].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[5].LineType.Should().Be(DiffLineType.Header);

        result.DiffLines[6].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[6].RightLineNumber.Should().Be(1);
        result.DiffLines[6].LineType.Should().Be(DiffLineType.PlusRight);

        result.DiffLines[8].LeftLineNumber.Should().Be(1);
        result.DiffLines[8].RightLineNumber.Should().Be(3);
        result.DiffLines[8].LineType.Should().Be(DiffLineType.Context);

        result.DiffLines[15].LeftLineNumber.Should().Be(19);
        result.DiffLines[15].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[15].LineType.Should().Be(DiffLineType.MinusLeft);

        result.DiffLines[23].LeftLineNumber.Should().Be(28);
        result.DiffLines[23].RightLineNumber.Should().Be(28);
        result.DiffLines[23].LineType.Should().Be(DiffLineType.MinusPlus);
    }

    [Test]
    public void CanGetDifftasticInfo()
    {
        string text = _sampleDifftastic;
        EnvironmentAbstraction env = new();
        env.SetEnvironmentVariable("DFT_WIDTH", "200"); // Matching the value when getting the sample
        bool theme = AppSettings.ReverseGitColoring.Value;
        AppSettings.ReverseGitColoring.Value = false;

        DifftasticHighlightService diffHighlightService = new(ref text);
        _textEditor.Text = text;

        diffHighlightService.AddTextHighlighting(_textEditor.Document);
        DiffViewerLineNumberControl lineNumbersControl = new(_textEditor.ActiveTextAreaControl.TextArea);
        diffHighlightService.SetLineControl(lineNumbersControl, _textEditor);
        DiffLinesInfo result = lineNumbersControl.GetTestAccessor().Result;

        result.DiffLines[5].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[5].RightLineNumber.Should().Be(16);
        result.DiffLines[5].LineType.Should().Be(DiffLineType.PlusRight);

        result.DiffLines[22].LeftLineNumber.Should().Be(51);
        result.DiffLines[22].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[22].LineType.Should().Be(DiffLineType.MinusLeft);

        result.DiffLines[29].LeftLineNumber.Should().Be(106);
        result.DiffLines[29].RightLineNumber.Should().Be(111);
        result.DiffLines[29].LineType.Should().Be(DiffLineType.Context);

        result.DiffLines[30].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[30].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[30].LineType.Should().Be(DiffLineType.Context);

        result.DiffLines[33].LeftLineNumber.Should().Be(109);
        result.DiffLines[33].RightLineNumber.Should().Be(114);
        result.DiffLines[33].LineType.Should().Be(DiffLineType.MinusPlus);

        result.DiffLines[34].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[34].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[34].LineType.Should().Be(DiffLineType.MinusPlus);

        result.DiffLines[56].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        result.DiffLines[56].RightLineNumber.Should().Be(342);
        result.DiffLines[56].LineType.Should().Be(DiffLineType.PlusRight);

        AppSettings.ReverseGitColoring.Value = theme;
    }
}
