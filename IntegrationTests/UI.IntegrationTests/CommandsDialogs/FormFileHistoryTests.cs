using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormFileHistoryTests
{
    private ReferenceRepository _referenceRepository;

    private GitUICommands _commands;

    [SetUp]
    public void SetUp()
    {
        ReferenceRepository.ResetRepo(ref _referenceRepository);
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);

        AppSettings.UseBrowseForFileHistory.Value = false;
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _referenceRepository.Dispose();
    }

    [TestCase("", "file.txt")]
    [TestCase("", "file with spaces.txt")]
    [TestCase("Dir with spaces", "file.txt")]
    [TestCase("Dir with spaces", "file with spaces.txt")]
    public void File_history_should_behave_as_expected(string fileRelativePath, string fileName)
    {
        string revision1 = _referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Create '{fileName}' in directory '{fileRelativePath}'");
        string revision2 = _referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}'");
        string revision3 = _referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}' again");

        RunFormTest(
            form =>
            {
                WaitForRevisionsToBeLoaded(form);
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision1)).Should().NotBeNull();
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision2)).Should().NotBeNull();
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision3)).Should().NotBeNull();
            }, Path.Combine(fileRelativePath, fileName), _commands);
    }

    private static void RunFormTest(Action<FormFileHistory> testDriver, string fileName, GitUICommands commands)
    {
        UITest.RunForm(
            showForm: () => commands.GetTestAccessor().ShowFileHistoryDialog(fileName),
            (FormFileHistory form) =>
            {
                testDriver(form);

                return Task.CompletedTask;
            });
    }

    private static void WaitForRevisionsToBeLoaded(FormFileHistory form)
    {
        UITest.ProcessUntil("Loading Revisions", () => form.GetTestAccessor().RevisionGrid.GetTestAccessor().IsDataLoadComplete);
    }
}
