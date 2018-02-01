namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesFormSettings
    {
        public DeleteUnusedBranchesFormSettings(
            int daysOlderThan,
            string mergedInBranch,
            bool removeDeleteRemoteBranchesFromFlag,
            string remoteName,
            bool userRegexToFIlterBranchesFlag,
            string regexFilter,
            bool regexCaseInsensitiveFlag,
            bool regexInvertedFlag,
            bool includeUnmergedBranchesFlag)
        {
            MergedInBranch = mergedInBranch;
            DaysOlderThan = daysOlderThan;
            DeleteRemoteBranchesFromFlag = removeDeleteRemoteBranchesFromFlag;
            RemoteName = remoteName;
            UseRegexToFilterBranchesFlag = userRegexToFIlterBranchesFlag;
            RegexFilter = regexFilter;
            RegexCaseInsensitiveFlag = regexCaseInsensitiveFlag;
            RegexInvertedFlag = regexInvertedFlag;
            IncludeUnmergedBranchesFlag = includeUnmergedBranchesFlag;
        }

        public string MergedInBranch { get; private set; }
        public int DaysOlderThan { get; private set; }
        public bool DeleteRemoteBranchesFromFlag { get; private set; }
        public string RemoteName { get; private set; }
        public bool UseRegexToFilterBranchesFlag { get; private set; }
        public string RegexFilter { get; private set; }
        public bool RegexCaseInsensitiveFlag { get; private set; }
        public bool RegexInvertedFlag { get; private set; }
        public bool IncludeUnmergedBranchesFlag { get; private set; }
    }
}
