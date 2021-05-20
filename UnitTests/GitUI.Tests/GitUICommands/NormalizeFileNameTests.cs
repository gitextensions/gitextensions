using FluentAssertions;
using GitCommands;
using GitUI;
using NUnit.Framework;

namespace GitUITests.GitUICommandsTests
{
    [TestFixture]
    public sealed class NormalizeFileNameTests
    {
        [TestCase(@"file", "file")]
        [TestCase(@"file.ext", "file.ext")]
        [TestCase(@"path\file.ext", "path/file.ext")]
        [TestCase(@"path/file.ext", "path/file.ext")]
        [TestCase(@"c:\path\file.ext", "c:/path/file.ext")]
        [TestCase(@"c:\path/file.ext", "c:/path/file.ext")]
        [TestCase(@"c:/path/file.ext", "c:/path/file.ext")]
        [TestCase(@"c:\working\dir\path\file.ext", "path/file.ext")]
        [TestCase(@"c:/working/dir/path/file.ext", "path/file.ext")]
        [TestCase(@"C:\working\dir\path\file.ext", "C:/working/dir/path/file.ext")]
        [TestCase(@"C:/working/dir/path/file.ext", "C:/working/dir/path/file.ext")]
        public void NormalizeFileNameTest(string fileName, string expected)
        {
            GitModule module = new(@"c:\working\dir");
            GitUICommands commands = new(module);

            commands.GetTestAccessor().NormalizeFileName(fileName).Should().Be(expected);
        }
    }
}
