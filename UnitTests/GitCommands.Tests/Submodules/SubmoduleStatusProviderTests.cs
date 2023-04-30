using System.Diagnostics;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitCommands.Submodules;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.Submodules
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]

    [Parallelizable]
    internal class SubmoduleStatusProviderTests
    {
        private GitModuleTestHelper _repo1;
        private GitModuleTestHelper _repo2;
        private GitModuleTestHelper _repo3;

        // Note that _repo2Module and _repo3Module point to the submodules under _repo1Module,
        // not the 'origin' _repo2.Module and _repo3.Module respectively. In general, the tests should here
        // should interact with these modules, not with _repo2 and _repo3.
        private GitModule _repo1Module;
        private GitModule _repo2Module;
        private GitModule _repo3Module;

        private ISubmoduleStatusProvider _provider;
        private bool _isInit = false;

        /// <summary>
        /// The repo creation should only be done once as it slows tests down considerably
        /// Each test must therefore revert changes after the test
        /// OneTimeSetUp cannot be used, as the setup must run after BeforeTest in CommonTestUtils\ConfigureJoinableTaskFactoryAttribute.cs
        /// OneTimeTearDown can be used though
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            _repo1 = new GitModuleTestHelper("repo1");
            _repo2 = new GitModuleTestHelper("repo2");
            _repo3 = new GitModuleTestHelper("repo3");

            _repo2.AddSubmodule(_repo3, "repo3");
            _repo1.AddSubmodule(_repo2, "repo2");
            var submodules = _repo1.GetSubmodulesRecursive();

            _repo1Module = _repo1.Module;
            _repo2Module = submodules.ElementAt(0);
            _repo3Module = submodules.ElementAt(1);

            GitModule[] actualModules = new GitModule[]
            {
                _repo1Module,
                _repo2Module,
                _repo3Module
            };
            for (int i = 0; i < actualModules.Length; i++)
            {
                Debug.WriteLine($"Repo[{i + 1}]:{actualModules[i].WorkingDir}");
            }

            _provider = new SubmoduleStatusProvider();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _provider.Dispose();
            _repo1.Dispose();
            _repo2.Dispose();
            _repo3.Dispose();
        }

        [NonParallelizable]
        [Test]
        public async Task UpdateSubmoduleStructure_valid_result_for_top_module()
        {
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, _repo1Module);

            result.Should().NotBeNull();
            result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
            result.SuperProject.Should().Be(null);
            result.CurrentSubmoduleName.Should().Be(null);
            result.AllSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
        }

        [NonParallelizable]
        [Test]
        public async Task UpdateSubmoduleStructure_valid_result_for_first_nested_submodule()
        {
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, _repo2Module);

            result.Should().NotBeNull();
            result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
            result.SuperProject.Should().Be(result.TopProject);
            result.CurrentSubmoduleName.Should().Be("repo2");
            result.AllSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.OurSubmodules.Select(info => info.Path).Should().ContainSingle(_repo3Module.WorkingDir);
        }

        [NonParallelizable]
        [Test]
        public async Task UpdateSubmoduleStructure_valid_result_for_second_nested_submodule()
        {
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, _repo3Module);

            result.Should().NotBeNull();
            result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
            result.SuperProject.Path.Should().Be(_repo2Module.WorkingDir);
            result.CurrentSubmoduleName.Should().Be("repo3");
            result.AllSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Select(info => info.Path).Should().BeEmpty();
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_first_nested_module_change()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo2
            _repo1.CreateFile(_repo2Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            File.Delete(Path.Combine(_repo2Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_first_nested_module_commit()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in repo2
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_first_nested_module_change_commit()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change and update commit in repo2
            _repo1.CreateFile(_repo2Module.WorkingDir, "test.txt", "test");
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            File.Delete(Path.Combine(_repo2Module.WorkingDir, "test.txt"));
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_second_nested_module_change()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo3
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_first_nested_module_commit_second_nested_module_change()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo3
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change for repo3
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_second_nested_module_commit()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in repo3
            _repo3Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            _repo3Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_top_module_change()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo1
            _repo1.CreateFile(_repo1Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.Should().BeNull();
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            File.Delete(Path.Combine(_repo1Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_top_module_commit()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in topmodule
            currentModule.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Revert the change
            _repo1Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_first_nested_module_with_top_module_changes()
        {
            var currentModule = _repo2Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo1
            _repo1.CreateFile(_repo1Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.Should().BeNull();

            // Revert the change
            File.Delete(Path.Combine(_repo1Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_first_nested_module_with_second_nested_module_changes()
        {
            var currentModule = _repo2Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo3
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Revert the change
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_first_nested_module_with_second_nested_module_commit()
        {
            var currentModule = _repo2Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in repo3
            _repo3Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.OurSubmodules[0].Should().BeEquivalentTo(result.AllSubmodules[1]);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Revert the change
            _repo3Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_second_nested_module_changes()
        {
            var currentModule = _repo3Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo3
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Revert the change
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_second_nested_module_commit()
        {
            var currentModule = _repo3Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in repo3
            _repo3Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Revert the change
            _repo3Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_first_nested_module_commit()
        {
            var currentModule = _repo3Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Update commit in repo2
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Revert the change
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [Ignore("Delays tests with 15s without much value in the test")]
        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_no_forced_changes()
        {
            var currentModule = _repo1Module;
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // No changes in repo
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            // Make a change in repo1 without force update, should take 15s
            DateTime statusStart = DateTime.Now;
            _repo1.CreateFile(_repo1Module.WorkingDir, "test.txt", "test");
            changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles, false);
            result.AllSubmodules[0].Detailed.Should().BeNull();
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            (DateTime.Now - statusStart).TotalSeconds.Should().BeGreaterOrEqualTo(14);

            // Revert the change
            File.Delete(Path.Combine(_repo1Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_top_module_with_top_module_changes()
        {
            var currentModule = _repo1Module;
            _repo1.CreateFile(_repo1Module.WorkingDir, "test.txt", "test");

            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            // When top module is current, only structure is updated and no changes seen until explicit git-status
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.Should().BeNull();

            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.OurSubmodules.Should().BeEquivalentTo(result.AllSubmodules);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();
            result.TopProject.Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);

            // Revert the change
            File.Delete(Path.Combine(_repo1Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_first_nested_module_prechanges()
        {
            var currentModule = _repo3Module;

            // Update commit in repo2, will check status
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            result.AllSubmodules[0].Detailed.Should().NotBeNull();
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Revert the change
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_prechanges_noupdate()
        {
            var currentModule = _repo3Module;

            // Update repos
            _repo1.CreateFile(_repo1Module.WorkingDir, "test.txt", "test");
            _repo1Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            _repo1.CreateFile(_repo2Module.WorkingDir, "test.txt", "test");
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            _repo3Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");

            // Update requires: AppSettings.ShowSubmoduleStatus && (AppSettings.ShowGitStatusInBrowseToolbar || (AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowWorkingDirChanges))
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule, false);

            result.Should().NotBeNull();

            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();

            // Revert the change
            _repo1Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            File.Delete(Path.Combine(_repo1Module.WorkingDir, "test.txt"));
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            File.Delete(Path.Combine(_repo2Module.WorkingDir, "test.txt"));
            _repo3Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        [NonParallelizable]
        [Test]
        public async Task Submodule_status_changes_for_second_nested_module_with_first_nested_module_precommit()
        {
            var currentModule = _repo3Module;

            // Update commit in repo2, will check status
            _repo2Module.GitExecutable.GetOutput(@"commit --allow-empty -m ""Dummy commit""");
            var result = await SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResultAsync(_provider, currentModule);

            result.Should().NotBeNull();

            result.AllSubmodules[0].Detailed.Should().NotBeNull();
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeFalse();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.Should().BeNull();
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Make a change in repo3, still not changing
            _repo1.CreateFile(_repo3Module.WorkingDir, "test.txt", "test");
            var changedFiles = GetStatusChangedFiles(currentModule);
            changedFiles.Should().HaveCount(1);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, currentModule, changedFiles);
            result.AllSubmodules[0].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[0].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.FastForward);
            result.AllSubmodules[1].Detailed.IsDirty.Should().BeTrue();
            result.AllSubmodules[1].Detailed.Status.Should().BeEquivalentTo(SubmoduleStatus.Unknown);
            result.TopProject.Detailed.IsDirty.Should().BeTrue();

            // Revert the change
            _repo2Module.GitExecutable.Execute(@"checkout HEAD^", throwOnErrorExit: false);
            File.Delete(Path.Combine(_repo3Module.WorkingDir, "test.txt"));
            await CheckRevertedStatus(result);
        }

        /// <summary>
        /// Check that the repo is reverted after the test, prepared for next
        /// An explicit Git clean and reset will require additional time
        /// </summary>
        /// <param name="result">The existing structure, reused from the test</param>
        /// <returns>a Task</returns>
        private async Task CheckRevertedStatus(SubmoduleInfoResult result)
        {
            var changedFiles = GetStatusChangedFiles(_repo1Module);
            changedFiles.Should().HaveCount(0);
            await SubmoduleTestHelpers.UpdateSubmoduleStatusAndWaitForResultAsync(_provider, _repo1Module, changedFiles);
            result.AllSubmodules.All(i => i.Detailed is null).Should().BeTrue();
            result.TopProject.Detailed.Should().BeNull();
        }

        private static IReadOnlyList<GitItemStatus> GetStatusChangedFiles(IGitModule module)
        {
            var cmd = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default, noLocks: true);
            var output = module.GitExecutable.GetOutput(cmd);
            return new GetAllChangedFilesOutputParser(() => module).Parse(output);
        }
    }
}
