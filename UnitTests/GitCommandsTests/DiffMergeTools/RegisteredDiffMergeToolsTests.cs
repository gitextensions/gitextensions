using FluentAssertions;
using GitCommands.DiffMergeTools;
using NUnit.Framework;

namespace GitCommandsTests.DiffMergeTools
{
    [TestFixture]
    public class RegisteredDiffMergeToolsTests
    {
        [Test]
        public void All_DiffTools()
        {
            var tools = RegisteredDiffMergeTools.All(DiffMergeToolType.Diff);

            tools.Should().BeEquivalentTo("bc", "bc3", "diffmerge", "kdiff3", "meld", "p4merge", "semanticmerge", "tortoisemerge", "vscode", "vsdiffmerge", "winmerge");
        }

        [Test]
        public void All_MergeTools()
        {
            var tools = RegisteredDiffMergeTools.All(DiffMergeToolType.Merge);

            tools.Should().BeEquivalentTo("bc", "bc3", "diffmerge", "kdiff3", "meld", "p4merge", "semanticmerge", "tortoisemerge", "vscode", "vsdiffmerge", "winmerge");
        }
    }
}
