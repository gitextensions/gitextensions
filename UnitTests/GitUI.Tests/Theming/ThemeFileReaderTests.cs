using System.IO;
using FluentAssertions;
using GitUI.Theming;
using NUnit.Framework;

namespace GitUITests.Theming
{
    [TestFixture]
    public class ThemeFileReaderTests
    {
        [Test]
        public void Should_return_file_text()
        {
            const string mockThemeContent = "test content";

            ThemeFileReader reader = new();

            var tempPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempPath, mockThemeContent);
                reader.ReadThemeFile(tempPath).Should().Be(mockThemeContent);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        [Test]
        public void Should_throw_ThemeException_if_file_does_not_exist()
        {
            string nonExistingFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "non_existing_theme.css");

            ThemeFileReader reader = new();
            reader.Invoking(_ => _.ReadThemeFile(nonExistingFile))
                .Should().Throw<ThemeException>()
                .Which.InnerException.Should().BeOfType<FileNotFoundException>();
        }
    }
}
