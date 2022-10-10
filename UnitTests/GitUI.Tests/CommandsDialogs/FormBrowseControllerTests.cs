﻿using FluentAssertions;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class FormBrowseControllerTests
    {
        private FormBrowseController _controller;
        private IGitGpgController _gitGpgController;
        private IRepositoryCurrentBranchNameProvider _repositoryCurrentBranchNameProvider;
        private IInvalidRepositoryRemover _invalidRepositoryRemover;

        [SetUp]
        public void Setup()
        {
            _gitGpgController = Substitute.For<IGitGpgController>();
            _repositoryCurrentBranchNameProvider = Substitute.For<IRepositoryCurrentBranchNameProvider>();
            _invalidRepositoryRemover = Substitute.For<IInvalidRepositoryRemover>();

            _controller = new FormBrowseController(_gitGpgController, _repositoryCurrentBranchNameProvider, _invalidRepositoryRemover);
        }

        [Test]
        public void AddRecentRepositories_should_add_new_item()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _controller.AddRecentRepositories(containerMenu, repository, caption, (s, e) => { });

            containerMenu.DropDownItems.Count.Should().Be(1);
        }

        [Test]
        public void AddRecentRepositories_should_set_properties_correctly()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _controller.AddRecentRepositories(containerMenu, repository, caption, (s, e) => { });

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.Text.Should().Be(caption);
            item.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            item.ToolTipText.Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("master")]
        [TestCase("(no branch)")]
        public void AddRecentRepositories_should_show_branch_correctly(string branch)
        {
            _repositoryCurrentBranchNameProvider.GetCurrentBranchName(Arg.Any<string>()).Returns(x => branch);

            ToolStripMenuItem containerMenu = new();

            const string path = "somepath";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _controller.AddRecentRepositories(containerMenu, repository, caption, (s, e) => { });

            // await adding branch name in ShortcutKeyDisplayString, done async
            ThreadHelper.JoinableTaskContext.Factory.Run(() => ThreadHelper.JoinPendingOperationsAsync(default));

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.ShortcutKeyDisplayString.Should().Be(branch);
        }

        [Test]
        public void ChangeWorkingDir_should_promt_user_to_delete_invalid_repo()
        {
            ToolStripMenuItem containerMenu = new();

            const string path = "";
            const string caption = "CAPTION";
            Repository repository = new(path);

            _controller.AddRecentRepositories(containerMenu, repository, caption, (s, e) => { });

            ToolStripMenuItem item = (ToolStripMenuItem)containerMenu.DropDownItems[0];
            item.PerformClick();

            _invalidRepositoryRemover.Received(1).ShowDeleteInvalidRepositoryDialog(path);
        }
    }
}
