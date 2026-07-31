using CommonTestUtils;
using CommonTestUtils.MEF;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.UITests;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;

namespace UITests.CommandsDialogs.SettingsDialog.Pages;

[Apartment(ApartmentState.STA)]
public class AiFilterSettingsPageTests
{
    private ReferenceRepository _referenceRepository = null!;
    private MockHost _form = null!;
    private AiFilterSettingsPage _settingsPage = null!;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        ExportProvider mefExportProvider = TestComposition.Empty.ExportProviderFactory.CreateExportProvider();
        ManagedExtensibility.SetTestExportProvider(mefExportProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _settingsPage.Dispose();
        _form.Dispose();
        _referenceRepository.Dispose();
    }

    [Test]
    public void Input_controls_should_have_non_zero_size_when_shown()
    {
        RunFormTest(
            async form =>
            {
                await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                AiFilterSettingsPage.TestAccessor accessor = _settingsPage.GetTestAccessor();

                // Regression test: the inputs used to collapse to zero width inside an AutoSize FlowLayoutPanel,
                // leaving only the group borders visible.
                accessor.Backend.Width.Should().BeGreaterThan(0);
                accessor.Backend.Height.Should().BeGreaterThan(0);
                accessor.Endpoint.Width.Should().BeGreaterThan(0);
                accessor.Model.Width.Should().BeGreaterThan(0);
                accessor.ApiKey.Width.Should().BeGreaterThan(0);
                accessor.ClaudeCodeExecutable.Width.Should().BeGreaterThan(0);
                accessor.Imports.Width.Should().BeGreaterThan(0);
            });
    }

    private void RunFormTest(Func<MockHost, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _form = new MockHost(_referenceRepository.Module)
                {
                    Size = new(800, 600)
                };

                _settingsPage = SettingsPageBase.Create<AiFilterSettingsPage>(_form, GitUICommands.EmptyServiceProvider);
                _settingsPage.Dock = DockStyle.Fill;

                _form.Controls.Add(_settingsPage);

                _form.ShowDialog(owner: null);
            },
            testDriverAsync);
    }

    private class MockHost : Form, ISettingsPageHost
    {
        public MockHost(GitModule module)
        {
            CheckSettingsLogic = new(new(module));
        }

        public CheckSettingsLogic CheckSettingsLogic { get; }

        public void GotoPage(SettingsPageReference settingsPageReference)
        {
            throw new NotImplementedException();
        }

        public void LoadAll()
        {
            throw new NotImplementedException();
        }

        public void SaveAll()
        {
            throw new NotImplementedException();
        }
    }
}
