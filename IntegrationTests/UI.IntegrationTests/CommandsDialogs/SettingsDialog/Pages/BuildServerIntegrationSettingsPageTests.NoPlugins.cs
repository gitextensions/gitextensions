using CommonTestUtils;
using CommonTestUtils.MEF;
using GitCommands;
using GitExtensions.UITests;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;

namespace UITests.CommandsDialogs.SettingsDialog.Pages
{
    [Apartment(ApartmentState.STA)]
    public class BuildServerIntegrationSettingsPageTests_NoPlugins
    {
        // Created once for the fixture
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
            ExportProvider mefExportProvider = TestComposition.Empty.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _settingsPage.Dispose();
            _form.Dispose();
        }

        [Test]
        public void BuildServerType_should_contain_only_None_if_not_build_server_plugins_found()
        {
            RunFormTest(
              async form =>
              {
                  await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                  Assert.AreEqual(1, _settingsPage.GetTestAccessor().BuildServerType.Items.Count);
                  Assert.AreEqual(0, _settingsPage.GetTestAccessor().BuildServerType.SelectedIndex);
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
