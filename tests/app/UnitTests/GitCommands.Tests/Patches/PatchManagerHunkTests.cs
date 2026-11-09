using System.Text;
using GitCommands.Patches;

namespace GitCommandsTests.Patches;

[TestFixture]
public sealed class PatchManagerHunkTests
{
    private const string _diff =
        "diff --git a/f.txt b/f.txt\n" +
        "index 1111111..2222222 100644\n" +
        "--- a/f.txt\n" +
        "+++ b/f.txt\n" +
        "@@ -1,3 +1,3 @@\n" +
        " a\n" +
        "-b\n" +
        "+B\n" +
        " c\n" +
        "@@ -10,3 +10,3 @@\n" +
        " x\n" +
        "-y\n" +
        "+Y\n" +
        " z\n";

    private static readonly int _secondHunkStart = _diff.IndexOf("@@ -10", StringComparison.Ordinal);

    [Test]
    public void GetHunkRange_should_return_null_in_file_header()
    {
        PatchManager.GetHunkRange(_diff, _diff.IndexOf("index", StringComparison.Ordinal)).Should().BeNull();
        PatchManager.GetHunkRange("", 0).Should().BeNull();
    }

    [TestCase("@@ -1,3")]
    [TestCase("-b")]
    [TestCase(" c")]
    public void GetHunkRange_should_return_first_hunk(string lineInHunk)
    {
        int firstHunkStart = _diff.IndexOf("@@ -1,3", StringComparison.Ordinal);

        PatchManager.GetHunkRange(_diff, _diff.IndexOf(lineInHunk, StringComparison.Ordinal) + 1)
            .Should().Be((firstHunkStart, _secondHunkStart - 1 - firstHunkStart));
    }

    [TestCase("@@ -10")]
    [TestCase("+Y")]
    [TestCase(" z")]
    public void GetHunkRange_should_return_last_hunk(string lineInHunk)
    {
        PatchManager.GetHunkRange(_diff, _diff.IndexOf(lineInHunk, StringComparison.Ordinal))
            .Should().Be((_secondHunkStart, _diff.Length - _secondHunkStart));
        PatchManager.GetHunkRange(_diff, _diff.Length)
            .Should().Be((_secondHunkStart, _diff.Length - _secondHunkStart));
    }

    [Test]
    public void Patch_from_hunk_range_should_contain_only_that_hunk()
    {
        (int position, int length) = PatchManager.GetHunkRange(_diff, _diff.IndexOf("+B", StringComparison.Ordinal))!.Value;

        byte[]? patch = PatchManager.GetSelectedLinesAsPatch(_diff, position, length, isIndex: false, Encoding.UTF8, reset: false, isNewFile: false, isRenamed: false);

        string patchText = Encoding.UTF8.GetString(patch!);
        patchText.Should().Contain("-b\n+B\n");
        patchText.Should().NotContain("Y");
    }
}
