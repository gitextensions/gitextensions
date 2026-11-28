using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitExtensions.UITests.CommandsDialogs;

[Apartment(ApartmentState.STA)]
public class SelectedTabPersistenceTests
{
    private const string SelectedTabSettingName = "CommitInfoTabControl_SelectedTab";

    private ReferenceRepository _referenceRepository = null!;
    private MemorySettings _settings = null!;
    private CommitInfoPosition _originalCommitInfoPosition;
    private bool _originalShowAuthorAvatarColumn;
    private bool _originalShowAvailableDiffTools;

    [SetUp]
    public void SetUp()
    {
        _originalCommitInfoPosition = AppSettings.CommitInfoPosition.Value;
        _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;
        _originalShowAvailableDiffTools = AppSettings.ShowAvailableDiffTools;

        AppSettings.CommitInfoPosition.Value = CommitInfoPosition.BelowList;

        // Stop loading custom diff tools
        AppSettings.ShowAvailableDiffTools = false;

        // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
        AppSettings.ShowAuthorAvatarColumn = false;

        _settings = new();
        _referenceRepository = new ReferenceRepository();
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.CommitInfoPosition.Value = _originalCommitInfoPosition;
        AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
        AppSettings.ShowAvailableDiffTools = _originalShowAvailableDiffTools;

        _referenceRepository.Dispose();
    }

    [TestCase("CommitInfoTabPage")]
    [TestCase("DiffTabPage")]
    [TestCase("TreeTabPage")]
    public void Selected_tab_should_be_saved_on_close_and_restored_on_start(string tabName)
    {
        RunFormTest(
            form =>
            {
                FullBleedTabControl tabControl = form.GetTestAccessor().CommitInfoTabControl;
                tabControl.SelectedTab = tabControl.TabPages[tabName];
            });

        _settings.GetString(SelectedTabSettingName, defaultValue: null).Should().Be(tabName);

        RunFormTest(form => form.GetTestAccessor().CommitInfoTabControl.SelectedTab!.Name.Should().Be(tabName));
    }

    [TestCase(CommitInfoPosition.BelowList, null, "CommitInfoTabPage")]
    [TestCase(CommitInfoPosition.BelowList, "GpgInfoTabPage", "CommitInfoTabPage")]
    [TestCase(CommitInfoPosition.BelowList, "UnknownTabPage", "CommitInfoTabPage")]
    [TestCase(CommitInfoPosition.RightwardFromList, "CommitInfoTabPage", "DiffTabPage")]
    [TestCase(CommitInfoPosition.RightwardFromList, "TreeTabPage", "TreeTabPage")]
    public void Start_should_select_the_default_tab_if_the_saved_tab_cannot_be_restored(CommitInfoPosition commitInfoPosition, string? savedTabName, string expectedTabName)
    {
        AppSettings.CommitInfoPosition.Value = commitInfoPosition;
        _settings.SetString(SelectedTabSettingName, savedTabName);

        RunFormTest(form => form.GetTestAccessor().CommitInfoTabControl.SelectedTab!.Name.Should().Be(expectedTabName));
    }

    private void RunFormTest(Action<FormBrowse> testDriver)
    {
        UITest.RunForm(
            showForm: () =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                GitUICommands commands = new(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
                FormBrowse form = new(commands, new BrowseArguments(), _settings);
#pragma warning restore CS0618 // Type or member is obsolete

                form.GetGitExtensionsFormTestAccessor().WindowPositionManager = Substitute.For<IWindowPositionManager>();

                if (Application.MessageLoop)
                {
                    form.Show(owner: null);
                }
                else
                {
                    Application.Run(form);
                }
            },
            (FormBrowse form) =>
            {
                UITest.ProcessUntil("loading revisions", () => form.GetTestAccessor().RevisionGrid.GetTestAccessor().IsDataLoadComplete, maxMilliseconds: 10_000);
                testDriver(form);
                return Task.CompletedTask;
            });
    }
}
