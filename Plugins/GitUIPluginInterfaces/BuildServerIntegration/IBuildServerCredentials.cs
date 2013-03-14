namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerCredentials
    {
        bool UseGuestAccess { get; set; }

        string Username { get; set; }

        string Password { get; set; }
    }
}