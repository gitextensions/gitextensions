﻿using CommonTestUtils;
using CommonTestUtils.MEF;
using FluentAssertions;
using GitCommands;
using GitCommands.Submodules;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Composition;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class FormBrowse_LeftPanel_SubmodulesTests
    {
        // Created once for each test
        private GitUICommands _commands;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

        private GitModuleTestHelper _repo1;
        private GitModuleTestHelper _repo2;
        private GitModuleTestHelper _repo3;

        // Note that _repo2Module and _repo3Module point to the submodules under _repo1Module,
        // not _repo2.Module and _repo3.Module respectively. In general, the tests should here
        // should interact with these modules, not with _repo2 and _repo3.
        private GitModule _repo1Module;
        private GitModule _repo2Module;
        private GitModule _repo3Module;

        private ISubmoduleStatusProvider _provider;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            // Remember the current settings...
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;
            _showAvailableDiffTools = AppSettings.ShowAvailableDiffTools;

            // Stop loading custom diff tools
            AppSettings.ShowAvailableDiffTools = false;

            // We don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
            AppSettings.ShowAvailableDiffTools = _showAvailableDiffTools;
        }

        [SetUp]
        public void SetUp()
        {
            _repo1 = new GitModuleTestHelper("repo1");
            _repo2 = new GitModuleTestHelper("repo2");
            _repo3 = new GitModuleTestHelper("repo3");

            _repo2.AddSubmodule(_repo3, "repo3");
            _repo1.AddSubmodule(_repo2, "repo2");
            var submodules = _repo1.GetSubmodulesRecursive();

            _repo1Module = _repo1.Module;
            _repo2Module = submodules.ElementAt(0);
            _repo3Module = submodules.ElementAt(1);

            // Use the singleton provider, which is also used by the left panel, so we can synchronize on updates
            _provider = SubmoduleStatusProvider.Default;

            _commands = new GitUICommands(_repo1Module);

            var composition = TestComposition.Empty
                .AddParts(typeof(MockLinkFactory))
                .AddParts(typeof(MockWindowsJumpListManager))
                .AddParts(typeof(MockRepositoryDescriptionProvider))
                .AddParts(typeof(MockAppTitleGenerator));
            ExportProvider mefExportProvider = composition.ExportProviderFactory.CreateExportProvider();
            ManagedExtensibility.SetTestExportProvider(mefExportProvider);
        }

        [TearDown]
        public void TearDown()
        {
            // _provider is a singleton and must not be disposed
            _repo1.Dispose();
            _repo2.Dispose();
            _repo3.Dispose();
        }

        [Test]
        public void RepoObjectTree_should_show_all_submodules()
        {
            RunFormTest(
                async form =>
                {
                    // act
                    await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, _repo1Module);

                    // assert
                    var submodulesNode = GetSubmoduleNode(form);

                    submodulesNode.Nodes.Count.Should().Be(1);

                    var repo1Node = submodulesNode.Nodes[0];
                    repo1Node.Name.Should().StartWith("repo1");
                    repo1Node.Nodes.Count.Should().Be(1);

                    var repo2Node = repo1Node.Nodes[0];
                    repo2Node.Name.Should().StartWith("repo2");
                    repo2Node.Nodes.Count.Should().Be(1);

                    var repo3Node = repo2Node.Nodes[0];
                    repo3Node.Name.Should().StartWith("repo3");
                    repo3Node.Nodes.Count.Should().Be(0);
                });
        }

        private TreeNode GetSubmoduleNode(FormBrowse form)
        {
            var treeView = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor().TreeView;
            var remotesNode = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(n => n.Text == TranslatedStrings.Submodules);
            remotesNode.Should().NotBeNull();
            return remotesNode;
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }
    }
}
