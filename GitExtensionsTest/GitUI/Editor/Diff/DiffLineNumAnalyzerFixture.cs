using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using GitUI.Editor.Diff;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensionsTest.GitUI.Editor.Diff
{
    public interface IDiffLineNumRecv
    {
        void OnLineNumAnalyzed(DiffLineNum lineNum);
    }

    [TestFixture]
    public class TestDiffLineNumAnalyzer
    {
        private static readonly string TestDataDir = Path.Combine(GetCallingAssemblyDir(),
            "GitUI", "Editor", "Diff");
        private readonly string _sampleDiff;
        private readonly string _sampleCombindedDiff;
        private DiffLineNumAnalyzer _lineNumAnalyzer;
        private IDiffLineNumRecv _lineNumMetaRecv;

        private static string GetCallingAssemblyDir()
        {
            var codeBase = Assembly.GetCallingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }

        public TestDiffLineNumAnalyzer()
        {
            // File copied from https://github.com/libgit2/libgit2sharp/pull/1034/files
            _sampleDiff = File.ReadAllText(Path.Combine(TestDataDir, "Sample.diff"));

            _sampleCombindedDiff = File.ReadAllText(Path.Combine(TestDataDir, "SampleCombined.diff"));
        }

        [SetUp]
        public void SetUp()
        {
            _lineNumAnalyzer = new DiffLineNumAnalyzer();
            _lineNumMetaRecv = Substitute.For<IDiffLineNumRecv>();
            _lineNumAnalyzer.OnLineNumAnalyzed += _lineNumMetaRecv.OnLineNumAnalyzed;
        }

        [Test]
        public void CanGetHeaders()
        {
            _lineNumAnalyzer.Start(_sampleDiff);
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 5
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 17
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));
        }

        [Test]
        public void CanGetContextLines()
        {
            _lineNumAnalyzer.Start(_sampleDiff);

            // header1
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 6
                && line.LeftLineNum == 9
                && line.RightLineNum == 9));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 14
                && line.LeftLineNum == 15
                && line.RightLineNum == 16));

            // header2
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 18
                && line.LeftLineNum == 33
                && line.RightLineNum == 34));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 25
                && line.LeftLineNum == 39
                && line.RightLineNum == 40));
        }

        [Test]
        public void CanGetMinusLines()
        {
            _lineNumAnalyzer.Start(_sampleDiff);

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 9
                && line.LeftLineNum == 12
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 21
                && line.LeftLineNum == 36
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));
        }

        [Test]
        public void CanGetPlusLines()
        {
            _lineNumAnalyzer.Start(_sampleDiff);

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 12
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 14));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 13
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 15));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 22
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 37));
        }

        [Test]
        public void CanGetLineNumbersForCombinedDiff()
        {
            _lineNumAnalyzer.Start(_sampleCombindedDiff);

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 6
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 70
                && line.Style == DiffLineNum.DiffLineStyle.Context));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 9
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 73
                && line.Style == DiffLineNum.DiffLineStyle.Plus));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 19
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 83
                && line.Style == DiffLineNum.DiffLineStyle.Plus));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 34
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 100
                && line.Style == DiffLineNum.DiffLineStyle.Context));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 37
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum
                && line.Style == DiffLineNum.DiffLineStyle.Minus));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 38
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 103
                && line.Style == DiffLineNum.DiffLineStyle.Plus));
        }

    }
}
