using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using GitCommands.Patches;
using NUnit.Framework;

namespace GitCommandsTests.Patches
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
                var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Patches/testdata", fileName);
                var bytes = File.ReadAllBytes(path);
                return GitModule.LosslessEncoding.GetString(bytes);
            }
        }

        [Test]
        public void TestCorrectlyLoadPatch()
        {
            TestPatch expectedPatch = CreateSmallPatchExample();

            var patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, Encoding.UTF8);

            Patch createdPatch = patches.First();

            Assert.AreEqual(expectedPatch.Patch.Header, createdPatch.Header);
            Assert.AreEqual(expectedPatch.Patch.FileNameA, createdPatch.FileNameA);
            Assert.AreEqual(expectedPatch.Patch.Index, createdPatch.Index);
            Assert.AreEqual(expectedPatch.Patch.ChangeType, createdPatch.ChangeType);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadReversePatch()
        {
            TestPatch expectedPatch = CreateSmallPatchExample(reverse: true);

            var patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, Encoding.UTF8);

            Patch createdPatch = patches.First();

            Assert.AreEqual(expectedPatch.Patch.Header, createdPatch.Header, "header");
            Assert.AreEqual(expectedPatch.Patch.FileNameB, createdPatch.FileNameA, "fileA");
            Assert.AreEqual(expectedPatch.Patch.Index, createdPatch.Index);
            Assert.AreEqual(expectedPatch.Patch.ChangeType, createdPatch.ChangeType);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(17, patches.Count());
        }

        [Test]
        public void TestCorrectlyLoadsTheRightFileNamesInAPatchFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8).ToList();

            Assert.AreEqual(17, patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [Test]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigBinPatch, Encoding.UTF8);

            Assert.AreEqual(248, patches.Count(p => p.FileType == PatchFileType.Binary));
        }

        [Test]
        public void TestCorrectlyLoadsOneNewFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.NewFile));
        }

        [Test]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);

            Assert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.DeleteFile));
        }

        [Test]
        public void TestCorrectlyLoadsChangeFiles()
        {
            var bigPatches = PatchProcessor.CreatePatchesFromString(_bigPatch, Encoding.UTF8);
            Assert.AreEqual(15, bigPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));

            var smallPatches = PatchProcessor.CreatePatchesFromString(CreateSmallPatchExample().PatchOutput, Encoding.UTF8);
            Assert.AreEqual(1, smallPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));
        }

        [Test]
        public void TestCorrectlyLoadsRebaseDiff()
        {
            var patches = PatchProcessor.CreatePatchesFromString(_rebaseDiff, Encoding.UTF8).ToList();

            Assert.AreEqual(13, patches.Count);
            Assert.AreEqual(3, patches.Count(p => p.IsCombinedDiff));
        }

        [Test]
        public void TestCombinedDiff()
        {
            const string diff = @"diff --cc GitCommands/Patches/PatchProcessor.cs
index ec3da25f4,5acc3b45b..000000000
--- a/GitCommands/Patches/PatchProcessor.cs
+++ b/GitCommands/Patches/PatchProcessor.cs
diff --combined UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
index cdf8bebba,55ff37bb9..000000000
--- a/UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
+++ b/UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
";

            var patches = PatchProcessor.CreatePatchesFromString(diff, Encoding.UTF8).ToList();

            Assert.AreEqual(2, patches.Count);
            Assert.IsTrue(patches.All(p => p.IsCombinedDiff));
        }

        private static TestPatch CreateSmallPatchExample(bool reverse = false)
        {
            var header = reverse
                ? "diff --git a/thisisatestb.txt b/thisisatesta.txt"
                : "diff --git b/thisisatesta.txt a/thisisatestb.txt";
            const string index = "index 5e4dce2..5eb1e6f 100644";
            const string fileNameA = "thisisatesta.txt";
            const string fileNameB = "thisisatestb.txt";

            var patchText = new StringBuilder();
            var patchOutput = new StringBuilder();

            AppendHeaderLine(header);
            AppendHeaderLine(index);

            if (reverse)
            {
                AppendHeaderLine("--- b/" + fileNameB);
                AppendHeaderLine("+++ a/" + fileNameA);
            }
            else
            {
                AppendHeaderLine("--- a/" + fileNameA);
                AppendHeaderLine("+++ b/" + fileNameB);
            }

            AppendDiffLine("@@ -1,2 +1,2 @@");
            AppendDiffLine(" iiiiii");
            AppendDiffLine("-ąśdkjaldskjlaksd");
            AppendDiffLine("+changed again€");

            var patch = new Patch(header, index, PatchFileType.Text, fileNameA, fileNameB, false, PatchChangeType.ChangeFile, patchText.ToString());

            return new TestPatch(patch, patchOutput.ToString());

            void AppendHeaderLine(string line)
            {
                patchText.Append(line).Append("\n");
                patchOutput.Append(GitModule.ReEncodeString(line, GitModule.SystemEncoding, GitModule.LosslessEncoding));
                patchOutput.Append("\n");
            }

            void AppendDiffLine(string line)
            {
                patchText.Append(line).Append("\n");
                patchOutput.Append(GitModule.ReEncodeString(line, Encoding.UTF8, GitModule.LosslessEncoding));
                patchOutput.Append("\n");
            }
        }

        private sealed class TestPatch
        {
            public Patch Patch { get; }
            public string PatchOutput { get; }

            public TestPatch(Patch patch, string patchOutput)
            {
                Patch = patch;
                PatchOutput = patchOutput;
            }
        }
    }
}