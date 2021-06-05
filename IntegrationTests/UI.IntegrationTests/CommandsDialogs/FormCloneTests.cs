using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands.Git;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormCloneTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository is null)
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
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [TestCase(null, false, "")]
        [TestCase("", false, "")]
        [TestCase(" ", false, "")]
        [TestCase("blah", false, "")]
        [TestCase("git clone https://github.com/gitextensions/gitextensions && cd gitextensions", true, "https://github.com/gitextensions/gitextensions")]
        [TestCase("git clone ssh://username@gerrit-server:/PROJECT", true, "ssh://username@gerrit-server:/PROJECT")]
        [TestCase("git clone https://github.com/gitextensions/gitextensions && git clone https://github.com/gitextensions/git.hub", true, "https://github.com/gitextensions/gitextensions")]
        public void Test_Url_extract_from_string(
            string text, bool expected, string expectedUrl)
        {
            RunFormTest(
                form =>
                {
                    var accessor = form.GetTestAccessor();

                    accessor.TryExtractUrl(text, out var url).Should().Equals(expected);

                    // No need to compare URL if the result was expected to be false
                    if (expected)
                    {
                        url.Should().Equals(expectedUrl);
                    }
                });
        }

        private void RunFormTest(Action<FormClone> testDriver)
        {
            RunFormTest(
                form =>
                {
                    testDriver(form);
                    return Task.CompletedTask;
                });
        }

        private void RunFormTest(Func<FormClone, Task> testDriverAsync)
        {
            UITest.RunForm(
                () =>
                {
                    _commands.StartCloneDialog(owner: null, url: null);
                },
                testDriverAsync);
        }
    }
}
