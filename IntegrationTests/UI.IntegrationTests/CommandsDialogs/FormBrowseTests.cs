﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FormBrowseTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        // Track the original setting value
        private bool _originalShowAuthorAvatarColumn;
        private bool _showAvailableDiffTools;

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

            _referenceRepository.Dispose();
        }

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

        [Test]
        public void PopulateFavouriteRepositoriesMenu_should_order_favourites_alphabetically()
        {
            RunFormTest(
                form =>
                {
                    var tsmiFavouriteRepositories = new ToolStripMenuItem();
                    var repositoryHistory = new List<Repository>
                    {
                        new Repository(@"c:\") { Category = "D" },
                        new Repository(@"c:\") { Category = "A" },
                        new Repository(@"c:\") { Category = "C" },
                        new Repository(@"c:\") { Category = "B" }
                    };

                    form.GetTestAccessor().PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories, repositoryHistory);

                    // assert
                    var categories = tsmiFavouriteRepositories.DropDownItems.Cast<ToolStripMenuItem>().Select(x => x.Text).ToList();
                    categories.Should().BeInAscendingOrder();
                });
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
                showForm: () => _commands.StartBrowseDialog(owner: null).Should().BeTrue(),
                testDriverAsync);
        }
    }
}
