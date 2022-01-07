using System;
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

        /// <summary>
        /// Gets or sets the current git reference filter options.
        /// </summary>
        public RefFilterOptions RefFilterOptions
        {
            get
            {
                RefFilterOptions refFilterOptions;
                if (!ByBranchFilter)
                {
                    refFilterOptions = RefFilterOptions.All | RefFilterOptions.Boundary;
                }
                else if (ShowCurrentBranchOnly)
                {
                    refFilterOptions = RefFilterOptions.None;
                }
                else
                {
                    refFilterOptions = BranchFilter.Length > 0
                        ? RefFilterOptions.Branches
                        : RefFilterOptions.All | RefFilterOptions.Boundary;
                }

                const RefFilterOptions gitNotesOptions = RefFilterOptions.All | RefFilterOptions.Boundary;
                if (!AppSettings.ShowGitNotes && (refFilterOptions & gitNotesOptions) == gitNotesOptions)
                {
                    refFilterOptions |= RefFilterOptions.ShowGitNotes;
                }

                if (AppSettings.ShowGitNotes)
                {
                    refFilterOptions &= ~RefFilterOptions.ShowGitNotes;
                }

                if (!AppSettings.ShowMergeCommits)
                {
                    refFilterOptions |= RefFilterOptions.NoMerges;
                }

                if (!AppSettings.ShowLatestStash)
                {
                    refFilterOptions &= ~RefFilterOptions.Stashes;
                }

                if (ShowFirstParent)
                {
                    refFilterOptions |= RefFilterOptions.FirstParent;
                }

                if (ShowSimplifyByDecoration)
                {
                    refFilterOptions |= RefFilterOptions.SimplifyByDecoration;
                }

                if (ShowReflogReferences)
                {
                    refFilterOptions |= RefFilterOptions.Reflogs;
                }

                return refFilterOptions;
            }
        }

        public bool IsShowAllBranchesChecked => !ByBranchFilter;

        public bool IsShowCurrentBranchOnlyChecked => ByBranchFilter && ShowCurrentBranchOnly;

        public bool IsShowFilteredBranchesChecked => ByBranchFilter && !ShowCurrentBranchOnly;

        public bool ShowCurrentBranchOnly
        {
            get => AppSettings.ShowCurrentBranchOnly;
            set => AppSettings.ShowCurrentBranchOnly = value;
        }

        public bool ShowFirstParent
        {
            get => AppSettings.ShowFirstParent;
            set => AppSettings.ShowFirstParent = value;
        }

        public bool ShowReflogReferences
        {
            get => AppSettings.ShowReflogReferences;
            set
            {
                AppSettings.ShowReflogReferences = value;
                if (value)
                {
                    // If reflogs are shown, then we can't apply any filters
                    ByBranchFilter = false;
                    ShowCurrentBranchOnly = false;
                    ShowSimplifyByDecoration = false;
                }
            }
        }

        public bool ShowSimplifyByDecoration
        {
            get => AppSettings.ShowSimplifyByDecoration;
            set => AppSettings.ShowSimplifyByDecoration = value;
        }

        public bool HasFilter
        {
            get => ByDateFrom ||
                   ByDateTo ||
                   ByAuthor ||
                   ByCommitter ||
                   ByMessage ||
                   ByPathFilter ||
                   ByBranchFilter ||
                   ShowSimplifyByDecoration;
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

            if (filter.FilterByAuthor)
            {
                if (ByAuthor && string.Equals(Author, filter.Text, StringComparison.CurrentCulture))
                {
                    return false;
                }

                ByAuthor = !string.IsNullOrWhiteSpace(filter.Text);
                Author = filter.Text;
                return true;
            }

            if (filter.FilterByCommitter)
            {
                if (ByCommitter && string.Equals(Committer, filter.Text, StringComparison.CurrentCulture))
                {
                    return false;
                }

                ByCommitter = !string.IsNullOrWhiteSpace(filter.Text);
                Committer = filter.Text;
                return true;
            }

            if (filter.FilterByCommit)
            {
                if (ByMessage && string.Equals(Message, filter.Text, StringComparison.CurrentCulture))
                {
                    return false;
                }

                ByMessage = !string.IsNullOrWhiteSpace(filter.Text);
                Message = filter.Text;
                return true;
            }

            if (filter.FilterByDiffContent)
            {
                if (ByDiffContent && string.Equals(DiffContent, filter.Text, StringComparison.CurrentCulture))
                {
                    return false;
                }

                ByDiffContent = !string.IsNullOrWhiteSpace(filter.Text);
                DiffContent = filter.Text;
                return true;
            }

            return false;
        }

        public void ResetAllFilters()
        {
            ByDateFrom = false;
            ByDateTo = false;
            ByAuthor = false;
            ByCommitter = false;
            ByMessage = false;
            ByPathFilter = false;
            ByBranchFilter = false;
            ShowSimplifyByDecoration = false;
        }

        public ArgumentString GetRevisionFilter()
        {
            if (IsRaw)
            {
                Debug.Fail("Not supported");
            }

            ArgumentBuilder filter = new();

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

            if (!filter.IsEmpty && IgnoreCase)
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
        }

        public string GetSummary()
        {
            StringBuilder filter = new();

            if (ByPathFilter)
            {
                filter.AppendLine($"{TranslatedStrings.PathFilter}: {PathFilter}");
            }

            if (ByBranchFilter)
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
                filter.AppendLine($"{TranslatedStrings.Message}: {DiffContent}");
            }

            if (ByDateFrom)
            {
                filter.AppendLine($"{TranslatedStrings.Since}: {DateFrom}");
            }

            if (ByDateTo)
            {
                filter.AppendLine($"{TranslatedStrings.Until}: {DateTo}");
            }

            if (ShowSimplifyByDecoration)
            {
                filter.AppendLine($"{TranslatedStrings.SimplifyByDecoration}");
            }

            // Ignore IgnoreCase, CurrentBranchOnlyCheck

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
