using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;

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
            _sut.Initialize(_aheadBehindDataProvider);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowAheadBehindData = _originalShowAheadBehindData;
        }

        [Test]
        public void DisplayAheadBehindInformation_should_not_display_anything_if_does_not_support_ahead_behind()
        {
            _sut.Initialize(null);

            _sut.DisplayAheadBehindInformation("any-branchName");

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().Be("Push");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [TestCaseSource(nameof(GetInvalidAheadBehindData))]
        public void DisplayAheadBehindInformation_should_display_default_text_image_if_ahead_behind_data_null(IDictionary<string, AheadBehindData> data)
        {
            var branchName = "who_cares";
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
            var branchName = "(no branch) or any-branch";
            _aheadBehindDataProvider.GetData(branchName).Returns(x => null);

            _sut.DisplayAheadBehindInformation(branchName);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().Be("Push");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_normal_state_when_in_ahead_state()
        {
            var branchName = "my-branch";
            var data = new Dictionary<string, AheadBehindData>
            {
                { branchName, new AheadBehindData { AheadCount = "9", BehindCount = string.Empty, Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(branchName);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().Contain("9 new commit(s) will be pushed");
            _sut.ToolTipText.Should().NotContain("commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(PushImageHashCode);
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_warning_state_when_in_behind_state()
        {
            var branchName = "my-branch";
            var data = new Dictionary<string, AheadBehindData>
            {
                { branchName, new AheadBehindData { AheadCount = string.Empty, BehindCount = "2", Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(branchName);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().NotContain("new commit(s) will be pushed");
            _sut.ToolTipText.Should().Contain("2 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_warning_state_when_in_ahead_and_behind_state()
        {
            var branchName = "my-branch";
            var data = new Dictionary<string, AheadBehindData>
            {
                { branchName, new AheadBehindData { AheadCount = "99", BehindCount = "3", Branch = branchName } }
            };
            _aheadBehindDataProvider.GetData(branchName).Returns(x => data);

            _sut.DisplayAheadBehindInformation(branchName);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.ImageAndText);
            _sut.ToolTipText.Should().Contain("99 new commit(s) will be pushed");
            _sut.ToolTipText.Should().Contain("3 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }

        [Test]
        public void DisplayAheadBehindInformation_should_display_nothing_when_disabled()
        {
            AppSettings.ShowAheadBehindData = false;

            var branchName = "my-branch";

            _sut.DisplayAheadBehindInformation(branchName);

            _sut.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
            _sut.ToolTipText.Should().NotContain("99 new commit(s) will be pushed");
            _sut.ToolTipText.Should().NotContain("3 commit(s) should be integrated");
            _sut.Image.RawFormat.GetHashCode().Should().Be(Images.Unstage.RawFormat.GetHashCode());
        }
    }
}
