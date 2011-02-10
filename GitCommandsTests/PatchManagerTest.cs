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

        private static PatchManager NewManager()
        {
            return new PatchManager();
        }
    }
}