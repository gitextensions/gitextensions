using System;
using System.IO;
using GitCommands;
using NUnit.Framework;
using PatchApply;
using TestContext = System.Object;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using System.Linq;
using System.Text;

namespace GitExtensionsTest.Patches
{
    [TestClass]
    public class PatchManagerTest
    {
        private const string BigPatchFile = @"GitCommands/Patch/testdata/big.patch";
        private const string BigBinPatchFile = @"GitCommands/Patch/testdata/bigBin.patch";
        private const string RebaseDiffFile = @"GitCommands/Patch/testdata/rebase.diff";

        [TestMethod]
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

        public TestPatch CreateSmallPatchExample()
        {
            TestPatch testPatch = new TestPatch();
            Encoding fileEncoding = Encoding.UTF8;
            testPatch.Patch.Type = Patch.PatchType.ChangeFile;
            testPatch.Patch.Apply = true;
            testPatch.Patch.PatchHeader = "diff --git a/thisisatest.txt b/thisisatest.txt";
            testPatch.Patch.PatchIndex = "index 5e4dce2..5eb1e6f 100644";
            testPatch.Patch.FileNameA = "thisisatest.txt";
            testPatch.Patch.FileNameB = "thisisatest.txt";
            testPatch.AppendHeaderLine(testPatch.Patch.PatchHeader);
            testPatch.AppendHeaderLine(testPatch.Patch.PatchIndex);
            testPatch.AppendHeaderLine("--- a/" + testPatch.Patch.FileNameA);
            testPatch.AppendHeaderLine("+++ b/" + testPatch.Patch.FileNameB);
            testPatch.AppendDiffLine("@@ -1,2 +1,2 @@", fileEncoding);
            testPatch.AppendDiffLine(" iiiiii", fileEncoding);
            testPatch.AppendDiffLine("-ąśdkjaldskjlaksd", fileEncoding);
            testPatch.AppendDiffLine("+changed again€", fileEncoding);

            return testPatch;
        }

        [TestMethod]
        public void TestCorrectlyLoadPatch()
        {
            PatchManager manager = NewManager();
            TestPatch expectedPatch = CreateSmallPatchExample();

            manager.LoadPatch(expectedPatch.PatchOutput.ToString(), false, Encoding.UTF8);

            Patch createdPatch = manager.Patches.First();

            Assert.AreEqual(expectedPatch.Patch.PatchHeader, createdPatch.PatchHeader);
            Assert.AreEqual(expectedPatch.Patch.FileNameA, createdPatch.FileNameA);
            Assert.AreEqual(expectedPatch.Patch.PatchIndex, createdPatch.PatchIndex);
            Assert.AreEqual(expectedPatch.Patch.Rate, createdPatch.Rate);
            Assert.AreEqual(expectedPatch.Patch.Type, createdPatch.Type);
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }


        [TestMethod]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Count);
        }

        [TestMethod]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [TestMethod]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigBinPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(248, manager.Patches.Count(p => p.File == Patch.FileType.Binary));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneNewFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.NewFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.DeleteFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsChangeFiles()
        {
            PatchManager manager = NewManager();
            var testBigPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(BigPatchFile));
            var testSmallPatch = CreateSmallPatchExample();

            manager.LoadPatch(testBigPatch, false, Encoding.UTF8);
            Assert.AreEqual(15, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));

            manager.LoadPatch(testSmallPatch.PatchOutput.ToString(), false, Encoding.UTF8);
            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsRebaseDiff()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(LoadTestPatchDataBytes(RebaseDiffFile));
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
            public Patch Patch { get; private set; }
            public StringBuilder PatchOutput { get; private set; }

            public TestPatch()
            {
                Patch = new Patch();
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