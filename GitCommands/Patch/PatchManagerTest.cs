using System.IO;
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
        private const string SmallPatchFile = @"GitCommands/Patch/testdata/small.patch";
        private const string BigPatchFile = @"GitCommands/Patch/testdata/big.patch";
        private const string BigBinPatchFile = @"GitCommands/Patch/testdata/bigBin.patch";

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

        [TestMethod]
        public void TestLoadPatch()
        {
            PatchManager manager = NewManager();
            Patch expectedPatch = new Patch
                                      {
                                          Type = Patch.PatchType.ChangeFile,
                                          Apply = true,
                                          PatchHeader = "diff --git a/thisisatest.txt b/thisisatest.txt",
                                          PatchIndex = "index 5e4dce2..5eb1e6f 100644",
                                          FileNameA = "thisisatest.txt",
                                          FileNameB = "thisisatest.txt"
                                      };
            expectedPatch.AppendTextLine(expectedPatch.PatchHeader);
            expectedPatch.AppendTextLine(expectedPatch.PatchIndex);
            expectedPatch.AppendTextLine("--- a/thisisatest.txt");
            expectedPatch.AppendTextLine("+++ b/thisisatest.txt");
            expectedPatch.AppendTextLine("@@ -1,2 +1,2 @@");
            expectedPatch.AppendTextLine("iiiiii");
            expectedPatch.AppendTextLine("-asdkjaldskjlaksd");
            expectedPatch.AppendTextLine("+changed again");

            manager.LoadPatch(expectedPatch.Text, false, Encoding.UTF8);

            Patch createdPatch = manager.Patches.First();
            Assert.AreEqual(expectedPatch.Text, createdPatch.Text);
        }


        [TestMethod]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Count);
        }

        [TestMethod]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(17, manager.Patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [TestMethod]
        public void TestCorrectlyLoadsBinaryPatch()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigBinPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(248, manager.Patches.Count(p => p.File == Patch.FileType.Binary));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneNewFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.NewFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigPatchFile));
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.DeleteFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsChangeFiles()
        {
            PatchManager manager = NewManager();
            var testBigPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(BigPatchFile));
            var testSmallPatch = Encoding.UTF8.GetString(LoadTestPatchDataBytes(SmallPatchFile));
            
            manager.LoadPatch(testBigPatch, false, Encoding.UTF8);
            Assert.AreEqual(15, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));

            manager.LoadPatch(testSmallPatch, false, Encoding.UTF8);
            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));
        }

        private static PatchManager NewManager()
        {
            return new PatchManager();
        }
    }
}