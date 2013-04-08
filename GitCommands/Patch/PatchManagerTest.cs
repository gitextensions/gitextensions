using NUnit.Framework;
using PatchApply;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitExtensionsTest.Patch
{
    [TestClass]
    class PatchManagerTest
    {
        [TestMethod]
        public void TestPatchManagerConstructor()
        {
            PatchManager manager = new PatchManager();
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void TestLoadPatch()
        {
            PatchManager manager = new PatchManager();

            PatchApply.Patch expectedPatch = new PatchApply.Patch();
            expectedPatch.Type = PatchApply.Patch.PatchType.ChangeFile;
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

            manager.LoadPatch(expectedPatch.Text, false, Encoding.Default);

            PatchApply.Patch createdPatch = manager.Patches.First();
            Assert.AreEqual(expectedPatch.Text, createdPatch.Text);
        }

    }
}
