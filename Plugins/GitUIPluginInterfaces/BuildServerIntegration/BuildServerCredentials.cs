namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public class BuildServerCredentials : IBuildServerCredentials
    {
        public BuildServerCredentialsType BuildServerCredentialsType { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? BearerToken { get; set; }
    }
}
