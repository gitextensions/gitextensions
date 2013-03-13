namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerWatcher
    {
        IBuildServerCredentials GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool useStoredCredentialsIfExisting);

        void LaunchBuildServerInfoFetchOperation();

        void CancelBuildStatusFetchOperation();
    }
}