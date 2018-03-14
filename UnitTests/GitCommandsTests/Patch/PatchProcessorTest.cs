using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using NUnit.Framework;
using PatchApply;

namespace GitCommandsTests.PatchApply
{
    [TestFixture]
    public sealed class PatchProcessorTest
    {
        private readonly string _bigPatch;
        private readonly string _bigBinPatch;
        private readonly string _rebaseDiff;

        public PatchProcessorTest()
        {
            _bigPatch = LoadPatch("big.patch");
            _bigBinPatch = LoadPatch("bigBin.patch");
            _rebaseDiff = LoadPatch("rebase.diff");

            string LoadPatch(string fileName)
            {
                var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Patch/testdata", fileName);
                var bytes = File.ReadAllBytes(path);
                return GitModule.LosslessEncoding.GetString(bytes);
            }
        }

        [Test]
        public void TestCorrectlyLoadPatch()
        {
            TestPatch expectedPatch = CreateSmallPatchExample();

            var patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput.ToString(), Encoding.UTF8);

            Patch createdPatch = patches.First();

            Assert.AreEqual(expectedPatch.Patch.PatchHeader, createdPatch.PatchHeader);
            Assert.AreEqual(expectedPatch.Patch.FileNameA, createdPatch.FileNameA);
            Assert.AreEqual(expectedPatch.Patch.PatchIndex, createdPatch.PatchIndex);
            Assert.AreEqual(expectedPatch.Patch.Type, createdPatch.Type);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadReversePatch()
        {
            TestPatch expectedPatch = CreateSmallPatchExample(true);

            var patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput.ToString(), Encoding.UTF8);

            Patch createdPatch = patches.First();

            Assert.AreEqual(expectedPatch.Patch.PatchHeader, createdPatch.PatchHeader, "header");
            Assert.AreEqual(expectedPatch.Patch.FileNameB, createdPatch.FileNameA, "fileA");
            Assert.AreEqual(expectedPatch.Patch.PatchIndex, createdPatch.PatchIndex);
            Assert.AreEqual(expectedPatch.Patch.Type, createdPatch.Type);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(17, patches.Count());
        }

        [Test]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8).ToList();

            Assert.AreEqual(17, patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [Test]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigBinPatch, Encoding.UTF8);

            Assert.AreEqual(248, patches.Count(p => p.File == Patch.FileType.Binary));
        }

        [Test]
        public void TestCorrectlyLoadsOneNewFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(1, patches.Count(p => p.Type == Patch.PatchType.NewFile));
        }

        [Test]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(1, patches.Count(p => p.Type == Patch.PatchType.DeleteFile));
        }

        [Test]
        public void TestCorrectlyLoadsChangeFiles()
        {
            var bigPatches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);
            Assert.AreEqual(15, bigPatches.Count(p => p.Type == Patch.PatchType.ChangeFile));

            var smallPatches = PatchProcessor.CreatePatchesFromString(CreateSmallPatchExample().PatchOutput.ToString(), Encoding.UTF8);
            Assert.AreEqual(1, smallPatches.Count(p => p.Type == Patch.PatchType.ChangeFile));
        }

        [Test]
        public void TestCorrectlyLoadsRebaseDiff()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_rebaseDiff, Encoding.UTF8).ToList();

            Assert.AreEqual(13, patches.Count);
            Assert.AreEqual(3, patches.Count(p => p.IsCombinedDiff));
        }

        private static TestPatch CreateSmallPatchExample(bool reverse = false)
        {
            var testPatch = new TestPatch
            {
                Patch =
                {
                    Type = Patch.PatchType.ChangeFile,
                    PatchHeader = reverse
                        ? "diff --git b/thisisatestb.txt a/thisisatesta.txt"
                        : "diff --git a/thisisatesta.txt b/thisisatestb.txt",
                    PatchIndex = "index 5e4dce2..5eb1e6f 100644",
                    FileNameA = "thisisatesta.txt",
                    FileNameB = "thisisatestb.txt"
                }
            };

            testPatch.AppendHeaderLine(testPatch.Patch.PatchHeader);
            testPatch.AppendHeaderLine(testPatch.Patch.PatchIndex);

            if (reverse)
            {
                testPatch.AppendHeaderLine("--- b/" + testPatch.Patch.FileNameB);
                testPatch.AppendHeaderLine("+++ a/" + testPatch.Patch.FileNameA);
            }
            else
            {
                testPatch.AppendHeaderLine("--- a/" + testPatch.Patch.FileNameA);
                testPatch.AppendHeaderLine("+++ b/" + testPatch.Patch.FileNameB);
            }

            var fileEncoding = Encoding.UTF8;

            testPatch.AppendDiffLine("@@ -1,2 +1,2 @@", fileEncoding);
            testPatch.AppendDiffLine(" iiiiii", fileEncoding);
            testPatch.AppendDiffLine("-ąśdkjaldskjlaksd", fileEncoding);
            testPatch.AppendDiffLine("+changed again€", fileEncoding);

            return testPatch;
        }

        private sealed class TestPatch
        {
            public Patch Patch { get; } = new Patch();
            public StringBuilder PatchOutput { get; } = new StringBuilder();

            public void AppendHeaderLine(string line)
            {
                Patch.AppendTextLine(line);
                PatchOutput.Append(GitModule.ReEncodeString(line, GitModule.SystemEncoding, GitModule.LosslessEncoding));
                PatchOutput.Append("\n");
            }

            public void AppendDiffLine(string line, Encoding fileEncoding)
            {
                Patch.AppendTextLine(line);
                PatchOutput.Append(GitModule.ReEncodeString(line, fileEncoding, GitModule.LosslessEncoding));
                PatchOutput.Append("\n");
            }
        }
    }
}