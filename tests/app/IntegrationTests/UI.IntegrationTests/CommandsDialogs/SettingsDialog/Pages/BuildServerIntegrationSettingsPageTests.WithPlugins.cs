using CommonTestUtils;
using CommonTestUtils.MEF;
using GitCommands;
using GitExtensions.Extensibility.Settings;
using GitExtensions.UITests;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Composition;

namespace UITests.CommandsDialogs.SettingsDialog.Pages;

[Apartment(ApartmentState.STA)]
public class BuildServerIntegrationSettingsPageTests_WithPlugins
{
    private ReferenceRepository _referenceRepository;
    private MockHost _form;
    private BuildServerIntegrationSettingsPage _settingsPage;

    [SetUp]
    public void SetUp()
    {
        _referenceRepository = new ReferenceRepository();
        TestComposition composition = TestComposition.Empty
            .AddParts(typeof(MockGenericBuildServerAdapter))
            .AddParts(typeof(MockGenericBuildServerSettingsUserControl));
        ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
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
    public void BuildServerType_should_contain_discovered_build_server_plugins()
    {
        RunFormTest(
          async form =>
          {
              await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

              ClassicAssert.AreEqual(/* default None + GenericBuildServerMock */2, _settingsPage.GetTestAccessor().BuildServerType.Items.Count);
              ClassicAssert.AreEqual(0, _settingsPage.GetTestAccessor().BuildServerType.SelectedIndex);
              ClassicAssert.AreEqual("GenericBuildServerMock", _settingsPage.GetTestAccessor().BuildServerType.Items[1]);
          });
    }

    [Test]
    public void BuildServerType_should_toggle_plugin_settings_controls()
    {
        RunFormTest(
          async form =>
          {
              await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

              // Default option, no custom control
              ClassicAssert.AreEqual(0, _settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls.Count);

              // Select the custom build server
              _settingsPage.GetTestAccessor().BuildServerType.SelectedIndex = 1;
              ClassicAssert.AreEqual(1, _settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls.Count);
              ClassicAssert.IsInstanceOf<IBuildServerSettingsUserControl>(_settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls[0]);
          });
    }

    private void RunFormTest(Func<MockHost, Task> testDriverAsync)
    {
        UITest.RunForm(
            () =>
            {
                _form = new MockHost(_referenceRepository.Module)
                {
                    Size = new(800, 400)
                };

                _settingsPage = SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(_form, GitUICommands.EmptyServiceProvider);
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
