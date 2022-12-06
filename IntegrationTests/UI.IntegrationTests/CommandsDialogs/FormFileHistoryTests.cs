using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormFileHistoryTests
{
    [SetUp]
    public void SetUp()
    {
        AppSettings.UseBrowseForFileHistory.Value = false;

        TestComposition composition = TestComposition.Empty
            .AddParts(typeof(MockLinkFactory))
            .AddParts(typeof(MockWindowsJumpListManager))
            .AddParts(typeof(MockRepositoryDescriptionProvider))
            .AddParts(typeof(MockAppTitleGenerator));

        ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
        ManagedExtensibility.SetTestExportProvider(mefExportProvider);
    }

    [TestCase("", "file.txt")]
    [TestCase("", "file with spaces.txt")]
    [TestCase("Dir with spaces", "file.txt")]
    [TestCase("Dir with spaces", "file with spaces.txt")]
    public void File_history_should_behave_as_expected(string fileRelativePath, string fileName)
    {
        using ReferenceRepository referenceRepository = new();
        GitUICommands commands = new(referenceRepository.Module);

        string revision1 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Create '{fileName}' in directory '{fileRelativePath}'");
        string revision2 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}'");
        string revision3 = referenceRepository.CreateCommitRelative(fileRelativePath, fileName, $"Update '{fileName}' in directory '{fileRelativePath}' again");

        RunFormTest(
            form =>
            {
                WaitForRevisionsToBeLoaded(form);
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision1)).Should().NotBeNull();
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision2)).Should().NotBeNull();
                form.GetTestAccessor().RevisionGrid.GetRevision(ObjectId.Parse(revision3)).Should().NotBeNull();
            }, Path.Combine(fileRelativePath, fileName), commands);
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
