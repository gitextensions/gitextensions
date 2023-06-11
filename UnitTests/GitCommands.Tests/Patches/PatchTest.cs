﻿using GitCommands.Patches;

namespace GitCommandsTests.Patches
{
    [TestFixture]
    internal class PatchTest
    {
        [Test]
        public void TestPatchConstructor()
        {
            Patch patch = new("header", "index", PatchFileType.Text, "A", "B", true, PatchChangeType.NewFile, "text");

            Assert.AreEqual("header", patch.Header);
            Assert.AreEqual("index", patch.Index);
            Assert.AreEqual(PatchFileType.Text, patch.FileType);
            Assert.AreEqual("A", patch.FileNameA);
            Assert.AreEqual("B", patch.FileNameB);
            Assert.IsTrue(patch.IsCombinedDiff);
            Assert.AreEqual(PatchChangeType.NewFile, patch.ChangeType);
            Assert.AreEqual("text", patch.Text);
        }
    }
}
