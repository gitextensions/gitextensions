namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IPasswordHelper
    {
        string TryGetHelperPassword(string inputUrl);
    }
}
