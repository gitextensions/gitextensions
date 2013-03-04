namespace GitUI.BuildServerIntegration
{
    public interface IBuildServerWatcher
    {
        bool GetBuildServerCredentials(IBuildServerAdapter buildServerAdapter, bool firstTime, out bool useGuestAccess, out string userName, out string password);

        void LaunchBuildServerInfoFetchOperation();

        void CancelBuildStatusFetchOperation();
    }
}