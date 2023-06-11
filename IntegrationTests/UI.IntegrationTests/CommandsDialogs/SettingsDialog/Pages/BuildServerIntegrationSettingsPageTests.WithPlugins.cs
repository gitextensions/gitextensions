using CommonTestUtils;
using CommonTestUtils.MEF;
using GitCommands;
using GitExtensions.UITests;
using GitExtensions.UITests.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Composition;

namespace UITests.CommandsDialogs.SettingsDialog.Pages
{
    [Apartment(ApartmentState.STA)]
    public class BuildServerIntegrationSettingsPageTests_WithPlugins
    {
        private ReferenceRepository _referenceRepository;
        private MockHost _form;
        private BuildServerIntegrationSettingsPage _settingsPage;

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            var composition = TestComposition.Empty
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
        }

        [Test]
        public void BuildServerType_should_contain_discovered_build_server_plugins()
        {
            RunFormTest(
              async form =>
              {
                  await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                  Assert.AreEqual(/* default None + GenericBuildServerMock */2, _settingsPage.GetTestAccessor().BuildServerType.Items.Count);
                  Assert.AreEqual(0, _settingsPage.GetTestAccessor().BuildServerType.SelectedIndex);
                  Assert.AreEqual("GenericBuildServerMock", _settingsPage.GetTestAccessor().BuildServerType.Items[1]);
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
                  Assert.AreEqual(0, _settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls.Count);

                  // Select the custom build server
                  _settingsPage.GetTestAccessor().BuildServerType.SelectedIndex = 1;
                  Assert.AreEqual(1, _settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls.Count);
                  Assert.IsInstanceOf<IBuildServerSettingsUserControl>(_settingsPage.GetTestAccessor().buildServerSettingsPanel.Controls[0]);
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

                    _settingsPage = SettingsPageBase.Create<BuildServerIntegrationSettingsPage>(_form);
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
                throw new System.NotImplementedException();
            }

            public void LoadAll()
            {
                throw new System.NotImplementedException();
            }

            public void SaveAll()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
