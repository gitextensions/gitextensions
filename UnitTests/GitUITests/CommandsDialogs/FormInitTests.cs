using System;
using System.Threading;
using System.Threading.Tasks;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs.CommitDialog
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

        [TearDown]
        public void TearDown()
        {
            _commands = null;
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
