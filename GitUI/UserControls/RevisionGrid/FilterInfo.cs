using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms.VisualStyles;
using GitCommands;
using GitExtUtils;
using GitUI.UserControls.RevisionGrid.Graph;

namespace GitUI.UserControls.RevisionGrid
{
    public record FilterInfo
    {
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private string _author = string.Empty;
        private string _committer = string.Empty;
        private string _message = string.Empty;
        private string _content = string.Empty;
        private string _pathFilter = string.Empty;
        private string _branchFilter = string.Empty;
        private int _commitsLimit = -1;

        /// <summary>
        ///  Gets whether all properties will unconditionally return the underlying data.
        ///  Otherwise return values will depend on the respective filter, e.g. "get => ByXyz ? Xyz : default".
        /// </summary>
        public bool IsRaw { get; init; } = false;

        public bool ByDateFrom { get; set; }

        public DateTime DateFrom
        {
            get => GetValue(ByDateFrom, _dateFrom, DateTime.MinValue);
            set => _dateFrom = value;
        }

        public bool ByDateTo { get; set; }

        public DateTime DateTo
        {
            get => GetValue(ByDateTo, _dateTo, DateTime.MinValue);
            set => _dateTo = value;
        }

        public bool ByAuthor { get; set; }

        public string Author
        {
            get => GetValue(ByAuthor, _author, string.Empty);
            set => _author = value ?? string.Empty;
        }

        public bool ByCommitter { get; set; }

        public string Committer
        {
            get => GetValue(ByCommitter, _committer, string.Empty);
            set => _committer = value ?? string.Empty;
        }

        public bool ByMessage { get; set; }

        public string Message
        {
            get => GetValue(ByMessage, _message, string.Empty);
            set => _message = value ?? string.Empty;
        }

        public bool ByDiffContent { get; set; }

        public string DiffContent
        {
            get => GetValue(ByDiffContent, _content, string.Empty);
            set => _content = value ?? string.Empty;
        }

        public bool IgnoreCase { get; set; } = true;

        public bool HasCommitsLimit { get => CommitsLimit > 0; }

        public bool ByCommitsLimit { get; set; } = false;

        public int CommitsLimitDefault => AppSettings.MaxRevisionGraphCommits;

        public int CommitsLimit
        {
            get => ByCommitsLimit && _commitsLimit >= 0 ? _commitsLimit : CommitsLimitDefault;
            set => _commitsLimit = value;
        }

        public bool ByPathFilter { get; set; }

        public string PathFilter
        {
            get => GetValue(ByPathFilter, _pathFilter, string.Empty);
            set => _pathFilter = value ?? string.Empty;
        }

        // Controls if BranchFilter is valid or not
        // An empty filter must still be handled as all branches
        public bool ByBranchFilter
        {
            get => AppSettings.BranchFilterEnabled;
            set => AppSettings.BranchFilterEnabled = value;
        }

        public string BranchFilter
        {
            get => GetValue(ByBranchFilter, _branchFilter, string.Empty);
            set => _branchFilter = value ?? string.Empty;
        }

        public bool IsShowAllBranchesChecked => !ByBranchFilter && !ShowCurrentBranchOnly && !ShowReflogReferences;

        public bool IsShowCurrentBranchOnlyChecked => ShowCurrentBranchOnly && !ShowReflogReferences;

        // IsChecked is not the same as a filter is active, see ByBranchFilter
        public bool IsShowFilteredBranchesChecked => ByBranchFilter && !ShowCurrentBranchOnly && !ShowReflogReferences;

        public bool ShowCurrentBranchOnly
        {
            get => AppSettings.ShowCurrentBranchOnly;
            set => AppSettings.ShowCurrentBranchOnly = value;
        }

        public bool ShowOnlyFirstParent
        {
            get => AppSettings.ShowOnlyFirstParent;
            set => AppSettings.ShowOnlyFirstParent = value;
        }

        public bool ShowReflogReferences
        {
            get => AppSettings.ShowReflogReferences;
            set
            {
                // Do not unset ByBranchFilter or ShowCurrentBranchOnly.
                // ShowReflogReferences dominates those settings and if the user
                // toggles the Reflog button, the curremt branch fílter should appear.
                AppSettings.ShowReflogReferences = value;
            }
        }

        public bool ShowSimplifyByDecoration
        {
            get => AppSettings.ShowSimplifyByDecoration;
            set => AppSettings.ShowSimplifyByDecoration = value;
        }

        public bool ShowMergeCommits
        {
            get => AppSettings.ShowMergeCommits;
            set => AppSettings.ShowMergeCommits = value;
        }

        public bool ShowFullHistory
        {
            get => AppSettings.FullHistoryInFileHistory;
            set => AppSettings.FullHistoryInFileHistory = value;
        }

        public bool ShowSimplifyMerges
        {
            get => AppSettings.SimplifyMergesInFileHistory;
            set => AppSettings.SimplifyMergesInFileHistory = value;
        }

        /// <summary>
        /// Has any filters in addition to revision filters that sets the history.
        /// Currently, this is only branch filters.
        /// Note that All/Reflog are not considered as filters.
        /// </summary>
        public bool HasFilter
        {
            get => HasRevisionFilter
                || ShowCurrentBranchOnly
                || ShowOnlyFirstParent
                || !string.IsNullOrWhiteSpace(BranchFilter);
        }

        /// <summary>
        /// Has revision filters that hides parents, not just branches.
        /// </summary>
        public bool HasRevisionFilter
        {
            get => ByDateFrom
                || ByDateTo
                || ByAuthor
                || ByCommitter
                || ByMessage
                || ByDiffContent
                || !string.IsNullOrWhiteSpace(PathFilter)
                || !ShowMergeCommits
                || ShowSimplifyByDecoration;
        }

        /// <summary>
        /// Disables all active filters.
        /// Reflog is not disabled.
        /// FullHistory and SimplifyMerges are considered settings and not reset.
        /// </summary>
        public void ResetAllFilters()
        {
            ByDateFrom = false;
            ByDateTo = false;
            ByAuthor = false;
            ByCommitter = false;
            ByMessage = false;
            ByDiffContent = false;
            ByPathFilter = false;
            ByBranchFilter = false;
            ShowCurrentBranchOnly = false;
            ShowOnlyFirstParent = false;
            ShowMergeCommits = true;
            ShowSimplifyByDecoration = false;
        }

        /// <summary>
        ///  Applies the conditions from the supplied <paramref name="filter"/> only those are different from the current filter conditions.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <returns><see langword="true"/> if the current filter has changed; otherwise <see langword="false"/>.</returns>
        public bool Apply(RevisionFilter filter)
        {
            if (IsRaw)
            {
                Debug.Fail("Not supported");
            }

            bool searchParametersChanged = filter.FilterByAuthor != ByAuthor
                                          || filter.FilterByCommitter != ByCommitter
                                          || filter.FilterByCommit != ByMessage
                                          || filter.FilterByDiffContent != ByDiffContent;

            if (filter.FilterByAuthor)
            {
                if (!string.Equals(Author, filter.Text, StringComparison.CurrentCulture))
                {
                    ByAuthor = !string.IsNullOrWhiteSpace(filter.Text);
                    Author = filter.Text;
                    searchParametersChanged = true;
                }
            }
            else
            {
                ByAuthor = false;
            }

            if (filter.FilterByCommitter)
            {
                if (!string.Equals(Committer, filter.Text, StringComparison.CurrentCulture))
                {
                    ByCommitter = !string.IsNullOrWhiteSpace(filter.Text);
                    Committer = filter.Text;
                    searchParametersChanged = true;
                }
            }
            else
            {
                ByCommitter = false;
            }

            if (filter.FilterByCommit)
            {
                if (!string.Equals(Message, filter.Text, StringComparison.CurrentCulture))
                {
                    ByMessage = !string.IsNullOrWhiteSpace(filter.Text);
                    Message = filter.Text;
                    searchParametersChanged = true;
                }
            }
            else
            {
                ByMessage = false;
            }

            if (filter.FilterByDiffContent)
            {
                if (!string.Equals(DiffContent, filter.Text, StringComparison.CurrentCulture))
                {
                    ByDiffContent = !string.IsNullOrWhiteSpace(filter.Text);
                    DiffContent = filter.Text;
                    searchParametersChanged = true;
                }
            }
            else
            {
                ByDiffContent = false;
            }

            return searchParametersChanged;
        }

        public ArgumentString GetRevisionFilter()
        {
            if (IsRaw)
            {
                Debug.Fail("Not supported");
            }

            ArgumentBuilder filter = new();

            if (CommitsLimit > 0)
            {
                filter.Add($"--max-count={CommitsLimit}");
            }

            // Branch filters
            if (ShowReflogReferences)
            {
                // All commits
                filter.Add("--reflog");
            }
            else if (IsShowCurrentBranchOnlyChecked)
            {
                // Default git-log (no option), only current branch (HEAD)
            }
            else if (IsShowFilteredBranchesChecked && !string.IsNullOrWhiteSpace(BranchFilter))
            {
                // Show filtered branches only
                filter.Add(IsSimpleBranchFilter(BranchFilter) ? BranchFilter : "--branches=" + BranchFilter);
            }
            else
            {
                // refs (like notes) requires --reflog/--all or explicit inclusion (--glob)
                // (so included for --reflog/--all, not explicitly added for other)
                // --exclude is ignored for --reflog, only used with --all
                // Similar but unhandled refs include Gerrit refs like refs/for/ and refs/changes/
                if (!AppSettings.ShowGitNotes)
                {
                    filter.Add($"--exclude={GitRefName.RefsNotesPrefix}");
                }

                if (!AppSettings.ShowStashes)
                {
                    filter.Add($"--exclude={GitRefName.RefsStashPrefix}");
                }

                // All refs/
                filter.Add("--all");

                // The inclusion of boundary parents to matches is historical
                // (why Message etc is handled as a special case)
                if (!string.IsNullOrWhiteSpace(Message) && !string.IsNullOrWhiteSpace(DiffContent))
                {
                    filter.Add("--boundary");
                }
            }

            if (HasRevisionFilter)
            {
                filter.Add("--parents");
                if (ShowFullHistory)
                {
                    filter.Add("--full-history");
                    if (ShowSimplifyMerges)
                    {
                        filter.Add("--simplify-merges");
                    }
                }
            }

            if (!ShowMergeCommits)
            {
                filter.Add("--no-merges");
            }

            if (ShowOnlyFirstParent)
            {
                filter.Add("--first-parent");
            }

            // Listed in Git help as history simplification, but is a revision filter
            if (ShowSimplifyByDecoration)
            {
                filter.Add("--simplify-by-decoration");
            }

            if (ByAuthor && !string.IsNullOrWhiteSpace(Author))
            {
                filter.Add($"--author=\"{Author}\"");
            }

            if (ByCommitter && !string.IsNullOrWhiteSpace(Committer))
            {
                filter.Add($"--committer=\"{Committer}\"");
            }

            if (ByMessage && !string.IsNullOrEmpty(Message))
            {
                filter.Add($"--grep=\"{Message}\"");
            }

            if (ByDiffContent && !string.IsNullOrEmpty(DiffContent))
            {
                filter.Add($"-G\"{DiffContent}\"");
            }

            if (IgnoreCase && (ByAuthor || ByCommitter || ByMessage || ByDiffContent))
            {
                filter.Add("--regexp-ignore-case");
            }

            if (ByDateFrom)
            {
                filter.Add($"--since=\"{DateFrom:yyyy-MM-dd hh:mm:ss}\"");
            }

            if (ByDateTo)
            {
                filter.Add($"--until=\"{DateTo:yyyy-MM-dd hh:mm:ss}\"");
            }

            return filter;

            static bool IsSimpleBranchFilter(string branchFilter) =>
               branchFilter.IndexOfAny(new[] { '?', '*', '[' }) == -1;
        }

        public string GetSummary()
        {
            StringBuilder filter = new();

            // Ignore IgnoreCase, ShowMergeCommits, FullHistoryInFileHistory/SimplifyMergesInFileHistory (when history filtered)

            // path and revision filters always applies
            if (ByPathFilter)
            {
                filter.AppendLine($"{TranslatedStrings.PathFilter}: {PathFilter}");
            }

            if (ShowReflogReferences)
            {
                filter.AppendLine(TranslatedStrings.ShowReflog);
            }
            else if (ShowCurrentBranchOnly)
            {
                filter.AppendLine(TranslatedStrings.ShowCurrentBranchOnly);
            }
            else if (!string.IsNullOrWhiteSpace(BranchFilter))
            {
                filter.AppendLine($"{TranslatedStrings.Branches}: {BranchFilter}");
            }

            if (ByAuthor && !string.IsNullOrWhiteSpace(Author))
            {
                filter.AppendLine($"{TranslatedStrings.Author}: {Author}");
            }

            if (ByCommitter && !string.IsNullOrWhiteSpace(Committer))
            {
                filter.AppendLine($"{TranslatedStrings.Committer}: {Committer}");
            }

            if (ByMessage && !string.IsNullOrEmpty(Message))
            {
                filter.AppendLine($"{TranslatedStrings.Message}: {Message}");
            }

            if (ByDiffContent && !string.IsNullOrEmpty(DiffContent))
            {
                filter.AppendLine($"{TranslatedStrings.DiffContent}: {DiffContent}");
            }

            if (ByDateFrom)
            {
                filter.AppendLine($"{TranslatedStrings.Since}: {DateFrom}");
            }

            if (ByDateTo)
            {
                filter.AppendLine($"{TranslatedStrings.Until}: {DateTo}");
            }

            if (ShowOnlyFirstParent)
            {
                filter.AppendLine(TranslatedStrings.ShowOnlyFirstParent);
            }

            if (ShowSimplifyByDecoration)
            {
                filter.AppendLine($"{TranslatedStrings.SimplifyByDecoration}");
            }

            return filter.ToString();
        }

        private T GetValue<T>(bool condition, T valueTrue, T valueFalse)
        {
            if (IsRaw)
            {
                return valueTrue;
            }

            return condition ? valueTrue : valueFalse;
        }
    }
}
