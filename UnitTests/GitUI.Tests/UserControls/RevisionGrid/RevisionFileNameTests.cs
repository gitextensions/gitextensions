using System.ComponentModel.Design;
using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.RevisionGrid;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class RevisionFileNameTests
{
    private RevisionGridControl _revisionGridControl;
    private MockExecutable _executable;
    private IGitCommandRunner _commandRunner;

    [SetUp]
    public void SetUp()
    {
        _revisionGridControl = new RevisionGridControl();
        _executable = new MockExecutable();
        _commandRunner = new GitCommandRunner(_executable, () => GitModule.SystemEncoding);

        ServiceContainer serviceContainer = new();
        serviceContainer.AddService(Substitute.For<GitUI.Hotkey.IHotkeySettingsManager>());
        serviceContainer.AddService(Substitute.For<ResourceManager.IHotkeySettingsLoader>());

        IGitModule gitModule = Substitute.For<IGitModule>();
        gitModule.GitExecutable.Returns(_ => _executable);
        gitModule.GitCommandRunner.Returns(_ => _commandRunner);

        GitUICommands uiCommands = new(serviceContainer, gitModule);

        IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
        uiCommandsSource.UICommands.Returns(_ => uiCommands);

        _revisionGridControl.UICommandsSource = uiCommandsSource;
        _revisionGridControl.FilePathByObjectId = new();
    }

    [TestCase("17b2a8777e43dff588284c9661206c97ccb6cf8e", "a.txt")]
    [TestCase("83ec7c0a516f00b67aa9915d8e673a26510c2eed", "b.txt")]
    public void GetRevisionFileName_should_query_git_and_return_file_name(string objectId, string expectedFileName)
    {
        const string path = "a.txt";
        string actualFileName;
        string line = $"-c log.showSignature=false log --format=\"????%H\" --name-only --follow --diff-merges=separate  --find-renames --find-copies {objectId} --max-count=1 -- a.txt";

        bool originalFollowRenamesInFileHistoryExactOnly = AppSettings.FollowRenamesInFileHistoryExactOnly;
        AppSettings.FollowRenamesInFileHistoryExactOnly = false;

        try
        {
            using (_executable.StageOutput(line, $"????{objectId}\n{expectedFileName}"))
            {
                actualFileName = _revisionGridControl.GetRevisionFileName(path, ObjectId.Parse(objectId));
            }
        }
        finally
        {
            AppSettings.FollowRenamesInFileHistoryExactOnly = originalFollowRenamesInFileHistoryExactOnly;
        }

        Assert.AreEqual(expectedFileName, actualFileName);
    }
}
