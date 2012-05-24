using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatchApply;

namespace GitCommandsTests
{
    [TestClass]
    public class PatchManagerTest
    {
        [TestMethod]
        public void TestPatchManagerInstanceNotNull()
        {
            PatchManager manager = NewManager();
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void TestLoadPatch()
        {
            PatchManager manager = NewManager();

            Patch expectedPatch = new Patch();
            expectedPatch.Type = Patch.PatchType.ChangeFile;
            expectedPatch.Apply = true;
            expectedPatch.PatchHeader = "diff --git a/thisisatest.txt b/thisisatest.txt";
            expectedPatch.PatchIndex = "index 5e4dce2..5eb1e6f 100644";
            expectedPatch.FileNameA = "thisisatest.txt";
            expectedPatch.FileNameB = "thisisatest.txt";
            expectedPatch.AppendTextLine(expectedPatch.PatchHeader);
            expectedPatch.AppendTextLine(expectedPatch.PatchIndex);
            expectedPatch.AppendTextLine("--- a/thisisatest.txt");
            expectedPatch.AppendTextLine("+++ b/thisisatest.txt");
            expectedPatch.AppendTextLine("@@ -1,2 +1,2 @@");
            expectedPatch.AppendTextLine("iiiiii");
            expectedPatch.AppendTextLine("-asdkjaldskjlaksd");
            expectedPatch.AppendTextLine("+changed again");

            manager.LoadPatch(expectedPatch.Text, false);
           
            Patch createdPatch = manager.Patches.First();
            Assert.AreEqual(expectedPatch.Text, createdPatch.Text);
        }


        [TestMethod]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);

            Assert.AreEqual(12, manager.Patches.Count);
        }

        [TestMethod]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);

            Assert.AreEqual(12, manager.Patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(12, manager.Patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneBinaryPatch()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);
            
            Assert.AreEqual(1, manager.Patches.Count(p => p.File == Patch.FileType.Binary));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneNewFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.NewFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.DeleteFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsTenChangeFiles()
        {
            PatchManager manager = NewManager();
            var testPatch = Encoding.UTF8.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false);

            Assert.AreEqual(10, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));
        }

        [TestMethod]
        public void TestGetSelectedLinesAsPatchReturnsNull()
        {
            Assert.IsNull(PatchManager.GetSelectedLinesAsPatch(null, -1, -1, false));
        }

        private static PatchManager NewManager()
        {
            return new PatchManager();
        }
    }
}