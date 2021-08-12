using System;
using System.Threading;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class FilterToolBarTests
    {
        private bool _originalShowFirstParent;
        private FilterToolBar _filterToolBar;
        private IGitModule _gitModule;
        private IRevisionGridFilter _revisionGridFilter;

        [SetUp]
        public void Setup()
        {
            _originalShowFirstParent = AppSettings.ShowFirstParent;
            AppSettings.ShowFirstParent = false;

            _gitModule = Substitute.For<IGitModule>();
            _revisionGridFilter = Substitute.For<IRevisionGridFilter>();

            _filterToolBar = new();
            _filterToolBar.Bind(() => _gitModule, _revisionGridFilter);
            _filterToolBar.GetTestAccessor().SetUnitTestsMode();
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowFirstParent = _originalShowFirstParent;
        }

        [Test]
        public void RevisionGridFilter_should_throw_if_unbound()
        {
            ((Action)(() => _ = new FilterToolBar().GetTestAccessor().RevisionGridFilter))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void GetModule_should_throw_if_unbound()
        {
            ((Action)(() => _ = new FilterToolBar().GetTestAccessor().GetModule()))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ShowFirstParentChecked_should_behave_expected()
        {
            _filterToolBar.ShowFirstParentChecked.Should().Be(false);
            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked.Should().Be(false);

            _filterToolBar.ShowFirstParentChecked = true;
            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked.Should().Be(true);
        }

        [Test]
        public void ShowFirstParentChecked_should_reflect_button_state()
        {
            _filterToolBar.ShowFirstParentChecked.Should().Be(false);
            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked.Should().Be(false);

            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked = true;
            _filterToolBar.ShowFirstParentChecked.Should().Be(true);

            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked = false;
            _filterToolBar.ShowFirstParentChecked.Should().Be(false);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ShowFirstParentChecked_should_be_bound_from_AppSettings(bool settingValue)
        {
            bool original = AppSettings.ShowFirstParent;
            try
            {
                AppSettings.ShowFirstParent = settingValue;

                FilterToolBar filterToolBar = new();
                filterToolBar.ShowFirstParentChecked.Should().Be(settingValue);
                filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked.Should().Be(settingValue);
            }
            finally
            {
                AppSettings.ShowFirstParent = original;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_no_branches(bool refresh)
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.Count.Should().Be(0);

            _filterToolBar.GetTestAccessor().ApplyBranchFilter(refresh);

            _revisionGridFilter.Received().SetAndApplyBranchFilter(string.Empty, refresh);
            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_NoResultsFound(bool refresh)
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = TranslatedStrings.NoResultsFound;

            _filterToolBar.GetTestAccessor().ApplyBranchFilter(refresh);

            _revisionGridFilter.Received().SetAndApplyBranchFilter(string.Empty, refresh);
            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_branches(bool refresh)
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = "on";

            _filterToolBar.GetTestAccessor().ApplyBranchFilter(refresh);

            _revisionGridFilter.Received().SetAndApplyBranchFilter("on", refresh);
            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [TestCase(false, false, false, false)]
        [TestCase(true, true, true, true)]
        public void ApplyRevisionFilter_should_invoke_RevisionGridFilter(bool byCommit, bool byCommitter, bool byAuthor, bool byDiffContains)
        {
            _filterToolBar.GetTestAccessor().tstxtRevisionFilter.Text = "on";
            _filterToolBar.GetTestAccessor().tsmiCommitFilter.Checked = byCommit;
            _filterToolBar.GetTestAccessor().tsmiCommitterFilter.Checked = byCommitter;
            _filterToolBar.GetTestAccessor().tsmiAuthorFilter.Checked = byAuthor;
            _filterToolBar.GetTestAccessor().tsmiDiffContainsFilter.Checked = byDiffContains;

            _filterToolBar.GetTestAccessor().ApplyRevisionFilter();

            _revisionGridFilter.Received().SetAndApplyRevisionFilter(
                Arg.Is<RevisionFilter>(revisionFilter => AssertFilter(revisionFilter, "on", byCommit, byCommitter, byAuthor, byDiffContains)));

            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [Test]
        public void ApplyRevisionFilter_should_reset_filer_if_RevisionGridFilter_throws_IOE()
        {
            _revisionGridFilter
                .When(x => x.SetAndApplyRevisionFilter(Arg.Any<RevisionFilter>()))
                .Do(x => throw new InvalidOperationException("Boom!"));
            _filterToolBar.GetTestAccessor().tstxtRevisionFilter.Text = "on";

            _filterToolBar.GetTestAccessor().ApplyRevisionFilter();

            _filterToolBar.GetTestAccessor().tstxtRevisionFilter.Text.Should().Be(string.Empty);
        }

        [Test]
        public void SetBranchFilter_should_reset_text()
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = "bla";

            _filterToolBar.SetBranchFilter(string.Empty, refresh: false);

            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text.Should().Be(string.Empty);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("foo", "foo")]
        public void SetBranchFilter(string given, string expected)
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });

            _filterToolBar.SetBranchFilter(given, true);

            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text.Should().Be(expected);
            _revisionGridFilter.Received().SetAndApplyBranchFilter(expected, true);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("foo", "foo")]
        public void SetRevisionFilter(string given, string expected)
        {
            _filterToolBar.SetRevisionFilter(given);

            _filterToolBar.GetTestAccessor().tstxtRevisionFilter.Text.Should().Be(expected);
            _revisionGridFilter.Received().SetAndApplyRevisionFilter(
                Arg.Is<RevisionFilter>(revisionFilter => AssertFilter(revisionFilter, expected,
                                                                      _filterToolBar.GetTestAccessor().tsmiCommitFilter.Checked,
                                                                      _filterToolBar.GetTestAccessor().tsmiCommitterFilter.Checked,
                                                                      _filterToolBar.GetTestAccessor().tsmiAuthorFilter.Checked,
                                                                      _filterToolBar.GetTestAccessor().tsmiDiffContainsFilter.Checked)));
        }

        private static bool AssertFilter(RevisionFilter receivedRevisionFilter, string filter, bool byCommit, bool byCommitter, bool byAuthor, bool byDiffContains)
        {
            receivedRevisionFilter.Should().NotBeNull();
            receivedRevisionFilter.Text.Should().Be(filter);
            receivedRevisionFilter.FilterByAuthor.Should().Be(byAuthor);
            receivedRevisionFilter.FilterByCommit.Should().Be(byCommit);
            receivedRevisionFilter.FilterByCommitter.Should().Be(byCommitter);
            receivedRevisionFilter.FilterByDiffContent.Should().Be(byDiffContains);
            return true;
        }
    }
}
