namespace GitUIPluginInterfaces.Settings
{
    public interface IDetachedSettings
    {
        string Dictionary { get; set; }

        bool NoFastForwardMerge { get; set; }
    }
}
