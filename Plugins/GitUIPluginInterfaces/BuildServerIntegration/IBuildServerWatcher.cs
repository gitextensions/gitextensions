using System.Threading.Tasks;

namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerWatcher
    {
        IBuildServerCredentials? GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting);

        Task LaunchBuildServerInfoFetchOperationAsync();

        void CancelBuildStatusFetchOperation();

        /// <summary>
        /// Replace variables for the project string with the current "repo shortname"
        /// </summary>
        /// <param name="projectNames">build server specific format, compatible with the variable format</param>
        /// <returns>projectNames with variables replaced</returns>
        string ReplaceVariables(string projectNames);
    }
}
