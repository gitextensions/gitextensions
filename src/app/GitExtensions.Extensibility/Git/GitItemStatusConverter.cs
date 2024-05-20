namespace GitExtensions.Extensibility.Git;

public static class GitItemStatusConverter
{
    // https://git-scm.com/docs/git-status#_short_format
    // const instead of static readonly:
    // These are external constants not expected to be changed or be used as a standalone library
    public const char AddedStatus = 'A';
    public const char CopiedStatus = 'C';
    public const char DeletedStatus = 'D';
    public const char ModifiedStatus = 'M';
    public const char RenamedStatus = 'R';
    public const char TypeChangedStatus = 'T';
    public const char UnmergedStatus = 'U';
    public const char UnmodifiedStatus_v1 = ' ';
    public const char UnmodifiedStatus_v2 = '.';
    public const char IgnoredStatus = '!';
    public const char UntrackedStatus = '?';

    // Unused char, to be used to get a default object
    public const char UnusedCharacter = '&';

    public static GitItemStatus FromStatusCharacter(StagedStatus staged, string fileName, char x)
    {
        bool isNew = x is AddedStatus or UntrackedStatus or IgnoredStatus;

        return new GitItemStatus(fileName)
        {
            IsNew = isNew,
            IsChanged = x is ModifiedStatus or TypeChangedStatus,
            IsDeleted = x is DeletedStatus,
            IsRenamed = x is RenamedStatus,
            IsCopied = x is CopiedStatus,
            IsTracked = !(x is UntrackedStatus or IgnoredStatus or UnmodifiedStatus_v1) || !isNew,
            IsIgnored = x is IgnoredStatus,
            IsUnmerged = x is UnmergedStatus,
            Staged = staged
        };
    }
}
