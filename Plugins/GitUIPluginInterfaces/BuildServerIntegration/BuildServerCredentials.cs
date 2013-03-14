namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public class BuildServerCredentials : IBuildServerCredentials
    {
        public bool UseGuestAccess { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}