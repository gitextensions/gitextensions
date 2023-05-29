using System.Reflection;
using CommonTestUtils;
using GitCommands;
using GitCommands.Settings;

namespace GitCommandsTests.Helpers
{
    [TestFixture]
    public class AutoCrlfExtensionsTests
    {
        [Test]
        [TestCase(AutoCRLFType.@true, "UnixLines")]
        [TestCase(AutoCRLFType.@true, "MacLines")]
        [TestCase(AutoCRLFType.@true, "WindowsLines")]
        [TestCase(AutoCRLFType.@false, "UnixLines")]
        [TestCase(AutoCRLFType.@false, "MacLines")]
        [TestCase(AutoCRLFType.@false, "WindowsLines")]
        [TestCase(AutoCRLFType.input, "UnixLines")]
        [TestCase(AutoCRLFType.input, "MacLines")]
        [TestCase(AutoCRLFType.input, "WindowsLines")]
        public async Task DoAutoCRLF_should_not_unnecessarily_duplicate_line_ending(AutoCRLFType autoCRLFType, string file)
        {
            string content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{file}.bin");

            await Verifier.Verify(content.AdjustLineEndings(autoCRLFType));
        }
    }
}
