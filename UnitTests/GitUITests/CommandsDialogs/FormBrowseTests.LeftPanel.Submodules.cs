using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Submodules;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormBrowseLeftPanelSubmodulesTests
    {
        // Created once for each test
        private GitUICommands _commands;
        private bool _originalShowAuthorAvatarColumn;

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
            _originalShowAuthorAvatarColumn = AppSettings.ShowAuthorAvatarColumn;

            // we don't want avatars during tests, otherwise we will be attempting to download them from gravatar....
            AppSettings.ShowAuthorAvatarColumn = false;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AppSettings.ShowAuthorAvatarColumn = _originalShowAuthorAvatarColumn;
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
        }

        [TearDown]
        public void TearDown()
        {
            _commands = null;

            _repo1.Dispose();
            _repo2.Dispose();
            _repo3.Dispose();
        }

        [Test]
        public void RepoObjectTree_should_show_all_submodules()
        {
            RunFormTest(
                form =>
                {
                    // act
                    SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResult(_provider, _repo1Module);

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
            var remotesNode = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(n => n.Text == GitUI.Strings.Submodules);
            remotesNode.Should().NotBeNull();
            return remotesNode;
        }

        private void RunFormTest(Action<FormBrowse> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Func<FormBrowse, Task> testDriverAsync)
        {
            UITest.RunForm(
                () =>
                {
                    Assert.True(_commands.StartBrowseDialog(owner: null));
                },
                testDriverAsync);
        }
    }
}
