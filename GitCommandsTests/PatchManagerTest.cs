#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using GitCommands;
using System.Linq;
using System.Text;
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

            TestPatch expectedPatch = new TestPatch();
            Encoding fileEncoding = Encoding.UTF8;
            expectedPatch.Patch.Type = Patch.PatchType.ChangeFile;
            expectedPatch.Patch.Apply = true;
            expectedPatch.Patch.PatchHeader = "diff --git a/thisisatest.txt b/thisisatest.txt";
            expectedPatch.Patch.PatchIndex = "index 5e4dce2..5eb1e6f 100644";
            expectedPatch.Patch.FileNameA = "thisisatest.txt";
            expectedPatch.Patch.FileNameB = "thisisatest.txt";
            expectedPatch.AppendHeaderLine(expectedPatch.Patch.PatchHeader);
            expectedPatch.AppendHeaderLine(expectedPatch.Patch.PatchIndex);
            expectedPatch.AppendHeaderLine("--- a/" + expectedPatch.Patch.FileNameA);
            expectedPatch.AppendHeaderLine("+++ b/" + expectedPatch.Patch.FileNameB);
            expectedPatch.AppendDiffLine("@@ -1,2 +1,2 @@", fileEncoding);
            expectedPatch.AppendDiffLine(" iiiiii", fileEncoding);
            expectedPatch.AppendDiffLine("-ąśdkjaldskjlaksd", fileEncoding);
            expectedPatch.AppendDiffLine("+changed again€", fileEncoding);

            manager.LoadPatch(expectedPatch.PatchOutput.ToString(), false, fileEncoding);

            Patch createdPatch = manager.Patches.First();
            Assert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
        }


        [TestMethod]
        public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(12, manager.Patches.Count);
        }

        [TestMethod]
        public void TestCorrectlyLoadsTheRightFilenamesInAPatchFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(12, manager.Patches.Select(p => p.FileNameA).Distinct().Count());
            Assert.AreEqual(12, manager.Patches.Select(p => p.FileNameB).Distinct().Count());
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneBinaryPatch()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);
            
            Assert.AreEqual(1, manager.Patches.Count(p => p.File == Patch.FileType.Binary));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneNewFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.NewFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsOneDeleteFile()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(1, manager.Patches.Count(p => p.Type == Patch.PatchType.DeleteFile));
        }

        [TestMethod]
        public void TestCorrectlyLoadsTenChangeFiles()
        {
            PatchManager manager = NewManager();
            var testPatch = GitModule.LosslessEncoding.GetString(TestResource.TestPatch);
            manager.LoadPatch(testPatch, false, Encoding.UTF8);

            Assert.AreEqual(10, manager.Patches.Count(p => p.Type == Patch.PatchType.ChangeFile));
        }

        [TestMethod]
        public void TestGetSelectedLinesAsPatchReturnsNull()
        {
            Assert.IsNull(PatchManager.GetSelectedLinesAsPatch(new GitModule(null), null, -1, -1, false, Encoding.UTF8));
        }

        private static PatchManager NewManager()
        {
            return new PatchManager();
        }
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