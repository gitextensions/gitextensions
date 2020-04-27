using System.Reflection;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using CommonTestUtils;
using GitCommands;
using GitCommands.Settings;
using NUnit.Framework;

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
        [IgnoreLineEndings(false)]
        public void DoAutoCRLF_should_not_unnecessarily_duplicate_line_ending(AutoCRLFType autoCRLFType, string file)
        {
            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{file}.bin");

            using (ApprovalResults.ForScenario(file, autoCRLFType))
            {
                Approvals.Verify(content.AdjustLineEndings(autoCRLFType));
            }
        }
    }
}
