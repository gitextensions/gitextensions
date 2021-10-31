using System;
using System.ComponentModel;
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
        private bool _originalShowReflogReferences;
        private FilterToolBar _filterToolBar;
        private IGitModule _gitModule;
        private IRevisionGridFilter _revisionGridFilter;

        [SetUp]
        public void Setup()
        {
            _originalShowFirstParent = AppSettings.ShowFirstParent;
            _originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowFirstParent = false;
            AppSettings.ShowReflogReferences = false;

            _gitModule = Substitute.For<IGitModule>();
            _revisionGridFilter = Substitute.For<IRevisionGridFilter>();

            _filterToolBar = new();
            _filterToolBar.Bind(() => _gitModule, _revisionGridFilter);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.ShowFirstParent = _originalShowFirstParent;
            AppSettings.ShowReflogReferences = _originalShowReflogReferences;
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
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_no_branches()
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.Count.Should().Be(0);

            _filterToolBar.GetTestAccessor().ApplyCustomBranchFilter();

            _revisionGridFilter.Received().SetAndApplyBranchFilter(string.Empty);
            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [Test]
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_NoResultsFound()
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = TranslatedStrings.NoResultsFound;

            _filterToolBar.GetTestAccessor().ApplyCustomBranchFilter();

            _revisionGridFilter.Received().SetAndApplyBranchFilter(string.Empty);
            _filterToolBar.GetTestAccessor()._isApplyingFilter.Should().BeFalse();
        }

        [Test]
        public void ApplyBranchFilter_should_invoke_RevisionGridFilter_with_branches()
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = "on";

            _filterToolBar.GetTestAccessor().ApplyCustomBranchFilter();

            _revisionGridFilter.Received().SetAndApplyBranchFilter("on");
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
        public void SetBranchFilter_should_reset_text()
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text = "bla";

            _filterToolBar.SetBranchFilter(string.Empty);

            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text.Should().Be(string.Empty);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("foo", "foo")]
        public void SetBranchFilter(string given, string expected)
        {
            _filterToolBar.GetTestAccessor().tscboBranchFilter.Items.AddRange(new[] { "one", "two" });

            _filterToolBar.SetBranchFilter(given);

            _filterToolBar.GetTestAccessor().tscboBranchFilter.Text.Should().Be(expected);
            _revisionGridFilter.Received().SetAndApplyBranchFilter(expected);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        public void SetRevisionFilter_when_empty(string given, string expected)
        {
            _filterToolBar.SetRevisionFilter(given);

            _filterToolBar.GetTestAccessor().tstxtRevisionFilter.Text.Should().Be(expected);
            _revisionGridFilter.DidNotReceive().SetAndApplyRevisionFilter(Arg.Any<RevisionFilter>());
        }

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

        [TestCase(false)]
        [TestCase(true)]
        public void ShowFirstParent_should_be_bound_via_FilterChanged(bool settingValue)
        {
            bool original = AppSettings.ShowFirstParent;
            try
            {
                AppSettings.ShowFirstParent = settingValue;

                _revisionGridFilter.FilterChanged += Raise.EventWith(_revisionGridFilter, new FilterChangedEventArgs(new()));
                _filterToolBar.GetTestAccessor().tsmiShowFirstParent.Checked.Should().Be(settingValue);
            }
            finally
            {
                AppSettings.ShowFirstParent = original;
            }
        }

        [Test]
        public void ShowFirstParent_should_invoke_ToggleShowFirstParent()
        {
            _filterToolBar.GetTestAccessor().tsmiShowFirstParent.PerformClick();
            _revisionGridFilter.Received(1).ToggleShowFirstParent();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ShowReflogs_should_be_bound_via_FilterChanged(bool settingValue)
        {
            bool original = AppSettings.ShowReflogReferences;
            try
            {
                AppSettings.ShowReflogReferences = settingValue;

                _revisionGridFilter.FilterChanged += Raise.EventWith(_revisionGridFilter, new FilterChangedEventArgs(new()));
                _filterToolBar.GetTestAccessor().tsmiShowReflogs.Checked.Should().Be(settingValue);
            }
            finally
            {
                AppSettings.ShowReflogReferences = original;
            }
        }

        [Test]
        public void ShowReflogs_should_invoke_ToggleShowReflogReferences()
        {
            _filterToolBar.GetTestAccessor().tsmiShowReflogs.PerformClick();
            _revisionGridFilter.Received(1).ToggleShowReflogReferences();
        }

        [Test]
        public void ShowBranches_ShowAll_should_invoke_ShowAllBranches()
        {
            _filterToolBar.GetTestAccessor().tsmiShowBranchesAll.PerformClick();
            _revisionGridFilter.Received(1).ShowAllBranches();
        }

        [Test]
        public void ShowBranches_ShowCurrent_should_invoke_ShowCurrentBranchOnly()
        {
            _filterToolBar.GetTestAccessor().tsmiShowBranchesCurrent.PerformClick();
            _revisionGridFilter.Received(1).ShowCurrentBranchOnly();
        }

        [Test]
        public void ShowBranches_ShowFiltered_should_invoke_ShowFilteredBranches()
        {
            _filterToolBar.GetTestAccessor().tsmiShowBranchesFiltered.PerformClick();
            _revisionGridFilter.Received(1).ShowFilteredBranches();
        }
    }
}
