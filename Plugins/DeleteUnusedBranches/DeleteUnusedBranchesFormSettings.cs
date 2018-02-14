namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesFormSettings
    {
        public DeleteUnusedBranchesFormSettings(
            int daysOlderThan,
            string mergedInBranch,
            bool removeDeleteRemoteBranchesFromFlag,
            string remoteName,
            bool userRegexToFilterBranchesFlag,
            string regexFilter,
            bool regexCaseInsensitiveFlag,
            bool regexInvertedFlag,
            bool includeUnmergedBranchesFlag)
        {
            MergedInBranch = mergedInBranch;
            DaysOlderThan = daysOlderThan;
            DeleteRemoteBranchesFromFlag = removeDeleteRemoteBranchesFromFlag;
            RemoteName = remoteName;
            UseRegexToFilterBranchesFlag = userRegexToFilterBranchesFlag;
            RegexFilter = regexFilter;
            RegexCaseInsensitiveFlag = regexCaseInsensitiveFlag;
            RegexInvertedFlag = regexInvertedFlag;
            IncludeUnmergedBranchesFlag = includeUnmergedBranchesFlag;
        }

        public string MergedInBranch { get; }
        public int DaysOlderThan { get; }
        public bool DeleteRemoteBranchesFromFlag { get; }
        public string RemoteName { get; }
        public bool UseRegexToFilterBranchesFlag { get; }
        public string RegexFilter { get; }
        public bool RegexCaseInsensitiveFlag { get; }
        public bool RegexInvertedFlag { get; }
        public bool IncludeUnmergedBranchesFlag { get; }
    }
}
