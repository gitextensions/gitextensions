using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormInitTests
{
    // Created once for the fixture
    private ReferenceRepository _referenceRepository;

    // Created once for each test
    private GitUICommands _commands;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
    }

    [TearDown]
    public void TearDown()
    {
        _referenceRepository.Dispose();
    }

    [Test]
    public void Should_show_supplied_path()
    {
        string currentDir = "bla";
        RunFormTest(
            form =>
            {
                ClassicAssert.AreEqual(currentDir, form.GetTestAccessor().DirectoryCombo.Text);
            },
            currentDir);
    }

    // Strictly speaking this is a test for GitUICommands.StartInitializeDialog
    [Test]
    public void Should_show_current_GitModuleWorkingDir_if_supplied_path_null()
    {
        RunFormTest(
            form =>
            {
                ClassicAssert.AreEqual(_referenceRepository.Module.WorkingDir, form.GetTestAccessor().DirectoryCombo.Text);
            },
            null);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("    ")]
    [TestCase(@"foo\bar")]
    public void IsRootedDirectoryPath_should_detect_invalid_paths(string input)
    {
        string currentDir = "bla";
        RunFormTest(
            form =>
            {
                ClassicAssert.IsFalse(form.GetTestAccessor().IsRootedDirectoryPath(input));
            },
            currentDir);
    }

    [TestCase(@"c:\foo\bar")]
    [TestCase(@"c:\foo\bar\")]
    [TestCase(@"c:")]
    [TestCase(@"  c:\foo\bar  ")]
    public void IsRootedDirectoryPath_returns_true_on_valid_paths(string input)
    {
        string currentDir = "bla";
        RunFormTest(
            form =>
            {
                ClassicAssert.IsTrue(form.GetTestAccessor().IsRootedDirectoryPath(input));
            },
            currentDir);
    }

    private void RunFormTest(Action<FormInit> testDriver, string path)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            },
            path);
    }

    private void RunFormTest(Func<FormInit, Task> testDriverAsync, string path)
    {
        UITest.RunForm(
            () =>
            {
                ClassicAssert.True(_commands.StartInitializeDialog(owner: null, path));
            },
            testDriverAsync);
    }
}
