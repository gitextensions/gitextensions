using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ApprovalTests;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class FilterInfoTests
    {
        [SetUp]
        public void SetUp()
        {
            AppSettings.ShowGitNotes = false;
        }

        [Test]
        public void FilterInfo_ctor_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new();
                Approvals.Verify(filterInfo);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [Test]
        public void FilterInfo_ctor_with_Raw_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new() { IsRaw = true };
                Approvals.Verify(filterInfo);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [Test]
        public void FilterInfo_ByDateFrom_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByDateFrom.Should().BeFalse();

            filterInfo.ByDateFrom = true;
            filterInfo.ByDateFrom.Should().BeTrue();

            filterInfo.ByDateFrom = false;
            filterInfo.ByDateFrom.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_DateFrom_with_Raw_expected(bool isRaw, bool byDateFrom)
        {
            DateTime date = new(2022, 1, 15);

            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByDateFrom = byDateFrom,
                DateFrom = date
            };

            if (isRaw || byDateFrom)
            {
                filterInfo.DateFrom.Should().Be(date);
            }
            else
            {
                filterInfo.DateFrom.Should().Be(DateTime.MinValue);
            }
        }

        [Test]
        public void FilterInfo_ByDateTo_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByDateTo.Should().BeFalse();

            filterInfo.ByDateTo = true;
            filterInfo.ByDateTo.Should().BeTrue();

            filterInfo.ByDateTo = false;
            filterInfo.ByDateTo.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_DateTo_with_Raw_expected(bool isRaw, bool byDateTo)
        {
            DateTime date = new(2022, 1, 15);

            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByDateTo = byDateTo,
                DateTo = date
            };

            if (isRaw || byDateTo)
            {
                filterInfo.DateTo.Should().Be(date);
            }
            else
            {
                filterInfo.DateTo.Should().Be(DateTime.MinValue);
            }
        }

        [Test]
        public void FilterInfo_ByAuthor_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByAuthor.Should().BeFalse();

            filterInfo.ByAuthor = true;
            filterInfo.ByAuthor.Should().BeTrue();

            filterInfo.ByAuthor = false;
            filterInfo.ByAuthor.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_Author_with_Raw_expected(bool isRaw, bool byAuthor)
        {
            const string value = "The Value";
            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByAuthor = byAuthor,
                Author = value
            };

            if (isRaw || byAuthor)
            {
                filterInfo.Author.Should().Be(value);
            }
            else
            {
                filterInfo.Author.Should().BeEmpty();
            }
        }

        [Test]
        public void FilterInfo_ByCommitter_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByCommitter.Should().BeFalse();

            filterInfo.ByCommitter = true;
            filterInfo.ByCommitter.Should().BeTrue();

            filterInfo.ByCommitter = false;
            filterInfo.ByCommitter.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_Committer_with_Raw_expected(bool isRaw, bool byCommitter)
        {
            const string value = "The Value";
            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByCommitter = byCommitter,
                Committer = value
            };

            if (isRaw || byCommitter)
            {
                filterInfo.Committer.Should().Be(value);
            }
            else
            {
                filterInfo.Committer.Should().BeEmpty();
            }
        }

        [Test]
        public void FilterInfo_ByMessage_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByMessage.Should().BeFalse();

            filterInfo.ByMessage = true;
            filterInfo.ByMessage.Should().BeTrue();

            filterInfo.ByMessage = false;
            filterInfo.ByMessage.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_Message_with_Raw_expected(bool isRaw, bool byMessage)
        {
            const string value = "The Value";
            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByMessage = byMessage,
                Message = value
            };

            if (isRaw || byMessage)
            {
                filterInfo.Message.Should().Be(value);
            }
            else
            {
                filterInfo.Message.Should().BeEmpty();
            }
        }

        [Test]
        public void FilterInfo_ByDiffContent_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByDiffContent.Should().BeFalse();

            filterInfo.ByDiffContent = true;
            filterInfo.ByDiffContent.Should().BeTrue();

            filterInfo.ByDiffContent = false;
            filterInfo.ByDiffContent.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_DiffContent_with_Raw_expected(bool isRaw, bool byDiffContent)
        {
            const string value = "The Value";
            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByDiffContent = byDiffContent,
                DiffContent = value
            };

            if (isRaw || byDiffContent)
            {
                filterInfo.DiffContent.Should().Be(value);
            }
            else
            {
                filterInfo.DiffContent.Should().BeEmpty();
            }
        }

        [Test]
        public void FilterInfo_IgnoreCase_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.IgnoreCase.Should().BeTrue();

            filterInfo.IgnoreCase = false;
            filterInfo.IgnoreCase.Should().BeFalse();

            filterInfo.IgnoreCase = true;
            filterInfo.IgnoreCase.Should().BeTrue();
        }

        [Test]
        public void FilterInfo_HasCommitsLimit_expected()
        {
            FilterInfo filterInfo = new() { IsRaw = true };
            filterInfo.HasCommitsLimit.Should().BeTrue();

            filterInfo.ByCommitsLimit = true;
            filterInfo.CommitsLimit = 0;
            filterInfo.HasCommitsLimit.Should().BeFalse();
        }

        [Test]
        public void FilterInfo_ByCommitsLimit_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByCommitsLimit.Should().BeFalse();

            filterInfo.ByCommitsLimit = true;
            filterInfo.ByCommitsLimit.Should().BeTrue();

            filterInfo.ByCommitsLimit = false;
            filterInfo.ByCommitsLimit.Should().BeFalse();
        }

        [TestCase(0, false)]
        [TestCase(100, false)]
        [TestCase(-100, true)]
        [TestCase(0, true)]
        [TestCase(100, true)]
        [TestCase(-100, true)]
        public void FilterInfo_CommitsLimit_with_Raw_expected(int value, bool byCommitsLimit)
        {
            FilterInfo filterInfo = new()
            {
                ByCommitsLimit = byCommitsLimit,
                CommitsLimit = value
            };

            if (byCommitsLimit && value >= 0)
            {
                filterInfo.CommitsLimit.Should().Be(value);
            }
            else
            {
                filterInfo.CommitsLimit.Should().Be(AppSettings.MaxRevisionGraphCommits);
            }
        }

        [Test]
        public void FilterInfo_ByPathFilter_expected()
        {
            FilterInfo filterInfo = new();
            filterInfo.ByPathFilter.Should().BeFalse();

            filterInfo.ByPathFilter = true;
            filterInfo.ByPathFilter.Should().BeTrue();

            filterInfo.ByPathFilter = false;
            filterInfo.ByPathFilter.Should().BeFalse();
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_PathFilter_with_Raw_expected(bool isRaw, bool byPathFilter)
        {
            const string value = "The Value";
            FilterInfo filterInfo = new()
            {
                IsRaw = isRaw,
                ByPathFilter = byPathFilter,
                PathFilter = value
            };

            if (isRaw || byPathFilter)
            {
                filterInfo.PathFilter.Should().Be(value);
            }
            else
            {
                filterInfo.PathFilter.Should().BeEmpty();
            }
        }

        [Test]
        public void FilterInfo_ByBranchFilter_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ByBranchFilter.Should().BeFalse();
                AppSettings.BranchFilterEnabled.Should().BeFalse();

                filterInfo.ByBranchFilter = true;
                filterInfo.ByBranchFilter.Should().BeTrue();
                AppSettings.BranchFilterEnabled.Should().BeTrue();

                filterInfo.ByBranchFilter = false;
                filterInfo.ByBranchFilter.Should().BeFalse();
                AppSettings.BranchFilterEnabled.Should().BeFalse();

                AppSettings.BranchFilterEnabled = true;
                filterInfo.ByBranchFilter.Should().BeTrue();
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_BranchFilter_with_Raw_expected(bool isRaw, bool byBranchFilter)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                const string value = "The Value";
                FilterInfo filterInfo = new()
                {
                    IsRaw = isRaw,
                    ByBranchFilter = byBranchFilter,
                    BranchFilter = value
                };

                if (isRaw || byBranchFilter)
                {
                    filterInfo.BranchFilter.Should().Be(value);
                }
                else
                {
                    filterInfo.BranchFilter.Should().BeEmpty();
                }
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        public void FilterInfo_IsShowAllBranchesChecked_expected(bool byBranchFilter, bool showCurrentBranchOnly, bool expected)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ShowReflogReferences = false,
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly,
                };

                filterInfo.IsShowAllBranchesChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, true)]
        [TestCase(true, true, true)]
        public void FilterInfo_IsShowCurrentBranchOnlyChecked_expected(bool byBranchFilter, bool showCurrentBranchOnly, bool expected)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ShowReflogReferences = false,
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly
                };

                filterInfo.IsShowCurrentBranchOnlyChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [TestCase(false, false, false)]
        [TestCase(true, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        public void FilterInfo_IsShowFilteredBranchesChecked_expected(bool byBranchFilter, bool showCurrentBranchOnly, bool expected)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ShowReflogReferences = false,
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly
                };

                filterInfo.IsShowFilteredBranchesChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [Test]
        public void FilterInfo_ShowCurrentBranchOnly_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ShowCurrentBranchOnly.Should().BeFalse();
                AppSettings.ShowCurrentBranchOnly.Should().BeFalse();

                filterInfo.ShowCurrentBranchOnly = true;
                filterInfo.ShowCurrentBranchOnly.Should().BeTrue();
                AppSettings.ShowCurrentBranchOnly.Should().BeTrue();

                filterInfo.ShowCurrentBranchOnly = false;
                filterInfo.ShowCurrentBranchOnly.Should().BeFalse();
                AppSettings.ShowCurrentBranchOnly.Should().BeFalse();

                AppSettings.ShowCurrentBranchOnly = true;
                filterInfo.ShowCurrentBranchOnly.Should().BeTrue();
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [Test]
        public void FilterInfo_ShowOnlyFirstParent_expected()
        {
            bool originalShowOnlyFirstParent = AppSettings.ShowOnlyFirstParent;
            AppSettings.ShowOnlyFirstParent = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ShowOnlyFirstParent.Should().BeFalse();
                AppSettings.ShowOnlyFirstParent.Should().BeFalse();

                filterInfo.ShowOnlyFirstParent = true;
                filterInfo.ShowOnlyFirstParent.Should().BeTrue();
                AppSettings.ShowOnlyFirstParent.Should().BeTrue();

                filterInfo.ShowOnlyFirstParent = false;
                filterInfo.ShowOnlyFirstParent.Should().BeFalse();
                AppSettings.ShowOnlyFirstParent.Should().BeFalse();

                AppSettings.ShowOnlyFirstParent = true;
                filterInfo.ShowOnlyFirstParent.Should().BeTrue();
            }
            finally
            {
                AppSettings.ShowOnlyFirstParent = originalShowOnlyFirstParent;
            }
        }

        [Test]
        public void FilterInfo_ShowReflogReferences_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ShowReflogReferences.Should().BeFalse();
                AppSettings.ShowReflogReferences.Should().BeFalse();

                filterInfo.ShowReflogReferences = true;
                filterInfo.ShowReflogReferences.Should().BeTrue();
                AppSettings.ShowReflogReferences.Should().BeTrue();

                filterInfo.ShowReflogReferences = false;
                filterInfo.ShowReflogReferences.Should().BeFalse();
                AppSettings.ShowReflogReferences.Should().BeFalse();

                AppSettings.ShowReflogReferences = true;
                filterInfo.ShowReflogReferences.Should().BeTrue();
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, true, true)]
        public void FilterInfo_ShowReflogReferences_dominates_other_filters(bool byBranchFilter, bool showCurrentBranchOnly, bool showReflog)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;
            AppSettings.ShowCurrentBranchOnly = false;
            AppSettings.BranchFilterEnabled = false;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly,
                    ShowReflogReferences = showReflog
                };

                filterInfo.ShowReflogReferences.Should().Be(showReflog);

                // showCurrentBranchOnly dominates byBranchFilter
                filterInfo.IsShowAllBranchesChecked.Should().Be(!byBranchFilter && !showCurrentBranchOnly && !showReflog);
                filterInfo.IsShowCurrentBranchOnlyChecked.Should().Be(showCurrentBranchOnly && !showReflog);
                filterInfo.IsShowFilteredBranchesChecked.Should().Be(byBranchFilter && !showCurrentBranchOnly && !showReflog);

                filterInfo.ByBranchFilter.Should().Be(byBranchFilter);
                AppSettings.BranchFilterEnabled.Should().Be(byBranchFilter);
                filterInfo.ShowCurrentBranchOnly.Should().Be(showCurrentBranchOnly);
                AppSettings.ShowCurrentBranchOnly.Should().Be(showCurrentBranchOnly);
            }
            finally
            {
                AppSettings.ShowReflogReferences = false;
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
                AppSettings.ShowReflogReferences = originalShowReflogReferences;
            }
        }

        [Test]
        public void FilterInfo_ShowSimplifyByDecoration_expected()
        {
            bool originalShowSimplifyByDecoration = AppSettings.ShowSimplifyByDecoration;
            AppSettings.ShowSimplifyByDecoration = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ShowSimplifyByDecoration.Should().BeFalse();
                AppSettings.ShowSimplifyByDecoration.Should().BeFalse();

                filterInfo.ShowSimplifyByDecoration = true;
                filterInfo.ShowSimplifyByDecoration.Should().BeTrue();
                AppSettings.ShowSimplifyByDecoration.Should().BeTrue();

                filterInfo.ShowSimplifyByDecoration = false;
                filterInfo.ShowSimplifyByDecoration.Should().BeFalse();
                AppSettings.ShowSimplifyByDecoration.Should().BeFalse();

                AppSettings.ShowSimplifyByDecoration = true;
                filterInfo.ShowSimplifyByDecoration.Should().BeTrue();
            }
            finally
            {
                AppSettings.ShowSimplifyByDecoration = originalShowSimplifyByDecoration;
            }
        }

        public static IEnumerable<TestCaseData> FilterInfo_HasFilterTestCases
        {
            get
            {
                foreach (bool byDateFrom in new[] { false, true })
                {
                    foreach (bool byDateTo in new[] { false, true })
                    {
                        foreach (bool byAuthor in new[] { false, true })
                        {
                            foreach (bool byCommitter in new[] { false, true })
                            {
                                foreach (bool byMessage in new[] { false, true })
                                {
                                    foreach (bool byDiffContent in new[] { false, true })
                                    {
                                        foreach (bool showSimplifyByDecoration in new[] { false, true })
                                        {
                                            foreach (bool showMergeCommits in new[] { false, true })
                                            {
                                                foreach (string pathFilter in new[] { "file1", "", null })
                                                {
                                                    foreach (bool showCurrentBranchOnly in new[] { false, true })
                                                    {
                                                        foreach (string branchFilter in new[] { "branch1", "", null })
                                                        {
                                                            yield return new TestCaseData(byDateFrom, byDateTo, byAuthor, byCommitter, byMessage, byDiffContent, showSimplifyByDecoration, showMergeCommits, pathFilter, showCurrentBranchOnly, branchFilter);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestCaseSource(nameof(FilterInfo_HasFilterTestCases))]
        public void FilterInfo_HasFilter_expected(bool byDateFrom, bool byDateTo, bool byAuthor, bool byCommitter, bool byMessage, bool byDiffContent, bool showSimplifyByDecoration, bool showMergeCommits, string pathFilter, bool showCurrentBranchOnly, string branchFilter)
        {
            FilterInfo filterInfo = new()
            {
                ByDateFrom = byDateFrom,
                ByDateTo = byDateTo,
                ByAuthor = byAuthor,
                ByCommitter = byCommitter,
                ByMessage = byMessage,
                ByDiffContent = byDiffContent,
                ShowSimplifyByDecoration = showSimplifyByDecoration,
                ShowMergeCommits = showMergeCommits,
                ByPathFilter = true,
                PathFilter = pathFilter,
                ShowCurrentBranchOnly = showCurrentBranchOnly,
                ByBranchFilter = true,
                BranchFilter = branchFilter
            };

            filterInfo.HasFilter.Should().Be(byDateFrom || byDateTo || byAuthor || byCommitter || byMessage || byDiffContent || showSimplifyByDecoration || !showMergeCommits || !string.IsNullOrWhiteSpace(pathFilter) || showCurrentBranchOnly || !string.IsNullOrWhiteSpace(branchFilter));
        }

        [TestCaseSource(nameof(FilterInfo_HasFilterTestCases))]
        public void FilterInfo_ResetAllFilters_expected(bool byDateFrom, bool byDateTo, bool byAuthor, bool byCommitter, bool byMessage, bool byDiffContent, bool showSimplifyByDecoration, bool showMergeCommits, string pathFilter, bool showCurrentBranchOnly, string branchFilter)
        {
            FilterInfo filterInfo = new()
            {
                ByDateFrom = byDateFrom,
                ByDateTo = byDateTo,
                ByAuthor = byAuthor,
                ByCommitter = byCommitter,
                ByMessage = byMessage,
                ByDiffContent = byDiffContent,
                ShowSimplifyByDecoration = showSimplifyByDecoration,
                ShowMergeCommits = showMergeCommits,
                ByPathFilter = true,
                PathFilter = pathFilter,
                ShowCurrentBranchOnly = showCurrentBranchOnly,
                ByBranchFilter = true,
                BranchFilter = branchFilter
            };

            filterInfo.ResetAllFilters();

            filterInfo.ByDateFrom.Should().BeFalse();
            filterInfo.ByDateTo.Should().BeFalse();
            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByDiffContent.Should().BeFalse();
            filterInfo.ShowSimplifyByDecoration.Should().BeFalse();
            filterInfo.ShowMergeCommits.Should().BeTrue();
            filterInfo.ByPathFilter.Should().BeFalse();
            filterInfo.ShowCurrentBranchOnly.Should().BeFalse();
            filterInfo.ByBranchFilter.Should().BeFalse();
        }

        [TestCase("author1", "committer2", "message3", "diffContent4", true, false, "pathFilter7", false, false, "branchFilter8",
            "Path filter: pathFilter7\r\nBranches: branchFilter8\r\nAuthor: author1\r\nCommitter: committer2\r\nMessage: message3\r\nDiff contains: diffContent4\r\nSince: 10/1/2021 1:30:34 AM\r\nUntil: 11/1/2021 1:30:34 AM\r\nSimplify by decoration\r\n",
            @"--max-count=100000 --glob=refs/stas[h] branchFilter8 --parents --no-merges --simplify-by-decoration --author=""author1"" --committer=""committer2"" --grep=""message3"" -G""diffContent4"" --regexp-ignore-case --since=""2021-10-01 01:30:34"" --until=""2021-11-01 01:30:34""")]
        public void FilterInfo_GetRevisionFilter(string author, string committer, string message, string diffContent, bool showSimplifyByDecoration, bool showMergeCommits, string pathFilter, bool showReflog, bool showCurrentBranchOnly, string branchFilter, string expectedSummary, string expectedArgs)
        {
            DateTime dateFrom = new(2021, 10, 1, 1, 30, 34, DateTimeKind.Local);
            DateTime dateTo = new(2021, 11, 1, 1, 30, 34, DateTimeKind.Local);
            FilterInfo filterInfo = new()
            {
                DateFrom = dateFrom,
                ByDateFrom = dateFrom != DateTime.MinValue,
                DateTo = dateTo,
                ByDateTo = dateTo != DateTime.MinValue,
                Author = author,
                ByAuthor = !string.IsNullOrEmpty(author),
                Committer = committer,
                ByCommitter = !string.IsNullOrEmpty(committer),
                Message = message,
                ByMessage = !string.IsNullOrEmpty(message),
                DiffContent = diffContent,
                ByDiffContent = !string.IsNullOrEmpty(message),
                ShowSimplifyByDecoration = showSimplifyByDecoration,
                ShowMergeCommits = showMergeCommits,
                PathFilter = pathFilter,
                ByPathFilter = !string.IsNullOrEmpty(pathFilter),
                ShowReflogReferences = showReflog,
                ShowCurrentBranchOnly = showCurrentBranchOnly,
                ByBranchFilter = !string.IsNullOrEmpty(branchFilter),
                BranchFilter = branchFilter
            };

            filterInfo.GetSummary().Should().Be(expectedSummary);
            filterInfo.GetRevisionFilter().ToString().Should().Be(expectedArgs);
        }

        [TestCase(false, false, "branchFilter")]
        [TestCase(false, false, "")]
        [TestCase(false, true, "branchFilter")]
        [TestCase(false, true, "")]
        [TestCase(true, false, "branchFilter")]
        [TestCase(true, false, "")]
        [TestCase(true, true, "branchFilter")]
        [TestCase(true, true, "")]
        public void FilterInfo_filter_should_add_reflog_if_requested(bool showRefLog, bool showCurrentBranchOnly, string branchFilter)
        {
            FilterInfo filterInfo = new()
            {
                ShowReflogReferences = showRefLog,
                ShowCurrentBranchOnly = showCurrentBranchOnly,
                ByBranchFilter = !string.IsNullOrEmpty(branchFilter),
                BranchFilter = branchFilter
            };

            string args = filterInfo.GetRevisionFilter();

            if (showRefLog)
            {
                args.ToString().Should().MatchRegex(@"(^|\s)--reflog($|\s)");
            }
            else
            {
                args.ToString().Should().NotMatchRegex(@"(^|\s)--reflog($|\s)");
            }

            if (!showRefLog && showCurrentBranchOnly)
            {
                args.ToString().Should().NotMatchRegex(@"(^|\s)--all($|\s)");
                args.ToString().Should().NotMatchRegex(@"(^|\s)branchFilter($|\s)");
            }

            if (!showRefLog && !showCurrentBranchOnly && !string.IsNullOrWhiteSpace(branchFilter))
            {
                args.ToString().Should().MatchRegex(@"(^|\s)branchFilter($|\s)");
            }
        }

        [TestCase("branchFilter", false)]
        [TestCase(@"branchFilter*", true)]
        [TestCase(@"branchFilter?", true)]
        [TestCase(@"branchFilter[", true)]
        public void FilterInfo_simple_branchfilter(string branchFilter, bool expectBranches)
        {
            FilterInfo filterInfo = new()
            {
                ShowReflogReferences = false,
                ShowCurrentBranchOnly = false,
                ByBranchFilter = !string.IsNullOrEmpty(branchFilter),
                BranchFilter = branchFilter
            };

            string args = filterInfo.GetRevisionFilter();

            if (expectBranches)
            {
                args.ToString().Should().MatchRegex(@$"(^|\s)--branches={Regex.Escape(branchFilter)}($|\s)");
            }
            else
            {
                args.ToString().Should().MatchRegex(@$"(^|\s){Regex.Escape(branchFilter)}($|\s)");
                args.ToString().Should().NotMatchRegex(@$"(^|\s)--branches={Regex.Escape(branchFilter)}($|\s)");
            }
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void FilterInfo_filter_should_add_maxcount_if_requested(int maxCount, bool expected)
        {
            FilterInfo filterInfo = new()
            {
                CommitsLimit = maxCount,
                ByCommitsLimit = true
            };
            string args = filterInfo.GetRevisionFilter();

            if (expected)
            {
                args.ToString().Should().MatchRegex(@$"(^|\s)--max-count={maxCount}($|\s)");
            }
            else
            {
                args.ToString().Should().NotMatchRegex(@"(^|\s)--max-count=\d+($|\s)");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FilterInfo_OnlyFirstParent(bool expected)
        {
            FilterInfo filterInfo = new()
            {
                ShowOnlyFirstParent = expected
            };

            string args = filterInfo.GetRevisionFilter();

            if (expected)
            {
                args.ToString().Should().MatchRegex(@"(^|\s)--first-parent($|\s)");
            }
            else
            {
                args.ToString().Should().NotMatchRegex(@"(^|\s)--first-parent($|\s)");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FilterInfo_NoMerges(bool expected)
        {
            FilterInfo filterInfo = new()
            {
                ShowMergeCommits = expected
            };

            string args = filterInfo.GetRevisionFilter();

            if (!expected)
            {
                args.ToString().Should().MatchRegex(@"(^|\s)--no-merges($|\s)");
            }
            else
            {
                args.ToString().Should().NotMatchRegex(@"(^|\s)--no-merges($|\s)");
            }
        }

        public static IEnumerable<TestCaseData> FilterInfo_NotesStash
        {
            get
            {
                foreach (bool showNotes in new[] { false, true })
                {
                    foreach (bool showStash in new[] { false, true })
                    {
                        foreach (bool showReflog in new[] { false, true })
                        {
                            foreach (bool showCurrentBranchOnly in new[] { false, true })
                            {
                                foreach (string branchFilter in new[] { "branch1", "", null })
                                {
                                    yield return new TestCaseData(showNotes, showStash, showReflog, showCurrentBranchOnly, branchFilter);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestCaseSource(nameof(FilterInfo_NotesStash))]
        public void FilterInfo_GitNotes_Stashes(bool showGitNotes, bool showStash, bool showReflog, bool showCurrentBranchOnly, string branchFilter)
        {
            bool originalShowGitNotes = AppSettings.ShowGitNotes;
            AppSettings.ShowGitNotes = showGitNotes;
            bool originalShowStash = AppSettings.ShowStashes;
            AppSettings.ShowStashes = showStash;
            FilterInfo filterInfo = new()
            {
                ShowReflogReferences = showReflog,
                ShowCurrentBranchOnly = showCurrentBranchOnly,
                ByBranchFilter = true,
                BranchFilter = branchFilter
            };
            string args = filterInfo.GetRevisionFilter();
            bool showAll = !showReflog && !showCurrentBranchOnly && string.IsNullOrWhiteSpace(branchFilter);
            bool showCurrent = !showReflog && showCurrentBranchOnly;
            bool showFiltredOrCurrent = !showReflog && (showCurrentBranchOnly || !string.IsNullOrWhiteSpace(branchFilter));

            try
            {
                if (showAll && !showGitNotes)
                {
                    args.ToString().Should().MatchRegex(@"(^|\s)--exclude=refs/notes/commits($|\s)");
                }
                else
                {
                    args.ToString().Should().NotMatchRegex(@"(^|\s)--exclude=refs/notes/commits($|\s)");
                }

                if (showAll && !showStash)
                {
                    args.ToString().Should().MatchRegex(@"(^|\s)--exclude=refs/stash($|\s)");
                }
                else
                {
                    args.ToString().Should().NotMatchRegex(@"(^|\s)--exclude=refs/stash($|\s)");
                }

                if (showCurrent && showStash)
                {
                    args.ToString().Should().MatchRegex(@"(^|\s)HEAD($|\s)");
                }
                else
                {
                    args.ToString().Should().NotMatchRegex(@"(^|\s)HEAD($|\s)");
                }

                string stash = Regex.Escape("--glob=refs/stas[h]");
                if (showFiltredOrCurrent && showStash)
                {
                    args.ToString().Should().MatchRegex(@$"(^|\s){stash}($|\s)");
                }
                else
                {
                    args.ToString().Should().NotMatchRegex(@$"(^|\s){stash}($|\s)");
                }
            }
            finally
            {
                AppSettings.ShowGitNotes = originalShowGitNotes;
                AppSettings.ShowStashes = originalShowStash;
            }
        }

        [Test]
        public void FilterInfo_Apply_ByCommitMessage()
        {
            FilterInfo filterInfo = new();
            var filterLaunched = filterInfo.Apply(new RevisionFilter("message", byCommit: true, byCommitter: false, byAuthor: false, byDiffContent: false));

            filterLaunched.Should().BeTrue();
            filterInfo.ByMessage.Should().BeTrue();
            filterInfo.Message.Should().Be("message");

            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
            filterInfo.ByDiffContent.Should().BeFalse();
        }

        [Test]
        public void FilterInfo_Apply_ByCommitter()
        {
            FilterInfo filterInfo = new();
            var filterLaunched = filterInfo.Apply(new RevisionFilter("committer", byCommit: false, byCommitter: true, byAuthor: false, byDiffContent: false));

            filterLaunched.Should().BeTrue();
            filterInfo.ByCommitter.Should().BeTrue();
            filterInfo.Committer.Should().Be("committer");

            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByDiffContent.Should().BeFalse();
        }

        [Test]
        public void FilterInfo_Apply_ByAuthor()
        {
            FilterInfo filterInfo = new();
            var filterLaunched = filterInfo.Apply(new RevisionFilter("author", byCommit: false, byCommitter: false, byAuthor: true, byDiffContent: false));

            filterLaunched.Should().BeTrue();
            filterInfo.ByAuthor.Should().BeTrue();
            filterInfo.Author.Should().Be("author");

            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
            filterInfo.ByDiffContent.Should().BeFalse();
        }

        [Test]
        public void FilterInfo_Apply_ByDiffContent()
        {
            FilterInfo filterInfo = new();
            var filterLaunched = filterInfo.Apply(new RevisionFilter("diff", byCommit: false, byCommitter: false, byAuthor: false, byDiffContent: true));

            filterLaunched.Should().BeTrue();
            filterInfo.ByDiffContent.Should().BeTrue();
            filterInfo.DiffContent.Should().Be("diff");

            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
        }

        [Test]
        public void FilterInfo_Apply_ByMessage_Then_By_DiffContent()
        {
            // FilterInfo keep a state, so simulating actions of user where filter is changed 2 times
            // to ensure that the filtering will be triggered i.e. returned value is always `true`

            FilterInfo filterInfo = new();
            var filterLaunched = filterInfo.Apply(new RevisionFilter("a_content", byCommit: true, byCommitter: false, byAuthor: false, byDiffContent: false));

            filterLaunched.Should().BeTrue();
            filterInfo.ByMessage.Should().BeTrue();
            filterInfo.Message.Should().Be("a_content");

            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
            filterInfo.ByDiffContent.Should().BeFalse();

            filterLaunched = filterInfo.Apply(new RevisionFilter("a_content", byCommit: true, byCommitter: false, byAuthor: false, byDiffContent: true));

            filterLaunched.Should().BeTrue();
            filterInfo.ByMessage.Should().BeTrue();
            filterInfo.Message.Should().Be("a_content");
            filterInfo.ByDiffContent.Should().BeTrue();
            filterInfo.DiffContent.Should().Be("a_content");

            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();

            filterLaunched = filterInfo.Apply(new RevisionFilter("a_content", byCommit: false, byCommitter: false, byAuthor: false, byDiffContent: true));

            filterLaunched.Should().BeTrue();
            filterInfo.ByDiffContent.Should().BeTrue();
            filterInfo.DiffContent.Should().Be("a_content");

            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
        }
    }
}
