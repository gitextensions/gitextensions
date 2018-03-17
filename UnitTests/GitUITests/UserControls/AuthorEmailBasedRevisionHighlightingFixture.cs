using System;
using System.IO;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitUI.UserControls;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [TestFixture]
    internal class AuthorEmailBasedRevisionHighlightingFixture
    {
        private const string ExpectedAuthorEmail1 = "doe1@example.org";
        private const string ExpectedAuthorEmail2 = "doe2@example.org";

        [Test]
        public void AuthorEmailToHighlight_should_be_null_when_no_revision_change_processed_yet()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();

            sut.AuthorEmailToHighlight.Should().BeNull();
        }

        [Test]
        public void When_multiple_revisions_selected_Then_ProcessSelectionChange_should_return_NoAction()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();

            var action = sut.ProcessRevisionSelectionChange(currentModule,
                                               new[]
                                                   {
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1),
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1)
                                                   });

            action.Should().Be(AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.NoAction);
        }

        [Test]
        public void Given_previously_selected_revision_When_multiple_revisions_selected_Then_AuthorEmailToHighlight_should_not_change()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule,
                                               new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule,
                                               new[]
                                                   {
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail2),
                                                       NewRevisionWithAuthorEmail(ExpectedAuthorEmail1)
                                                   });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_no_previously_selected_revision_When_single_revision_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();

            var action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            action.Should().Be(AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.RefreshUserInterface);
        }

        [Test]
        public void Given_no_previously_selected_revision_When_single_revision_selected_Then_AuthorEmailToHighlight_should_change()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_same_author_email_selected_Then_ProcessSelectionChange_should_return_NoAction()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            var action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            action.Should().Be(AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.NoAction);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_same_author_email_selected_Then_AuthorEmailToHighlight_should_not_change()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail1);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_different_author_email_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            var action = sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail2) });

            action.Should().Be(AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.RefreshUserInterface);
        }

        [Test]
        public void Given_previously_selected_revision_When_single_revision_with_different_author_email_selected_Then_AuthorEmailToHighlight_should_change()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail2) });

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail2);
        }

        [Test]
        public void Given_previously_selected_revision_When_no_revision_selected_Then_ProcessSelectionChange_should_return_RefreshUserInterface()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            currentModule.SetSetting(SettingKeyString.UserEmail, ExpectedAuthorEmail2);
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            var action = sut.ProcessRevisionSelectionChange(currentModule, Array.Empty<GitRevision>());

            action.Should().Be(AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.RefreshUserInterface);
        }

        [Test]
        public void Given_previously_selected_revision_When_no_revision_selected_Then_AuthorEmailToHighlight_should_be_value_of_current_user_email_setting()
        {
            var sut = new AuthorEmailBasedRevisionHighlighting();
            var currentModule = NewModule();
            currentModule.SetSetting(SettingKeyString.UserEmail, ExpectedAuthorEmail2);
            sut.ProcessRevisionSelectionChange(currentModule, new[] { NewRevisionWithAuthorEmail(ExpectedAuthorEmail1) });

            sut.ProcessRevisionSelectionChange(currentModule, Array.Empty<GitRevision>());

            sut.AuthorEmailToHighlight.Should().Be(ExpectedAuthorEmail2);
        }

        private static GitModule NewModule()
        {
            return new GitModule(Path.GetTempPath());
        }

        private static GitRevision NewRevisionWithAuthorEmail(string authorEmail)
        {
            return new GitRevision(Guid.NewGuid().ToString())
            {
                AuthorEmail = authorEmail
            };
        }
    }
}
