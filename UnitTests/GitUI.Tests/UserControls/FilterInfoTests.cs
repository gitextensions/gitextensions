using System;
using System.Collections.Generic;
using ApprovalTests;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using LibGit2Sharp;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class FilterInfoTests
    {
        [Test]
        public void FilterInfo_ctor_expected()
        {
            FilterInfo filterInfo = new();
            Approvals.Verify(filterInfo);
        }

        [Test]
        public void FilterInfo_ctor_with_Raw_expected()
        {
            FilterInfo filterInfo = new() { IsRaw = true };
            Approvals.Verify(filterInfo);
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
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_BranchFilter_with_Raw_expected(bool isRaw, bool byBranchFilter)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;

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
            }
        }

        // TODO: RefFilterOptions

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void FilterInfo_IsShowAllBranchesChecked_expected(bool byBranchFilter, bool expected)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ByBranchFilter = byBranchFilter,
                };

                filterInfo.IsShowAllBranchesChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
            }
        }

        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, true)]
        public void FilterInfo_IsShowCurrentBranchOnlyChecked_expected(bool byBranchFilter, bool showCurrentBranchOnly, bool expected)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly
                };

                filterInfo.IsShowCurrentBranchOnlyChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
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

            try
            {
                FilterInfo filterInfo = new()
                {
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly
                };

                filterInfo.IsShowFilteredBranchesChecked.Should().Be(expected);
            }
            finally
            {
                AppSettings.BranchFilterEnabled = originalBranchFilterEnabled;
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
            }
        }

        [Test]
        public void FilterInfo_ShowCurrentBranchOnly_expected()
        {
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            AppSettings.ShowCurrentBranchOnly = false;

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
                AppSettings.ShowCurrentBranchOnly = originalShowCurrentBranchOnly;
            }
        }

        [Test]
        public void FilterInfo_ShowFirstParent_expected()
        {
            bool originalShowFirstParent = AppSettings.ShowFirstParent;
            AppSettings.ShowFirstParent = false;

            try
            {
                FilterInfo filterInfo = new();
                filterInfo.ShowFirstParent.Should().BeFalse();
                AppSettings.ShowFirstParent.Should().BeFalse();

                filterInfo.ShowFirstParent = true;
                filterInfo.ShowFirstParent.Should().BeTrue();
                AppSettings.ShowFirstParent.Should().BeTrue();

                filterInfo.ShowFirstParent = false;
                filterInfo.ShowFirstParent.Should().BeFalse();
                AppSettings.ShowFirstParent.Should().BeFalse();

                AppSettings.ShowFirstParent = true;
                filterInfo.ShowFirstParent.Should().BeTrue();
            }
            finally
            {
                AppSettings.ShowFirstParent = originalShowFirstParent;
            }
        }

        [Test]
        public void FilterInfo_ShowReflogReferences_expected()
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;

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

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FilterInfo_ShowReflogReferences_resets_filters(bool byBranchFilter, bool showCurrentBranchOnly)
        {
            bool originalBranchFilterEnabled = AppSettings.BranchFilterEnabled;
            bool originalShowCurrentBranchOnly = AppSettings.ShowCurrentBranchOnly;
            bool originalShowReflogReferences = AppSettings.ShowReflogReferences;
            AppSettings.ShowReflogReferences = false;

            try
            {
                FilterInfo filterInfo = new()
                {
                    ByBranchFilter = byBranchFilter,
                    ShowCurrentBranchOnly = showCurrentBranchOnly
                };

                filterInfo.ShowReflogReferences = true;

                filterInfo.ByBranchFilter.Should().BeFalse();
                filterInfo.ShowCurrentBranchOnly.Should().BeFalse();
            }
            finally
            {
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
                                    foreach (bool byPathFilter in new[] { false, true })
                                    {
                                        foreach (bool byBranchFilter in new[] { false, true })
                                        {
                                            yield return new TestCaseData(byDateFrom, byDateTo, byAuthor, byCommitter, byMessage, byPathFilter, byBranchFilter);
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
        public void FilterInfo_HasFilter_expected(bool byDateFrom, bool byDateTo, bool byAuthor, bool byCommitter, bool byMessage, bool byPathFilter, bool byBranchFilter)
        {
            FilterInfo filterInfo = new()
            {
                ByDateFrom = byDateFrom,
                ByDateTo = byDateTo,
                ByAuthor = byAuthor,
                ByCommitter = byCommitter,
                ByMessage = byMessage,
                ByPathFilter = byPathFilter,
                ByBranchFilter = byBranchFilter
            };

            filterInfo.HasFilter.Should().Be(byDateFrom || byDateTo || byAuthor || byCommitter || byMessage || byPathFilter || byBranchFilter);
        }

        // TODO: Apply

        [TestCaseSource(nameof(FilterInfo_HasFilterTestCases))]
        public void FilterInfo_DisableFilters_expected(bool byDateFrom, bool byDateTo, bool byAuthor, bool byCommitter, bool byMessage, bool byPathFilter, bool byBranchFilter)
        {
            FilterInfo filterInfo = new()
            {
                ByDateFrom = byDateFrom,
                ByDateTo = byDateTo,
                ByAuthor = byAuthor,
                ByCommitter = byCommitter,
                ByMessage = byMessage,
                ByPathFilter = byPathFilter,
                ByBranchFilter = byBranchFilter
            };

            filterInfo.DisableFilters();

            filterInfo.ByDateFrom.Should().BeFalse();
            filterInfo.ByDateTo.Should().BeFalse();
            filterInfo.ByAuthor.Should().BeFalse();
            filterInfo.ByCommitter.Should().BeFalse();
            filterInfo.ByMessage.Should().BeFalse();
            filterInfo.ByPathFilter.Should().BeFalse();
            filterInfo.ByBranchFilter.Should().BeFalse();
        }

        // TODO: GetRevisionFilter
    }
}
