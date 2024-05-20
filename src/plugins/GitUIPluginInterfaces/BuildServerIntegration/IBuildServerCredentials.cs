namespace GitUIPluginInterfaces.BuildServerIntegration
{
    public interface IBuildServerCredentials
    {
        BuildServerCredentialsType BuildServerCredentialsType { get; set; }

        string? Username { get; set; }

        string? Password { get; set; }

        string? BearerToken { get; set; }
    }
}
