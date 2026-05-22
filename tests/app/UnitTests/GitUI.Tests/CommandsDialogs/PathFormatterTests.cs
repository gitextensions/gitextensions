using GitCommands;
using GitUI;

namespace GitUITests.CommandsDialogs;

public class PathFormatterTests
{
    private Bitmap _bitmap = null!;
    private Graphics _graphics = null!;
    private Font _font = null!;
    private TruncatePathMethod _originalTruncatePathMethod;

    [SetUp]
    public void SetUp()
    {
        _bitmap = new Bitmap(1, 1);
        _graphics = Graphics.FromImage(_bitmap);
        _font = SystemFonts.DefaultFont;
        _originalTruncatePathMethod = AppSettings.TruncatePathMethod;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.TruncatePathMethod = _originalTruncatePathMethod;
        _graphics.Dispose();
        _bitmap.Dispose();
    }

    [TestCase("name.ext", null, TruncatePathMethod.None, int.MaxValue, null, "name.ext", null)]
    [TestCase("path/name.ext", null, TruncatePathMethod.None, int.MaxValue, "path/", "name.ext", null)]
    [TestCase("name.ext", "old.ext", TruncatePathMethod.None, int.MaxValue, null, "name.ext", " (old.ext)")]
    [TestCase("path/name.ext", "old/old.ext", TruncatePathMethod.None, int.MaxValue, "path/", "name.ext", " (old/old.ext)")]
    [TestCase("path/name.ext", "old/old.ext", TruncatePathMethod.TrimStart, int.MaxValue, "path/", "name.ext", " (old/old.ext)")]
    [TestCase("path/name.ext", "old/old.ext", TruncatePathMethod.TrimStart, 1, null, "...", null)]
    [TestCase("path/name.ext", "old/old.ext", TruncatePathMethod.TrimStart, 18, null, "...", null)]
    [TestCase("name.ext", null, TruncatePathMethod.FileNameOnly, int.MaxValue, null, "name.ext", null)]
    [TestCase("path/name.ext", null, TruncatePathMethod.FileNameOnly, int.MaxValue, null, "name.ext", null)]
    [TestCase("path/name.ext", "old/old.ext", TruncatePathMethod.FileNameOnly, int.MaxValue, null, "name.ext", " (old.ext)")]
    public void FormatTextForDrawing_returns_expected_for_method(
        string name,
        string? oldName,
        TruncatePathMethod truncatePathMethod,
        int maxWidth,
        string? expectedPrefix,
        string? expectedText,
        string? expectedSuffix)
    {
        AppSettings.TruncatePathMethod = truncatePathMethod;
        PathFormatter formatter = new(_graphics, _font);

        formatter.FormatTextForDrawing(maxWidth, name, oldName).Should().Be((expectedPrefix, expectedText, expectedSuffix));
    }

    [Test]
    public void FormatTextForDrawing_TrimStart_with_empty_name_returns_null_prefix_empty_text_and_oldName_suffix()
    {
        // When name is empty and oldName differs, even a low maxWidth accommodates
        // the suffix-only result produced at step 0 of the binary search.
        const string oldName = "b";
        AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
        PathFormatter formatter = new(_graphics, _font);
        int maxWidth = formatter.MeasureString(null, string.Empty, $" ({oldName})").Width;

        formatter.FormatTextForDrawing(maxWidth, string.Empty, oldName).Should().Be((null, string.Empty, $" ({oldName})"));
    }

    [Test]
    public void FormatTextForDrawing_TrimStart_non_empty_name_never_yields_empty_text()
    {
        // Even with maxWidth so small that nothing fits, a non-empty name must still
        // produce at least "..." so the caller always has something to display.
        AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
        PathFormatter formatter = new(_graphics, _font);

        (string? prefix, string? text, string? suffix) = formatter.FormatTextForDrawing(maxWidth: 0, name: "path/file.ext", oldName: null);

        prefix.Should().BeNull();
        text.Should().Be("...");
        suffix.Should().BeNull();
    }

    [Test]
    public void FormatTextForDrawing_TrimStart_truncates_suffix_before_text()
    {
        // FormatString must consume all oldName chars before touching name chars.
        // Check that at every step up to oldName.Length the text is unchanged,
        // and only beyond that does text start to shrink.
        const string name = "file.ext";
        const string oldName = "very_long_old_name.ext";
        AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;

        for (int step = 0; step <= oldName.Length; step++)
        {
            (string? prefix, string? text, string? suffix) = PathFormatter.TestAccessor.FormatString(name, oldName, step);

            // Name must stay fully intact while only the suffix is being consumed.
            prefix.Should().BeNull($"step={step}");
            text.Should().Be(name, $"step={step}");
        }

        // One step beyond the full oldName length: name must start shrinking.
        (string? prefixAfter, string? textAfter, string? suffixAfter) = PathFormatter.TestAccessor.FormatString(name, oldName, oldName.Length + 1);

        textAfter.Should().NotBe(name, "name must start shrinking once oldName is fully consumed");
        suffixAfter.Should().Be(" (...)", "with TrimStart the maximally-truncated oldName is still shown as '...'");
    }

    [Test]
    public void FormatTextForDrawing_TrimStart_truncates_name_independently_of_oldName()
    {
        // Verify name and oldName are truncated by their respective counters (not swapped).
        // At step 0 both strings appear unmodified.
        const string name = "path/file.ext";
        const string oldName = "oldpath/oldfile.ext";
        AppSettings.TruncatePathMethod = TruncatePathMethod.TrimStart;
        PathFormatter formatter = new(_graphics, _font);
        int maxWidth = formatter.MeasureString("path/", "file.ext", $" ({oldName})").Width;

        (string? prefix, string? text, string? suffix) = formatter.FormatTextForDrawing(maxWidth, name, oldName);

        prefix.Should().Be("path/");
        text.Should().Be("file.ext");
        suffix.Should().Be($" ({oldName})");
    }

    [TestCase("new.ext", null, "new.ext", null)]
    [TestCase("new.ext", "", "new.ext", null)]
    [TestCase("path/new.ext", null, "new.ext", null)]
    [TestCase("/path/new.ext", null, "new.ext", null)]
    [TestCase("C:/path/new.ext", null, "new.ext", null)]
    [TestCase("path\\new.ext", null, "new.ext", null)]
    [TestCase("C:\\path\\new.ext", null, "new.ext", null)]
    [TestCase("path/new.ext", "old.ext", "new.ext", " (old.ext)")]
    [TestCase("path/new.ext", "oldPath/old.ext", "new.ext", " (old.ext)")]
    [TestCase("path/new.ext", "/oldPath/old.ext", "new.ext", " (old.ext)")]
    [TestCase("path/new.ext", "C:/oldPath/old.ext", "new.ext", " (old.ext)")]
    [TestCase("path/new.ext", "C:\\oldPath\\old.ext", "new.ext", " (old.ext)")]
    [TestCase("path/new.ext", "oldPath\\old.ext", "new.ext", " (old.ext)")]
    public void Test_FormatTextForFileNameOnly(string? name, string? oldName, string expectedText, string? expectedSuffix)
    {
        PathFormatter.FormatTextForFileNameOnly(name!, oldName).Should().Be((expectedText, expectedSuffix));
    }

    [TestCase(null, null)]
    [TestCase("", null)]
    [TestCase("filename.ext", " (filename.ext)")]
    [TestCase("/filename.ext", " (/filename.ext)")]
    [TestCase("path/filename.ext", " (path/filename.ext)")]
    [TestCase("nested/path/filename.ext", " (nested/path/filename.ext)")]
    [TestCase("/nested/path/filename.ext", " (/nested/path/filename.ext)")]
    [TestCase("path\\filename.ext", " (path\\filename.ext)")]
    public void Test_FormatOldName(string? oldName, string? expectedSuffix)
    {
        PathFormatter.TestAccessor.FormatOldName(oldName!).Should().Be(expectedSuffix);
    }

    [TestCase(null, null, null)]
    [TestCase("", null, "")]
    [TestCase(" ", null, " ")]
    [TestCase("filename.ext", null, "filename.ext")]
    [TestCase("/filename.ext", "/", "filename.ext")]
    [TestCase("path/filename.ext", "path/", "filename.ext")]
    [TestCase("nested/path/filename.ext", "nested/path/", "filename.ext")]
    [TestCase("/nested/path/filename.ext", "/nested/path/", "filename.ext")]
    [TestCase("path\\filename.ext", null, "path\\filename.ext")]
    [TestCase("path\\submodule.dir\\", null, "path\\submodule.dir\\")]
    [TestCase("/", null, "/")]
    [TestCase("submodule.dir/", null, "submodule.dir/")]
    [TestCase("/submodule.dir/", "/", "submodule.dir/")]
    [TestCase("path/submodule.dir/", "path/", "submodule.dir/")]
    [TestCase("/path/submodule.dir/", "/path/", "submodule.dir/")]
    public void Test_SplitPathName(string? name, string? expectedPath, string? expectedFileName)
    {
        PathFormatter.TestAccessor.SplitPathName(name!).Should().Be((expectedPath, expectedFileName));
    }
}
