using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUITests.UserControls;

public sealed class GitBlameParserTest
{
    [Test]
    public void GetOriginalLineInPreviousCommit()
    {
        // Simulate "Blame previous revision" on line 2281
        // when blaming file FormBrowse.cs on commit 7edc265403b54dd8ea2b8402f8790219d8d42077
        // line content is: internal bool ExecuteCommand(Command cmd)

        string gitExtensionsRepoPath = Path.GetFullPath(@"..\..\..\..\..\..\..");
        Assert.IsTrue(gitExtensionsRepoPath.EndsWith("gitextensions"));

        GitModule gitModule = new(gitExtensionsRepoPath);

        // Revision corresponding to line 2281
        GitRevision selectedBlamedRevision = gitModule.GetRevision(ObjectId.Parse("52476f30670ba5338756b606841fb0a346fd6460"));

        int matchingBlameLineNumber = new GitBlameParser(() => gitModule)
            .GetOriginalLineInPreviousCommit(selectedBlamedRevision, "GitUI/CommandsDialogs/FormBrowse.cs", 2281);

        Assert.AreEqual(2192, matchingBlameLineNumber); // line content is still: internal bool ExecuteCommand(Command cmd)
    }
}
