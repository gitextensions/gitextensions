using System.Collections;
using System.Text;
using GitCommands;
using GitCommands.Patches;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Patches;

[TestFixture]
public sealed class PatchProcessorTest
{
    private readonly string _bigPatch;
    private readonly string _bigBinPatch;
    private readonly string _rebaseDiff;
    private readonly string _colorDiff;
    private readonly string _colorBinDiff;
    private readonly string _colorPrefixDiff;

    public PatchProcessorTest()
    {
        _bigPatch = LoadPatch("big.patch");
        _bigBinPatch = LoadPatch("bigBin.patch");
        _rebaseDiff = LoadPatch("rebase.diff");
        _colorDiff = LoadPatch("color.diff");
        _colorBinDiff = LoadPatch("color-binary.diff");
        _colorPrefixDiff = LoadPatch("color-prefix.diff");

        string LoadPatch(string fileName)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Patches/testdata", fileName);
            byte[] bytes = File.ReadAllBytes(path);
            return GitModule.LosslessEncoding.GetString(bytes);
        }
    }

    [Test]
    public void TestCorrectlyLoadPatch()
    {
        TestPatch expectedPatch = CreateSmallPatchExample();

        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));

        Patch createdPatch = patches.First();

        ClassicAssert.AreEqual(expectedPatch.Patch.Header, createdPatch.Header);
        ClassicAssert.AreEqual(expectedPatch.Patch.FileNameA, createdPatch.FileNameA);
        ClassicAssert.AreEqual(expectedPatch.Patch.Index, createdPatch.Index);
        ClassicAssert.AreEqual(expectedPatch.Patch.ChangeType, createdPatch.ChangeType);
        ClassicAssert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
    }

    [Test]
    public void TestCorrectlyLoadReversePatch()
    {
        TestPatch expectedPatch = CreateSmallPatchExample(reverse: true);

        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(expectedPatch.PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));

        Patch createdPatch = patches.First();

        ClassicAssert.AreEqual(expectedPatch.Patch.Header, createdPatch.Header, "header");
        ClassicAssert.AreEqual(expectedPatch.Patch.FileNameB, createdPatch.FileNameA, "fileA");
        ClassicAssert.AreEqual(expectedPatch.Patch.Index, createdPatch.Index);
        ClassicAssert.AreEqual(expectedPatch.Patch.ChangeType, createdPatch.ChangeType);
        ClassicAssert.AreEqual(expectedPatch.Patch.Text, createdPatch.Text);
    }

    [Test]
    public void TestCorrectlyLoadsTheRightNumberOfDiffsInAPatchFile()
    {
        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

        ClassicAssert.AreEqual(17, patches.Count());
    }

    [Test]
    public void TestCorrectlyLoadsTheRightFileNamesInAPatchFile()
    {
        List<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(17, patches.Select(p => p.FileNameA).Distinct().Count());
        ClassicAssert.AreEqual(17, patches.Select(p => p.FileNameB).Distinct().Count());
    }

    [Test]
    public void TestCorrectlyLoadsBinaryPatch()
    {
        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigBinPatch, new Lazy<Encoding>(() => Encoding.UTF8));

        ClassicAssert.AreEqual(248, patches.Count(p => p.FileType == PatchFileType.Binary));
    }

    [Test]
    public void TestCorrectlyLoadsOneNewFile()
    {
        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

        ClassicAssert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.NewFile));
    }

    [Test]
    public void TestCorrectlyLoadsOneDeleteFile()
    {
        IEnumerable<Patch> patches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));

        ClassicAssert.AreEqual(1, patches.Count(p => p.ChangeType == PatchChangeType.DeleteFile));
    }

    [Test]
    public void TestCorrectlyLoadsChangeFiles()
    {
        IEnumerable<Patch> bigPatches = PatchProcessor.CreatePatchesFromString(_bigPatch, new Lazy<Encoding>(() => Encoding.UTF8));
        ClassicAssert.AreEqual(15, bigPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));

        IEnumerable<Patch> smallPatches = PatchProcessor.CreatePatchesFromString(CreateSmallPatchExample().PatchOutput, new Lazy<Encoding>(() => Encoding.UTF8));
        ClassicAssert.AreEqual(1, smallPatches.Count(p => p.ChangeType == PatchChangeType.ChangeFile));
    }

    [Test]
    public void TestCorrectlyLoadsRebaseDiff()
    {
        List<Patch> patches = PatchProcessor.CreatePatchesFromString(_rebaseDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(13, patches.Count);
    }

    [Test]
    public void TestCombinedDiff()
    {
        const string diff = @"diff --cc GitCommands/Patches/PatchProcessor.cs
index ec3da25f4,5acc3b45b..000000000
--- a/GitCommands/Patches/PatchProcessor.cs
+++ b/GitCommands/Patches/PatchProcessor.cs
diff --combined UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
index cdf8bebba,55ff37bb9..000000000
--- a/UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
+++ b/UnitTests/GitCommandsTests/Patches/PatchProcessorTest.cs
";

        List<Patch> patches = PatchProcessor.CreatePatchesFromString(diff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(2, patches.Count);
    }

    [Test]
    public void ColorDiff()
    {
        List<Patch> patches = PatchProcessor.CreatePatchesFromString(_colorDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(1, patches.Count);
        Patch createdPatch = patches.First();

        ClassicAssert.AreEqual(@"diff --git a/GitCommands/Patches/PatchProcessor.cs b/GitCommands/Patches/PatchProcessor.cs", createdPatch.Header, "header");
        ClassicAssert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameA, "fileA");
        ClassicAssert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameB, "fileB");
        ClassicAssert.AreEqual("index 70b40..c1e6c 100644", createdPatch.Index);
        ClassicAssert.AreEqual(PatchChangeType.ChangeFile, createdPatch.ChangeType);
        ClassicAssert.AreEqual(PatchFileType.Text, createdPatch.FileType);
    }

    // Prefix must *not* contains a space or a '/' (except the mandatory one at the end)
    [TestCase("before:/", "after:/")]
    [TestCase("a:./", "b:./")]
    [TestCase("./", "./")]
    public void ColorPrefixDiff(string prefixSrc, string prefixDst)
    {
        string diffWithCutomPrefixes = _colorPrefixDiff.Replace("[PLACEHOLDER_PREFIX_SRC]", prefixSrc).Replace("[PLACEHOLDER_PREFIX_DST]", prefixDst);
        List<Patch> patches = PatchProcessor.CreatePatchesFromString(diffWithCutomPrefixes, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(1, patches.Count);
        Patch createdPatch = patches.First();

        ClassicAssert.AreEqual(@$"diff --git {prefixSrc}GitCommands/Patches/PatchProcessor.cs {prefixDst}GitCommands/Patches/PatchProcessor.cs", createdPatch.Header, "header");
        ClassicAssert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameA, "fileA");
        ClassicAssert.AreEqual("GitCommands/Patches/PatchProcessor.cs", createdPatch.FileNameB, "fileB");
        ClassicAssert.AreEqual("index 70b40..c1e6c 100644", createdPatch.Index);
        ClassicAssert.AreEqual(PatchChangeType.ChangeFile, createdPatch.ChangeType);
        ClassicAssert.AreEqual(PatchFileType.Text, createdPatch.FileType);
    }

    [Test]
    public void ColorBinDiff()
    {
        List<Patch> patches = PatchProcessor.CreatePatchesFromString(_colorBinDiff, new Lazy<Encoding>(() => Encoding.UTF8)).ToList();

        ClassicAssert.AreEqual(1, patches.Count);
        Patch createdPatch = patches.First();

        ClassicAssert.AreEqual(@"diff --git a/syscolor 3 gray.7z b/syscolor 3 gray.7z", createdPatch.Header, "header");
        ClassicAssert.AreEqual("syscolor 3 gray.7z", createdPatch.FileNameA, "fileA");
        ClassicAssert.AreEqual("syscolor 3 gray.7z", createdPatch.FileNameB, "fileB");
        ClassicAssert.AreEqual("index 33b006c1..00000000", createdPatch.Index);
        ClassicAssert.AreEqual(PatchChangeType.DeleteFile, createdPatch.ChangeType);
        ClassicAssert.AreEqual(PatchFileType.Binary, createdPatch.FileType);
    }

    private static TestPatch CreateSmallPatchExample(bool reverse = false)
    {
        string header = reverse
            ? "diff --git a/thisisatestb.txt b/thisisatesta.txt"
            : "diff --git b/thisisatesta.txt a/thisisatestb.txt";
        const string index = "index 5e4dce2..5eb1e6f 100644";
        const string fileNameA = "thisisatesta.txt";
        const string fileNameB = "thisisatestb.txt";

        StringBuilder patchText = new();
        StringBuilder patchOutput = new();

        AppendHeaderLine(header);
        AppendHeaderLine(index);

        if (reverse)
        {
            AppendHeaderLine("--- b/" + fileNameB);
            AppendHeaderLine("+++ a/" + fileNameA);
        }
        else
        {
            AppendHeaderLine("--- a/" + fileNameA);
            AppendHeaderLine("+++ b/" + fileNameB);
        }

        AppendDiffLine("@@ -1,2 +1,2 @@");
        AppendDiffLine(" iiiiii");
        AppendDiffLine("-ąśdkjaldskjlaksd");
        AppendDiffLine("+changed again€");

        Patch patch = new(header, index, PatchFileType.Text, fileNameA, fileNameB, PatchChangeType.ChangeFile, patchText.ToString());

        return new TestPatch(patch, patchOutput.ToString());

        void AppendHeaderLine(string line)
        {
            patchText.Append(line).Append("\n");
            patchOutput.Append(GitModule.ReEncodeString(line, GitModule.SystemEncoding, GitModule.LosslessEncoding));
            patchOutput.Append("\n");
        }

        void AppendDiffLine(string line)
        {
            patchText.Append(line).Append("\n");
            patchOutput.Append(GitModule.ReEncodeString(line, Encoding.UTF8, GitModule.LosslessEncoding));
            patchOutput.Append("\n");
        }
    }

    [Test]
    public async Task CreatePatchFromString_with_spaces()
    {
        string patchText = """
            diff --git a/sub modules/test submodule b/sub modules/test submodule
            --- a/sub modules/test submodule    
            +++ b/sub modules/test submodule    
            @@ -1 +1 @@
            -Subproject commit 4c54fbefd8032acb59aa33ade3fb4bdff32bdde7
            +Subproject commit 4c54fbefd8032acb59aa33ade3fb4bdff32bdde7-dirty
            """;
        await Verifier.Verify(PatchProcessor.CreatePatchesFromString(patchText, new Lazy<Encoding>(Encoding.UTF8)));
    }

    [TestCaseSource(typeof(CreatePatchFromStringTestData), nameof(CreatePatchFromStringTestData.TestCases))]
    public async Task CreatePatchFromString_Text_ChangeFile(Encoding filesContentEncoding)
    {
        const char escape = '\u001b';
        string patchText = $"""
            From 42a3043eafe08409c55b48c36661cf6cf3055c68 Mon Sep 17 00:00:00 2001
            From: Some One <else@mail.net>
            Date: Mon, 2 Sep 2024 17:42:00 +0200
            Subject: Patch for test

            ---
            {escape}[1mdiff --git a/old.txt b/new.txt{escape}[m
            {escape}[1mindex cb36533..5550a88 100644{escape}[m
            {escape}[1m--- a/old.txt{escape}[m
            {escape}[1m+++ b/new.txt{escape}[m
            {escape}[7;37m@@ -1 +1 @@{escape}[m
            {escape}[7;31m-Ð ÐÐÐÐ{escape}[m
            {escape}[7;32m+{escape}[m{escape}[7;32mA ÐÐÐÐ{escape}[m
            """.Replace("\r\n", "\n");
        await Verifier.Verify(PatchProcessor.CreatePatchesFromString(patchText, new Lazy<Encoding>(filesContentEncoding)));
    }

    public class CreatePatchFromStringTestData
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(Encoding.UTF8);
                yield return new TestCaseData(Encoding.ASCII);
                yield return new TestCaseData(Encoding.Latin1);
            }
        }
    }

    private sealed class TestPatch
    {
        public Patch Patch { get; }
        public string PatchOutput { get; }

        public TestPatch(Patch patch, string patchOutput)
        {
            Patch = patch;
            PatchOutput = patchOutput;
        }
    }
}
