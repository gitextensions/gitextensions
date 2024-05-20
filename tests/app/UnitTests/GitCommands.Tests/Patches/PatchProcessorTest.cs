using System.Text;
using GitCommands;
using GitCommands.Patches;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Patches
{
    [TestFixture]
    public sealed class PatchProcessorTest
    {
        private readonly string _bigPatch;
        private readonly string _bigBinPatch;
        private readonly string _rebaseDiff;
        private readonly string _colorDiff;
        private readonly string _colorBinDiff;
        private readonly string _colorPrefixDiff;

        public PatchProcessorTest()
        {
            _bigPatch = LoadPatch("big.patch");
            _bigBinPatch = LoadPatch("bigBin.patch");
            _rebaseDiff = LoadPatch("rebase.diff");
            _colorDiff = LoadPatch("color.diff");
            _colorBinDiff = LoadPatch("color-binary.diff");
            _colorPrefixDiff = LoadPatch("color-prefix.diff");

            string LoadPatch(string fileName)
            {
                string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Patches/testdata", fileName);
                byte[] bytes = File.ReadAllBytes(path);
                return GitModule.LosslessEncoding.GetString(bytes);
            }
        }

        [Test]
        public void TestCorrectlyLoadPatch()
        {
            TestPatch expectedPatch = CreateSmallPatchExample();

            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));

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

            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));

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
            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

            Assert.AreEqual(17, patches.Count());
        }

        [Test]
        public void TestCorrectlyLoadsTheRightFileNamesInAPatchFile()
        {
            List<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(17, patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [Test]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigBinPatch, new Lazy<Encoding>(() => Encoding.UTF8));

            Assert.AreEqual(248, patches.Count(p => p.FileType == PatchFileType.Binary));
        }

        [Test]
        public void TestCorrectlyLoadsOneNewFile()
        {
            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

            Assert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.NewFile));
        }

        [Test]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

            Assert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.DeleteFile));
        }

        [Test]
        public void TestCorrectlyLoadsChangeFiles()
        {
            IEnumerable<Patch> bigPatches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));
            Assert.AreEqual(15, bigPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));

            IEnumerable<Patch> smallPatches = PatchProcessor.CreatePatchesFromString(CreateSmallPatchExample().PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));
            Assert.AreEqual(1, smallPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));
        }

        [Test]
        public void TestCorrectlyLoadsRebaseDiff()
        {
            List<Patch> patches = PatchProcessor.CreatePatchesFromString(_rebaseDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(13, patches.Count);
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

            List<Patch> patches = PatchProcessor.CreatePatchesFromString(diff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(2, patches.Count);
        }

        [Test]
        public void ColorDiff()
        {
            List<Patch> patches = PatchProcessor.CreatePatchesFromString(_colorDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(1, patches.Count);
            Patch createdPatch = patches.First();

            Assert.AreEqual(@"diff --git a/GitCommands/Patches/PatchProcessor.cs b/GitCommands/Patches/PatchProcessor.cs", createdPatch.Header, "header");
            Assert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameA, "fileA");
            Assert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameB, "fileB");
            Assert.AreEqual("index 70b40..c1e6c 100644", createdPatch.Index);
            Assert.AreEqual(PatchChangeType.ChangeFile, createdPatch.ChangeType);
            Assert.AreEqual(PatchFileType.Text, createdPatch.FileType);
        }

        // Prefix must *not* contains a space or a '/' (except the mandatory one at the end)
        [TestCase("before:/", "after:/")]
        [TestCase("a:./", "b:./")]
        [TestCase("./", "./")]
        public void ColorPrefixDiff(string prefixSrc, string prefixDst)
        {
            string diffWithCutomPrefixes = _colorPrefixDiff.Replace("[PLACEHOLDER_PREFIX_SRC]", prefixSrc).Replace("[PLACEHOLDER_PREFIX_DST]", prefixDst);
            List<Patch> patches = PatchProcessor.CreatePatchesFromString(diffWithCutomPrefixes, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(1, patches.Count);
            Patch createdPatch = patches.First();

            Assert.AreEqual(@$"diff --git {prefixSrc}GitCommands/Patches/PatchProcessor.cs {prefixDst}GitCommands/Patches/PatchProcessor.cs", createdPatch.Header, "header");
            Assert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameA, "fileA");
            Assert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameB, "fileB");
            Assert.AreEqual("index 70b40..c1e6c 100644", createdPatch.Index);
            Assert.AreEqual(PatchChangeType.ChangeFile, createdPatch.ChangeType);
            Assert.AreEqual(PatchFileType.Text, createdPatch.FileType);
        }

        [Test]
        public void ColorBinDiff()
        {
            List<Patch> patches = PatchProcessor.CreatePatchesFromString(_colorBinDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

            Assert.AreEqual(1, patches.Count);
            Patch createdPatch = patches.First();

            Assert.AreEqual(@"diff --git a/syscolor 3 gray.7z b/syscolor 3 gray.7z", createdPatch.Header, "header");
            Assert.AreEqual("syscolor 3 gray.7z", createdPatch.FileNameA, "fileA");
            Assert.AreEqual("syscolor 3 gray.7z", createdPatch.FileNameB, "fileB");
            Assert.AreEqual("index 33b006c1..00000000", createdPatch.Index);
            Assert.AreEqual(PatchChangeType.DeleteFile, createdPatch.ChangeType);
            Assert.AreEqual(PatchFileType.Binary, createdPatch.FileType);
        }

        private static TestPatch CreateSmallPatchExample(bool reverse = false)
        {
            string header = reverse
                ? "diff --git a/thisisatestb.txt b/thisisatesta.txt"
                : "diff --git b/thisisatesta.txt a/thisisatestb.txt";
            const string index = "index 5e4dce2..5eb1e6f 100644";
            const string fileNameA = "thisisatesta.txt";
            const string fileNameB = "thisisatestb.txt";

            StringBuilder patchText = new();
            StringBuilder patchOutput = new();

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

            Patch patch = new(header, index, PatchFileType.Text, fileNameA, fileNameB, PatchChangeType.ChangeFile, patchText.ToString());

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
