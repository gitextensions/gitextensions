﻿using Castle.Core.Internal;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using JetBrains.Annotations;
using NSubstitute;

namespace GitUITests.UserControls
{
    [TestFixture]
    public class ToolStripPushButtonTests
    {
        private static readonly int PushImageHashCode = Images.Push.RawFormat.GetHashCode();
        private bool _originalShowAheadBehindData;
        private IAheadBehindDataProvider _aheadBehindDataProvider;
        private ToolStripPushButton _sut;

        [SetUp]
        public void Setup()
        {
            _originalShowAheadBehindData = AppSettings.ShowAheadBehindData;
            AppSettings.ShowAheadBehindData = true;

            _aheadBehindDataProvider = Substitute.For<IAheadBehindDataProvider>();

            _sut = new ToolStripPushButton();
            _sut.ResetToDefaultState();
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowAheadBehindData = _originalShowAheadBehindData;
        }

        [Test]
        public void DisplayAheadBehindInformation_should_not_display_anything_if_does_not_support_ahead_behind()
        {
            _sut.ResetToDefaultState();

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(), "any-branchName", string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().Be("Push");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [TestCaseSource(nameof(GetInvalidAheadBehindData))]
        public void DisplayAheadBehindInformation_should_display_default_text_image_if_ahead_behind_data_null(IDictionary<string, AheadBehindData> data)
        {
            string branchName = "who_cares";
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().Be("Push");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        private static IEnumerable<TestCaseData> GetInvalidAheadBehindData
        {
            [UsedImplicitly]
            get
            {
                int index = 0;
                yield return new TestCaseData(null)
                    .SetName($"{++index}. AheadBehindData is null");

                yield return new TestCaseData(new Dictionary<string, AheadBehindData>())
                    .SetName($"{++index}. AheadBehindData is empty");

                yield return new TestCaseData(new Dictionary<string, AheadBehindData> { { "some_branch", new AheadBehindData() } })
                    .SetName($"{++index}. AheadBehindData doesn't contain desired branch");
            }
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_normal_state_when_no_data_returned()
        {
            string branchName = "(no branch) or any-branch";
            _aheadBehindDataProvider.GetData(branchName).Returns(x => null);

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().Be("Push");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_normal_state_when_in_ahead_state()
        {
            string branchName = "my-branch";
            Dictionary<string, AheadBehindData> data = new()
            {
                { branchName, new AheadBehindData { AheadCount = "9", BehindCount = string.Empty, Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().Contain("9 new commit(s) will be pushed");
            _sut.ToolTipText.Should().NotContain("commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_warning_state_when_in_behind_state()
        {
            string branchName = "my-branch";
            Dictionary<string, AheadBehindData> data = new()
            {
                { branchName, new AheadBehindData { AheadCount = string.Empty, BehindCount = "2", Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().NotContain("new commit(s) will be pushed");
            _sut.ToolTipText.Should().Contain("2 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_warning_state_when_in_ahead_and_behind_state()
        {
            string branchName = "my-branch";
            Dictionary<string, AheadBehindData> data = new()
            {
                { branchName, new AheadBehindData { AheadCount = "99", BehindCount = "3", Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().Contain("99 new commit(s) will be pushed");
            _sut.ToolTipText.Should().Contain("3 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_nothing_when_disabled()
        {
            AppSettings.ShowAheadBehindData = false;

            string branchName = "my-branch";

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().NotContain("99 new commit(s) will be pushed");
            _sut.ToolTipText.Should().NotContain("3 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }

        [Test]
        public void PushButton_should_keep_size_before_update_and_decrease_if_fewer_changes()
        {
            string branchName = "my-branch";
            Dictionary<string, AheadBehindData> data = new()
            {
                { branchName, new AheadBehindData { AheadCount = "99", BehindCount = "33", Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(_aheadBehindDataProvider?.GetData(branchName), branchName, string.Empty);
            int updatedSize = _sut.GetTestAccessor().GetButtonWidth();
            _sut.ResetBeforeUpdate();
            _sut.GetTestAccessor().GetButtonWidth().Should().Be(updatedSize);
            _sut.GetTestAccessor().GetButtonText().IsNullOrEmpty().Should().BeTrue();
            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
        }
    }
}
