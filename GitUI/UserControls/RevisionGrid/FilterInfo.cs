using System.Diagnostics;
using System.Text;
using GitCommands;
using GitExtUtils;

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
        /// Prefix to "Message" filters that forces the text to be interpreted as Git options.
        /// This enables use of options not available in the GUI.
        /// </summary>
        private readonly string[] _messageAsGitOptions = new[] { "--not ", "--exclude=" };

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
                // ShowReflogReferences dominates ByBranchFilter and ShowCurrentBranchOnly,
                // if the user toggles the Reflog button, the curremt branch fílter should appear.
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
        /// Currently, this is only branch and date filters.
        /// Note that Current/Reflog are not considered as filters (All is default).
        /// </summary>
        public bool HasFilter
        {
            get => HasRevisionFilter
                || ByDateFrom
                || ByDateTo
                || ShowOnlyFirstParent
                || !string.IsNullOrWhiteSpace(BranchFilter);
        }

        /// <summary>
        /// Has revision filters that potentially hides parents, not just branches.
        /// </summary>
        public bool HasRevisionFilter
        {
            get => ByAuthor
                || ByCommitter
                || ByMessage
                || ByDiffContent
                || !string.IsNullOrWhiteSpace(PathFilter)
                || !ShowMergeCommits
                || ShowSimplifyByDecoration;
        }

        /// <summary>
        /// Disables all active filters.
        /// CurrentBranch and Reflog are not disabled.
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

        public ArgumentString GetRevisionFilter(Lazy<string> currentBranch)
        {
            if (IsRaw)
            {
                Debug.Fail("Not supported");
            }

            ArgumentBuilder filter = new();

            // Separate the filters in groups
            GetCommitRevisionFilter(filter);
            GetLimitingRevisionFilter(filter);
            GetBranchRevisionFilter(filter, currentBranch);

            return filter;
        }

        public string GetSummary()
        {
            StringBuilder filter = new();

            GetCommitFilterSummary(filter);
            GetLimitingFilterSummary(filter);
            GetBranchFilterSummary(filter);

            return filter.ToString();
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Commit limiting filters, normally not affecting parents.
        /// </summary>
        /// <param name="filter">ArgumentBuilder arg</param>
        private void GetCommitRevisionFilter(ArgumentBuilder filter)
        {
            if (CommitsLimit > 0)
            {
                filter.Add($"--max-count={CommitsLimit}");
            }

            if (ByDateFrom)
            {
                filter.Add($"--since=\"{DateFrom:yyyy-MM-dd hh:mm:ss}\"");
            }

            if (ByDateTo)
            {
                filter.Add($"--until=\"{DateTo:yyyy-MM-dd hh:mm:ss}\"");
            }
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Commit limiting filters (affecting parents).
        /// </summary>
        /// <param name="filter">ArgumentBuilder arg</param>
        private void GetLimitingRevisionFilter(ArgumentBuilder filter)
        {
            if (!ShowMergeCommits)
            {
                filter.Add("--no-merges");
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

            if (IgnoreCase && (ByAuthor || ByCommitter || ByMessage || ByDiffContent))
            {
                filter.Add("--regexp-ignore-case");
            }

            if (ByDiffContent && !string.IsNullOrWhiteSpace(DiffContent))
            {
                filter.Add($"-G\"{DiffContent}\"");
            }

            if (ByMessage && !string.IsNullOrWhiteSpace(Message))
            {
                if (Message.StartsWithAny(_messageAsGitOptions))
                {
                    filter.Add(Message);
                }
                else
                {
                    filter.Add($"--grep=\"{Message}\"");
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
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Branch revision filters, not affecting parent rewriting.
        /// </summary>
        /// <param name="filter">ArgumentBuilder arg</param>
        private void GetBranchRevisionFilter(ArgumentBuilder filter, Lazy<string> currentBranch)
        {
            if (ShowOnlyFirstParent)
            {
                filter.Add("--first-parent");
            }

            if (ShowReflogReferences)
            {
                // All commits
                filter.Add("--reflog");
            }
            else if (IsShowCurrentBranchOnlyChecked && !string.IsNullOrWhiteSpace(currentBranch.Value))
            {
                // Git default, no option by default (stashes is special).

                AddFirstStashRef();

                // Add as filter (even if Git default is current branch) as the branch (ref) must exist
                // and the repo must contain commits, otherwise Git will exit with errors.
                filter.Add($"--branches={GetFilterRefName(currentBranch.Value)}");
            }
            else if (IsShowFilteredBranchesChecked && !string.IsNullOrWhiteSpace(BranchFilter))
            {
                // Show filtered branches only

                AddFirstStashRef();

                // If BranchFilter contains wildcards, "--branches=" must be prepended.
                // Use the simple branch filter if without wildcards (must be Git reference)
                // in order to avoid git adding implicit /* to the "--branches=" filter.
                bool useSimpleBranchFilter = BranchFilter.IndexOfAny(new[] { '?', '*', '[' }) == -1;
                filter.Add(useSimpleBranchFilter ? BranchFilter : $"--branches={BranchFilter}");
            }
            else
            {
                // refs (like notes) requires --reflog/--all or explicit inclusion (--glob)
                // (so included for --reflog/--all, not explicitly added for other)
                // --exclude is ignored for --reflog, notes/stash below only applicable with --all
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

            return;

            // git-log filters add implicit /* to the filters, unless there are wildcard characters in the ref name
            // get around this by adding a filter, that matches the branch name
            string GetFilterRefName(string gitRef)
            {
                return $"{gitRef[..^1]}[{gitRef[^1]}]";
            }

            // Add the most recent stash to the grid
            void AddFirstStashRef()
            {
                if (!AppSettings.ShowStashes)
                {
                    return;
                }

                // stash@{0} requires that the repo has commits and --glob=refs/stash is handled as refs/stash/*,
                // so this weird pattern must be used: "--glob=refs/stas[h]"
                filter.Add($"--glob={GetFilterRefName(GitRefName.RefsStashPrefix)}");
            }
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Commit limiting filters normally not affecting parents.
        /// </summary>
        /// <param name="filter">StringBuilder arg</param>
        private void GetCommitFilterSummary(StringBuilder filter)
        {
            if (ByDateFrom)
            {
                filter.AppendLine($"{TranslatedStrings.Since}: {DateFrom}");
            }

            if (ByDateTo)
            {
                filter.AppendLine($"{TranslatedStrings.Until}: {DateTo}");
            }
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Commit limiting filters (affecting parents).
        /// </summary>
        /// <param name="filter">StringBuilder arg</param>
        private void GetLimitingFilterSummary(StringBuilder filter)
        {
            // Ignore IgnoreCase, ShowMergeCommits, FullHistoryInFileHistory/SimplifyMergesInFileHistory (when history filtered)

            if (ByPathFilter)
            {
                filter.AppendLine($"{TranslatedStrings.PathFilter}: {PathFilter}");
            }

            if (ByAuthor && !string.IsNullOrWhiteSpace(Author))
            {
                filter.AppendLine($"{TranslatedStrings.Author}: {Author}");
            }

            if (ByCommitter && !string.IsNullOrWhiteSpace(Committer))
            {
                filter.AppendLine($"{TranslatedStrings.Committer}: {Committer}");
            }

            if (ShowSimplifyByDecoration)
            {
                filter.AppendLine($"{TranslatedStrings.SimplifyByDecoration}");
            }

            if (ByMessage && !string.IsNullOrEmpty(Message))
            {
                if (Message.StartsWithAny(_messageAsGitOptions))
                {
                    filter.AppendLine(Message);
                }
                else
                {
                    filter.AppendLine($"{TranslatedStrings.Message}: {Message}");
                }
            }

            if (ByDiffContent && !string.IsNullOrEmpty(DiffContent))
            {
                filter.AppendLine($"{TranslatedStrings.DiffContent}: {DiffContent}");
            }
        }

        /// <summary>
        /// Internal grouping of filter arguments.
        /// Branch revision filters, not affecting parent rewriting.
        /// </summary>
        /// <param name="filter">StringBuilder arg</param>
        private void GetBranchFilterSummary(StringBuilder filter)
        {
            if (ShowOnlyFirstParent)
            {
                filter.AppendLine(TranslatedStrings.ShowOnlyFirstParent);
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
