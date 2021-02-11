namespace GitCommands.Git
{
    public static class GitItemStatusConverter
    {
        public static GitItemStatus FromStatusCharacter(StagedStatus staged, string fileName, char x)
        {
            var isNew = x == 'A' || x == '?' || x == '!';

            return new GitItemStatus(fileName)
            {
                IsNew = isNew,
                IsChanged = x == 'M',
                IsDeleted = x == 'D',
                IsSkipWorktree = x == 'S',
                IsRenamed = x == 'R',
                IsCopied = x == 'C',
                IsTracked = (x != '?' && x != '!' && x != ' ') || !isNew,
                IsIgnored = x == '!',
                IsConflict = x == 'U',
                Staged = staged
            };
        }
    }
}
