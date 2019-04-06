﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Submodules;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace GitCommandsTests.Submodules
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    internal class SubmoduleStatusProviderTests
    {
        [Apartment(ApartmentState.STA)]
        public class IntegrationTests
        {
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

                _provider = new SubmoduleStatusProvider();
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
            public void UpdateSubmoduleStructure_valid_result_for_top_module()
            {
                var result = SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResult(_provider, _repo1Module);

                result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
                result.SuperProject.Should().Be(null);
                result.CurrentSubmoduleName.Should().Be(null);
                result.OurSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
                result.SuperSubmodules.Should().BeEmpty();
            }

            [Test]
            public void UpdateSubmoduleStructure_valid_result_for_first_nested_submodule()
            {
                var result = SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResult(_provider, _repo2Module);

                result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
                result.SuperProject.Should().Be(result.TopProject);
                result.CurrentSubmoduleName.Should().Be("repo2");
                result.OurSubmodules.Select(info => info.Path).Should().ContainSingle(_repo3Module.WorkingDir);
                result.SuperSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
            }

            [Test]
            public void UpdateSubmoduleStructure_valid_result_for_second_nested_submodule()
            {
                var result = SubmoduleTestHelpers.UpdateSubmoduleStructureAndWaitForResult(_provider, _repo3Module);

                result.TopProject.Path.Should().Be(_repo1Module.WorkingDir);
                result.SuperProject.Path.Should().Be(_repo2Module.WorkingDir);
                result.CurrentSubmoduleName.Should().Be("repo3");
                result.OurSubmodules.Select(info => info.Path).Should().BeEmpty();
                result.SuperSubmodules.Select(info => info.Path).Should().Contain(_repo2Module.WorkingDir, _repo3Module.WorkingDir);
            }

            private static IReadOnlyList<GitItemStatus> GetStatusChangedFiles(IGitModule module)
            {
                var cmd = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.Default, noLocks: true);
                var output = module.GitExecutable.GetOutput(cmd);
                return GitCommandHelpers.GetStatusChangedFilesFromString(module, output);
            }
        }
    }
}
