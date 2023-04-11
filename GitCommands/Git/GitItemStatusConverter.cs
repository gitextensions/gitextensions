namespace GitCommands.Git
{
    public static class GitItemStatusConverter
    {
        // const instead of static readonly:
        // These are external constants not expected to be changed or be used as a standalone library
        public const char AddedStatus = 'A';
        public const char CopiedStatus = 'C';
        public const char DeletedStatus = 'D';
        public const char ModifiedStatus = 'M';
        public const char IgnoredStatus = '!';
        public const char RenamedStatus = 'R';
        public const char SkippedStatus = 'S';
        public const char UnmergedStatus = 'U';
        public const char UnmodifiedStatus_v1 = ' ';
        public const char UnmodifiedStatus_v2 = '.';
        public const char UntrackedStatus = '?';

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
