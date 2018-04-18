using System.Threading.Tasks;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerWatcher
    {
        IBuildServerCredentials GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting);

        Task LaunchBuildServerInfoFetchOperationAsync();

        void CancelBuildStatusFetchOperation();

        string ReplaceVariables(string projectNames);
    }
}