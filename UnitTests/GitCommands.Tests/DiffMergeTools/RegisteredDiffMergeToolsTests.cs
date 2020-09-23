﻿using FluentAssertions;
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

            tools.Should().BeEquivalentTo("araxis", "bc", "bc3", "diffmerge", "kdiff3", "meld", "p4merge", "semanticmerge", "tortoisediff", "tortoisemerge", "vscode", "vsdiffmerge", "winmerge");
        }

        [Test]
        public void All_MergeTools()
        {
            var tools = RegisteredDiffMergeTools.All(DiffMergeToolType.Merge);

            tools.Should().BeEquivalentTo("araxis", "bc", "bc3", "diffmerge", "kdiff3", "meld", "p4merge", "semanticmerge", "tortoisediff", "tortoisemerge", "vscode", "vsdiffmerge", "winmerge");
        }
    }
}
