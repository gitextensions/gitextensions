namespace GitCommands.Git
{
    public interface IGitRevisionProvider
    {
        GitRevision GetRevision(string commit, bool shortFormat = false);
    }
}