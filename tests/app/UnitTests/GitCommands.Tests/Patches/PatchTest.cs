using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Patches;

[TestFixture]
internal class PatchTest
{
    [Test]
    public void TestPatchConstructor()
    {
        Patch patch = new("header", "index", PatchFileType.Text, "A", "B", PatchChangeType.NewFile, "text");

        ClassicAssert.AreEqual("header", patch.Header);
        ClassicAssert.AreEqual("index", patch.Index);
        ClassicAssert.AreEqual(PatchFileType.Text, patch.FileType);
        ClassicAssert.AreEqual("A", patch.FileNameA);
        ClassicAssert.AreEqual("B", patch.FileNameB);
        ClassicAssert.AreEqual(PatchChangeType.NewFile, patch.ChangeType);
        ClassicAssert.AreEqual("text", patch.Text);
    }
}
