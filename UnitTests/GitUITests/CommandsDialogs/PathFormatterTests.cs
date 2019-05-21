using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    public class PathFormatterTests
    {
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
        public void Test_FormatTextForFileNameOnly(string name, string oldName, string expectedText, string expectedSuffix)
        {
            PathFormatter.FormatTextForFileNameOnly(name, oldName).Should().Be((expectedText, expectedSuffix));
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("filename.ext", " (filename.ext)")]
        [TestCase("/filename.ext", " (/filename.ext)")]
        [TestCase("path/filename.ext", " (path/filename.ext)")]
        [TestCase("nested/path/filename.ext", " (nested/path/filename.ext)")]
        [TestCase("/nested/path/filename.ext", " (/nested/path/filename.ext)")]
        [TestCase("path\\filename.ext", " (path\\filename.ext)")]
        public void Test_FormatOldName(string oldName, string expectedSuffix)
        {
            PathFormatter.TestAccessor.FormatOldName(oldName).Should().Be(expectedSuffix);
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
        public void Test_SplitPathName(string name, string expectedPath, string expectedFileName)
        {
            PathFormatter.TestAccessor.SplitPathName(name).Should().Be((expectedPath, expectedFileName));
        }
    }
}