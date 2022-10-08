namespace GitCommands.Git.Gpg
{
    public enum CommitStatus
    {
        NoSignature = 0,
        GoodSignature = 1,
        SignatureError = 2,
        MissingPublicKey = 3,
    }
}
