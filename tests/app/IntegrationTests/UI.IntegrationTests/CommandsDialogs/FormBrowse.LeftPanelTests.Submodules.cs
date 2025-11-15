using System.ComponentModel.Design;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;

namespace GitExtensions.UITests.CommandsDialogs;

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
    private IGitModule _repo1Module;
    private IGitModule _repo2Module;
    private IGitModule _repo3Module;

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
        IEnumerable<IGitModule> submodules = _repo1.GetSubmodulesRecursive();

        _repo1Module = _repo1.Module;
        _repo2Module = submodules.ElementAt(0);
        _repo3Module = submodules.ElementAt(1);

        _provider = new SubmoduleStatusProvider((path) => new GitModule(path));

        IServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        serviceContainer.RemoveService<ISubmoduleStatusProvider>();
        serviceContainer.AddService(_provider);

        _commands = new GitUICommands(serviceContainer, _repo1Module);
    }

    [TearDown]
    public void TearDown()
    {
        _provider.Dispose();
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
                TreeNode submodulesNode = GetSubmoduleNode(form);

                // act: wait until loaded by left panel
                UITest.ProcessUntil("Loading submodules", () => submodulesNode.Nodes.Count == 1);

                // assert
                submodulesNode.Nodes.Count.Should().Be(1);

                TreeNode repo1Node = submodulesNode.Nodes[0];
                repo1Node.Name.Should().StartWith("repo1");
                repo1Node.Nodes.Count.Should().Be(1);

                TreeNode repo2Node = repo1Node.Nodes[0];
                repo2Node.Name.Should().StartWith("repo2");
                repo2Node.Nodes.Count.Should().Be(1);

                TreeNode repo3Node = repo2Node.Nodes[0];
                repo3Node.Name.Should().StartWith("repo3");
                repo3Node.Nodes.Count.Should().Be(0);

                return Task.CompletedTask;
            });
    }

    private static TreeNode GetSubmoduleNode(FormBrowse form)
    {
        GitUI.UserControls.NativeTreeView treeView = form.GetTestAccessor().RepoObjectsTree.GetTestAccessor().TreeView;
        TreeNode remotesNode = treeView.Nodes.OfType<TreeNode>().FirstOrDefault(n => n.Text == TranslatedStrings.Submodules);
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
