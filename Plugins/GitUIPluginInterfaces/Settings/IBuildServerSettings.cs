namespace GitUIPluginInterfaces.Settings
{
    public interface IBuildServerSettings
    {
        string? Type { get; set; }

        bool EnableIntegration { get; set; }

        bool ShowBuildResultPage { get; set; }
    }
}
