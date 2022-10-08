namespace GitCommands.Git.Gpg
{
    public enum TagStatus
    {
        NoTag = 0,
        OneGood = 1,
        OneBad = 2,
        Many = 3,
        NoPubKey = 4,
        TagNotSigned = 5
    }
}
