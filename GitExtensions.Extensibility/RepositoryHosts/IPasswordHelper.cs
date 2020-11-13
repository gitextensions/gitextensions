namespace GitExtensions.Extensibility.RepositoryHosts
{
    public interface IPasswordHelper
    {
        string TryGetHelperPassword(string inputUrl);
    }
}
