namespace GitCommands.Git
{
    public static class GitItemStatusConverter
    {
        public static readonly char AddedStatus = 'A';
        public static readonly char CopiedStatus = 'C';
        public static readonly char DeletedStatus = 'D';
        public static readonly char ModifiedStatus = 'M';
        public static readonly char IgnoredStatus = '!';
        public static readonly char RenamedStatus = 'R';
        public static readonly char SkippedStatus = 'S';
        public static readonly char UnmergedStatus = 'U';
        public static readonly char UnmodifiedStatus_v1 = ' ';
        public static readonly char UnmodifiedStatus_v2 = '.';
        public static readonly char UntrackedStatus = '?';

        public static GitItemStatus FromStatusCharacter(StagedStatus staged, string fileName, char x)
        {
            var isNew = x == AddedStatus || x == UntrackedStatus || x == IgnoredStatus;

            return new GitItemStatus(fileName)
            {
                IsNew = isNew,
                IsChanged = x == ModifiedStatus,
                IsDeleted = x == DeletedStatus,
                IsSkipWorktree = x == SkippedStatus,
                IsRenamed = x == RenamedStatus,
                IsCopied = x == CopiedStatus,
                IsTracked = (x != UntrackedStatus && x != IgnoredStatus && x != UnmodifiedStatus_v1) || !isNew,
                IsIgnored = x == IgnoredStatus,
                IsConflict = x == UnmergedStatus,
                Staged = staged
            };
        }
    }
}
