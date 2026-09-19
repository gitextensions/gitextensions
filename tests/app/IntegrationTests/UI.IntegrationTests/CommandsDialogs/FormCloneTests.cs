using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class FormCloneTests
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

    [TestCase(null, false, "")]
    [TestCase("", false, "")]
    [TestCase(" ", false, "")]
    [TestCase("blah", false, "")]
    [TestCase("git clone https://github.com/gitextensions/gitextensions && cd gitextensions", true, "https://github.com/gitextensions/gitextensions")]
    [TestCase("git clone ssh://username@gerrit-server:/PROJECT", true, "ssh://username@gerrit-server:/PROJECT")]
    [TestCase("git clone https://github.com/gitextensions/gitextensions && git clone https://github.com/gitextensions/git.hub", true, "https://github.com/gitextensions/gitextensions")]
    public void Test_Url_extract_from_string(
        string? text, bool expected, string expectedUrl)
    {
        RunFormTest(
            form =>
            {
                FormClone.TestAccessor accessor = form.GetTestAccessor();

                accessor.TryExtractUrl(text!, out string url).Should().Be(expected);

                // No need to compare URL if the result was expected to be false
                if (expected)
                {
                    url.Should().Be(expectedUrl);
                }
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Initializing_the_submodules_must_be_preset_as_it_was_chosen_before(bool initializeSubmodules)
    {
        bool originalSetting = AppSettings.CloneInitializeAllSubmodules;

        try
        {
            AppSettings.CloneInitializeAllSubmodules = initializeSubmodules;

            RunFormTest(form => form.GetTestAccessor().InitializeAllSubmodules.Checked.Should().Be(initializeSubmodules));
        }
        finally
        {
            AppSettings.CloneInitializeAllSubmodules = originalSetting;
        }
    }

    private void RunFormTest(Action<FormClone> testDriver)
    {
        RunFormTest(
            form =>
            {
                testDriver(form);
                return Task.CompletedTask;
            });
    }

    private void RunFormTest(Func<FormClone, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _commands.StartCloneDialog(owner: null, url: null);
            },
            testDriverAsync);
    }
}
