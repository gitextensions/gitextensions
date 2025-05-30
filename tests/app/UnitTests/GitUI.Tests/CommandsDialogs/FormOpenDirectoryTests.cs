﻿using CommonTestUtils;
using FluentAssertions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using NSubstitute;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormOpenDirectoryTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;
        private ILocalRepositoryManager _localRepositoryManager;

        [SetUp]
        public void Setup()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);
            _localRepositoryManager = Substitute.For<ILocalRepositoryManager>();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void OpenGitRepository_should_return_null_if_directory_not_exist()
        {
            string path = @"C:\some\directory\that\does\not\exist\at\all";

            FormOpenDirectory.TestAccessor.OpenGitRepository(path, _localRepositoryManager).Should().BeNull();
            _localRepositoryManager.DidNotReceive().AddAsMostRecentAsync(Arg.Any<string>());
        }

        [Test]
        public void OpenGitRepository_should_return_null_if_not_valid_git_repo()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[^1].Should().Be(Path.DirectorySeparatorChar);

            FormOpenDirectory.TestAccessor.OpenGitRepository(path, _localRepositoryManager).Should().BeNull();
            _localRepositoryManager.DidNotReceive().AddAsMostRecentAsync(Arg.Any<string>());
        }

        [Test]
        public void OpenGitRepository_should_not_throw_if_path_not_end_with_slash()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[^1].Should().Be(Path.DirectorySeparatorChar);

            // ensure absence of the trailing slash isn't a problem
            path = path[..^1];
            ClassicAssert.DoesNotThrow(() => FormOpenDirectory.TestAccessor.OpenGitRepository(path, _localRepositoryManager));
        }

        [Test]
        public void OpenGitRepository_should_return_module_For_valid_git_repo()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[^1].Should().Be(Path.DirectorySeparatorChar);

            IGitModule module = FormOpenDirectory.TestAccessor.OpenGitRepository(_referenceRepository.Module.WorkingDir, _localRepositoryManager);

            module.Should().NotBeNull();
            module.WorkingDir.Should().Be(_referenceRepository.Module.WorkingDir);
            _localRepositoryManager.Received(1).AddAsMostRecentAsync(_referenceRepository.Module.WorkingDir);
        }
    }
}
