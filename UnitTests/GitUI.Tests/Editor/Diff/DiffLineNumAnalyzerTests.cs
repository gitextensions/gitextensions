using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GitUI.Editor.Diff;
using NUnit.Framework;

namespace GitUITests.Editor.Diff
{
    [TestFixture]
    public class DiffLineNumAnalyzerTests
    {
        private static readonly string TestDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff");
        private readonly string _sampleDiff;
        private readonly string _sampleCombinedDiff;
        private DiffLineNumAnalyzer _lineNumAnalyzer;

        public DiffLineNumAnalyzerTests()
        {
            // File copied from https://github.com/libgit2/libgit2sharp/pull/1034/files
            _sampleDiff = File.ReadAllText(Path.Combine(TestDataDir, "Sample.diff"));

            _sampleCombinedDiff = File.ReadAllText(Path.Combine(TestDataDir, "SampleCombined.diff"));
        }

        [SetUp]
        public void SetUp()
        {
            _lineNumAnalyzer = new DiffLineNumAnalyzer();
        }

        [Test]
        public void CanGetHeaders()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);
            var headerLines = new List<int> { 5, 17 };
            foreach (var header in headerLines)
            {
                result.DiffLines[header].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
                result.DiffLines[header].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
            }
        }

        [Test]
        public void CanGetContextLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.DiffLines[6].LeftLineNumber.Should().Be(9);
            result.DiffLines[6].RightLineNumber.Should().Be(9);

            result.DiffLines[14].LeftLineNumber.Should().Be(15);
            result.DiffLines[14].RightLineNumber.Should().Be(16);

            result.DiffLines[18].LeftLineNumber.Should().Be(33);
            result.DiffLines[18].RightLineNumber.Should().Be(34);

            result.DiffLines[25].LeftLineNumber.Should().Be(39);
            result.DiffLines[25].RightLineNumber.Should().Be(40);
        }

        [Test]
        public void CanGetMinusLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.DiffLines[9].LeftLineNumber.Should().Be(12);
            result.DiffLines[9].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);

            result.DiffLines[21].LeftLineNumber.Should().Be(36);
            result.DiffLines[21].RightLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
        }

        [Test]
        public void CanGetPlusLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.DiffLines[12].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
            result.DiffLines[12].RightLineNumber.Should().Be(14);

            result.DiffLines[13].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
            result.DiffLines[13].RightLineNumber.Should().Be(15);

            result.DiffLines[22].LeftLineNumber.Should().Be(DiffLineInfo.NotApplicableLineNum);
            result.DiffLines[22].RightLineNumber.Should().Be(37);
        }

        [Test]
        public void CanGetLineNumbersForCombinedDiff()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleCombinedDiff);

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
    }
}
