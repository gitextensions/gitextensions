using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormInitTests
{
    // Created once for the fixture
    private ReferenceRepository _referenceRepository = null!;

    // Created once for each test
    private GitUICommands _commands = null!;

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
                form.GetTestAccessor().DirectoryCombo.Text.Should().Be(currentDir);
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
                form.GetTestAccessor().DirectoryCombo.Text.Should().Be(_referenceRepository.Module.WorkingDir);
            },
            null!);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("    ")]
    [TestCase(@"foo\bar")]
    public void IsRootedDirectoryPath_should_detect_invalid_paths(string? input)
    {
        string currentDir = "bla";
        RunFormTest(
            form =>
            {
                form.GetTestAccessor().IsRootedDirectoryPath(input!).Should().BeFalse();
            },
            currentDir);
    }

    [TestCase(@"c:\foo\bar")]
    [TestCase(@"c:\foo\bar\")]
    [TestCase(@"c:")]
    [TestCase(@"  c:\foo\bar  ")]
    public void IsRootedDirectoryPath_returns_true_on_valid_paths(string? input)
    {
        string currentDir = "bla";
        RunFormTest(
            form =>
            {
                form.GetTestAccessor().IsRootedDirectoryPath(input!).Should().BeTrue();
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
                _commands.StartInitializeDialog(owner: null, path).Should().BeTrue();
            },
            testDriverAsync);
    }
}
