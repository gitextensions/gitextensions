using System.IO;
using System.Threading;
using FluentAssertions;
using GitCommands.UserRepositoryHistory;
using GitUI.CommandsDialogs.BrowseDialog;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormOpenDirectoryTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;
        private ILocalRepositoryManager _localRepositoryManager;

        private FormOpenDirectory _form;

        [SetUp]
        public void Setup()
        {
            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _localRepositoryManager = Substitute.For<ILocalRepositoryManager>();
            _form = new FormOpenDirectory(null);
        }

        [Test]
        public void OpenGitRepository_should_return_null_if_directory_not_exist()
        {
            string path = @"C:\some\directory\that\does\not\exist\at\all";

            _form.GetTestAccessor().OpenGitRepository(path, _localRepositoryManager).Should().BeNull();
            _localRepositoryManager.DidNotReceive().AddAsMostRecentAsync(Arg.Any<string>());
        }

        [Test]
        public void OpenGitRepository_should_return_null_if_not_valid_git_repo()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[path.Length - 1].Should().Be(Path.DirectorySeparatorChar);

            _form.GetTestAccessor().OpenGitRepository(path, _localRepositoryManager).Should().BeNull();
            _localRepositoryManager.DidNotReceive().AddAsMostRecentAsync(Arg.Any<string>());
        }

        [Test]
        public void OpenGitRepository_should_not_throw_if_path_not_end_with_slash()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[path.Length - 1].Should().Be(Path.DirectorySeparatorChar);

            // ensure absence of the trailing slash isn't a problem
            path = path.Substring(0, path.Length - 1);
            Assert.DoesNotThrow(() => _form.GetTestAccessor().OpenGitRepository(path, _localRepositoryManager));
        }

        [Test]
        public void OpenGitRepository_should_return_module_For_valid_git_repo()
        {
            // Path.GetTempPath returns a path with the trailing slash
            string path = Path.GetTempPath();
            path[path.Length - 1].Should().Be(Path.DirectorySeparatorChar);

            var module = _form.GetTestAccessor().OpenGitRepository(_referenceRepository.Module.WorkingDir, _localRepositoryManager);

            module.Should().NotBeNull();
            module.WorkingDir.Should().Be(_referenceRepository.Module.WorkingDir);
            _localRepositoryManager.Received(1).AddAsMostRecentAsync(_referenceRepository.Module.WorkingDir);
        }
    }
}