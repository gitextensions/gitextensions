using CommonTestUtils;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUITests.UserControls;

public sealed class GitBlameParserTest
{
    // Track the original setting value
    private bool _useHistogramDiffAlgorithm;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _useHistogramDiffAlgorithm = AppSettings.UseHistogramDiffAlgorithm;
        AppSettings.UseHistogramDiffAlgorithm = false;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        AppSettings.UseHistogramDiffAlgorithm = _useHistogramDiffAlgorithm;
    }

    [Test]
    public void GetOriginalLineInPreviousCommit()
    {
        // Simulate "Blame previous revision" on line 2281
        // when blaming file FormBrowse.cs on commit 7edc265403b54dd8ea2b8402f8790219d8d42077
        // line content is: internal bool ExecuteCommand(Command cmd)

        string gitExtensionsRepoPath = Path.GetFullPath(@"..\..\..\..\..\..\..");

        GitModule gitModule = new(gitExtensionsRepoPath);

        // Revision corresponding to line 2281
        GitRevision selectedBlamedRevision = gitModule.GetRevision(ObjectId.Parse("52476f30670ba5338756b606841fb0a346fd6460"));

        // line content is still: internal bool ExecuteCommand(Command cmd)
        Assert.AreEqual(2192, GetOriginalLineInPreviousCommit(2281));

        // line content from offset in diff
        //   case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
        //  ->
        //   case Commands.QuickPull: UICommands.StartPullDialogAndPullImmediately(this); break;
        Assert.AreEqual(2147, GetOriginalLineInPreviousCommit(2187));

        // line content is still around: internal enum Command -> internal enum Commands
        Assert.AreEqual(2085, GetOriginalLineInPreviousCommit(2090));

        int GetOriginalLineInPreviousCommit(int blamedLineNumber)
            => new GitBlameParser(() => gitModule)
                    .GetOriginalLineInPreviousCommit(selectedBlamedRevision, "GitUI/CommandsDialogs/FormBrowse.cs", blamedLineNumber);
    }
}
