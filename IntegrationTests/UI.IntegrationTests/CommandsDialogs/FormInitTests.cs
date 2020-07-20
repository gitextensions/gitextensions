using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormInitTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void Should_show_supplied_path()
        {
            var currentDir = "bla";
            RunFormTest(
                form =>
                {
                    Assert.AreEqual(currentDir, form.GetTestAccessor().DirectoryCombo.Text);
                },
                currentDir);
        }

        // Strictly speaking this is a test for GitUICommands.StartInitializeDialog
        [Test]
        public void Should_show_current_GitModuleWorkingDir_if_supplied_path_null()
        {
            RunFormTest(
                form =>
                {
                    Assert.AreEqual(_referenceRepository.Module.WorkingDir, form.GetTestAccessor().DirectoryCombo.Text);
                },
                null);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("    ")]
        [TestCase(@"foo\bar")]
        public void IsRootedDirectoryPath_should_detect_invalid_paths(string input)
        {
            var currentDir = "bla";
            RunFormTest(
                form =>
                {
                    Assert.IsFalse(form.GetTestAccessor().IsRootedDirectoryPath(input));
                },
                currentDir);
        }

        [TestCase(@"c:\foo\bar")]
        [TestCase(@"c:\foo\bar\")]
        [TestCase(@"c:")]
        [TestCase(@"  c:\foo\bar  ")]
        public void IsRootedDirectoryPath_returns_true_on_valid_paths(string input)
        {
            var currentDir = "bla";
            RunFormTest(
                form =>
                {
                    Assert.IsTrue(form.GetTestAccessor().IsRootedDirectoryPath(input));
                },
                currentDir);
        }

        private void RunFormTest(Action<FormInit> testDriver, string path)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                },
                path);
        }

        private void RunFormTest(Func<FormInit, Task> testDriverAsync, string path)
        {
            UITest.RunForm(
                () =>
                {
                    Assert.True(_commands.StartInitializeDialog(owner: null, path));
                },
                testDriverAsync);
        }
    }
}
