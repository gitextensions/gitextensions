using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Patches;
internal class PatchTest
{
    [Test]
    public void TestPatchConstructor()
    {
        Patch patch = new("header", "index", PatchFileType.Text, "A", "B", PatchChangeType.NewFile, "text");

        patch.Header.Should().Be("header");
        patch.Index.Should().Be("index");
        patch.FileType.Should().Be(PatchFileType.Text);
        patch.FileNameA.Should().Be("A");
        patch.FileNameB.Should().Be("B");
        patch.ChangeType.Should().Be(PatchChangeType.NewFile);
        patch.Text.Should().Be("text");
    }
}
