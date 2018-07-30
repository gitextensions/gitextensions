using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GitUI.Editor.Diff;
using NUnit.Framework;

namespace GitUITests.Editor.Diff
{
    [TestFixture]
    public class TestDiffLineNumAnalyzer
    {
        private static readonly string TestDataDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Editor", "Diff");
        private readonly string _sampleDiff;
        private readonly string _sampleCombinedDiff;
        private DiffLineNumAnalyzer _lineNumAnalyzer;

        public TestDiffLineNumAnalyzer()
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
                result.LineNumbers[header].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
                result.LineNumbers[header].RightLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            }
        }

        [Test]
        public void CanGetContextLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.LineNumbers[6].LeftLineNum.Should().Be(9);
            result.LineNumbers[6].RightLineNum.Should().Be(9);

            result.LineNumbers[14].LeftLineNum.Should().Be(15);
            result.LineNumbers[14].RightLineNum.Should().Be(16);

            result.LineNumbers[18].LeftLineNum.Should().Be(33);
            result.LineNumbers[18].RightLineNum.Should().Be(34);

            result.LineNumbers[25].LeftLineNum.Should().Be(39);
            result.LineNumbers[25].RightLineNum.Should().Be(40);
        }

        [Test]
        public void CanGetMinusLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.LineNumbers[9].LeftLineNum.Should().Be(12);
            result.LineNumbers[9].RightLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);

            result.LineNumbers[21].LeftLineNum.Should().Be(36);
            result.LineNumbers[21].RightLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
        }

        [Test]
        public void CanGetPlusLines()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleDiff);

            result.LineNumbers[12].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[12].RightLineNum.Should().Be(14);

            result.LineNumbers[13].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[13].RightLineNum.Should().Be(15);

            result.LineNumbers[22].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[22].RightLineNum.Should().Be(37);
        }

        [Test]
        public void CanGetLineNumbersForCombinedDiff()
        {
            var result = _lineNumAnalyzer.Analyze(_sampleCombinedDiff);

            result.LineNumbers[6].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[6].RightLineNum.Should().Be(70);
            result.LineNumbers[6].Style.Should().Be(DiffLineNum.DiffLineStyle.Context);

            result.LineNumbers[9].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[9].RightLineNum.Should().Be(73);
            result.LineNumbers[9].Style.Should().Be(DiffLineNum.DiffLineStyle.Plus);

            result.LineNumbers[19].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[19].RightLineNum.Should().Be(83);
            result.LineNumbers[19].Style.Should().Be(DiffLineNum.DiffLineStyle.Plus);

            result.LineNumbers[34].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[34].RightLineNum.Should().Be(100);
            result.LineNumbers[34].Style.Should().Be(DiffLineNum.DiffLineStyle.Context);

            result.LineNumbers[37].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[37].RightLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[37].Style.Should().Be(DiffLineNum.DiffLineStyle.Minus);

            result.LineNumbers[38].LeftLineNum.Should().Be(DiffLineNum.NotApplicableLineNum);
            result.LineNumbers[38].RightLineNum.Should().Be(103);
            result.LineNumbers[38].Style.Should().Be(DiffLineNum.DiffLineStyle.Plus);
        }
    }
}
