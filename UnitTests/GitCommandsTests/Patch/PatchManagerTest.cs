using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using NUnit.Framework;
using PatchApply;

namespace GitCommandsTests.Patch
{
    [TestFixture]
    public class PatchManagerTest
    {
        private readonly string _bigPatchFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Patch/testdata/big.patch");
        private readonly string _bigBinPatchFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Patch/testdata/bigBin.patch");
        private readonly string _rebaseDiffFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Patch/testdata/rebase.diff");

        [Test]
        public void TestPatchManagerInstanceNotNull()
        {
            PatchManager manager = NewManager();
            Assert.IsNotNull(manager);
        }

        public byte[] LoadTestPatchDataBytes(string fileName)
        {
            byte[] patchBytes;

            using (var reader = new StreamReader(fileName))
            {
                patchBytes = new BinaryReader(reader.BaseStream).ReadBytes((int)reader.BaseStream.Length);
            }

            return patchBytes;
        }

        public TestPatch CreateSmallPatchExample(bool reverse = false)
        {
            TestPatch testPatch = new TestPatch();
            Encoding fileEncoding = Encoding.UTF8;
            testPatch.Patch.Type = PatchApply.Patch.PatchType.ChangeFile;
            testPatch.Patch.Apply = true;
            if (reverse)
            {
                testPatch.Patch.PatchHeader = "diff --git b/thisisatestb.txt a/thisisatesta.txt";
            }
            else
            {
                testPatch.Patch.PatchHeader = "diff --git a/thisisatesta.txt b/thisisatestb.txt";
            }

            testPatch.Patch.PatchIndex = "index 5e4dce2..5eb1e6f 100644";
            testPatch.Patch.FileNameA = "thisisatesta.txt";
            testPatch.Patch.FileNameB = "thisisatestb.txt";
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

            testPatch.AppendDiffLine("@@ -1,2 +1,2 @@", fileEncoding);
            testPatch.AppendDiffLine(" iiiiii", fileEncoding);
            testPatch.AppendDiffLine("-ąśdkjaldskjlaksd", fileEncoding);
            testPatch.AppendDiffLine("+changed again€", fileEncoding);

            return testPatch;
        }

        [Test]
        public void TestCorrectlyLoadPatch()
        {
            PatchManager manager = NewManager();
            TestPatch expectedPatch = CreateSmallPatchExample();

            manager.LoadPatch(expectedPatch.PatchOutput.ToString(), false, Encoding.UTF8);

            PatchApply.Patch createdPatch = manager.Patches.First();

            Assert.AreEqual(expectedPatch.Patch.PatchHeader, createdPatch.PatchHeader);
            Assert.AreEqual(expectedPatch.Patch.FileNameA, createdPatch.FileNameA);
            Assert.AreEqual(expectedPatch.Patch.PatchIndex, createdPatch.PatchIndex);
            Assert.AreEqual(expectedPatch.Patch.Rate, createdPatch.Rate);
            Assert.AreEqual(expectedPatch.Patch.Type, createdPatch.Type);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadReversePatch()
        {
            PatchManager manager = NewManager();
            TestPatch expectedPatch = CreateSmallPatchExample(true);

            manager.LoadPatch(expectedPatch.PatchOutput.ToString(), false, Encoding.UTF8);

            PatchApply.Patch createdPatch = manager.Patches.First();

            Assert.AreEqual(expectedPatch.Patch.PatchHeader, createdPatch.PatchHeader, "header");
            Assert.AreEqual(expectedPatch.Patch.FileNameB, createdPatch.FileNameA, "fileA");
            Assert.AreEqual(expectedPatch.Patch.PatchIndex, createdPatch.PatchIndex);
            Assert.AreEqual(expectedPatch.Patch.Rate, createdPatch.Rate);
            Assert.AreEqual(expectedPatch.Patch.Type, createdPatch.Type);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }

        [Test]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Count);
        }

        [Test]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [Test]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigBinPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(248, manager.Patches.Count(p => p.File == PatchApply.Patch.FileType.Binary));
        }

        [Test]
        public void TestCorrectlyLoadsOneNewFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == PatchApply.Patch.PatchType.NewFile));
        }

        [Test]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == PatchApply.Patch.PatchType.DeleteFile));
        }

        [Test]
        public void TestCorrectlyLoadsChangeFiles()
        {
            PatchManager manager = NewManager();
            var testBigPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_bigPatchFile));
            var testSmallPatch = CreateSmallPatchExample();

            manager.LoadPatch(testBigPatch, false, Encoding.UTF8);
            Assert.AreEqual(15, manager.Patches.Count(p => p.Type == PatchApply.Patch.PatchType.ChangeFile));

            manager.LoadPatch(testSmallPatch.PatchOutput.ToString(), false, Encoding.UTF8);
            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == PatchApply.Patch.PatchType.ChangeFile));
        }

        [Test]
        public void TestCorrectlyLoadsRebaseDiff()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(_rebaseDiffFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(13, manager.Patches.Count);
            Assert.AreEqual(3, manager.Patches.Count(p => p.CombinedDiff));
        }

        private static PatchManager NewManager()
        {
            return new PatchManager();
        }

        public class TestPatch
        {
            public PatchApply.Patch Patch { get; }
            public StringBuilder PatchOutput { get; }

            public TestPatch()
            {
                Patch = new PatchApply.Patch();
                PatchOutput = new StringBuilder();
            }

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