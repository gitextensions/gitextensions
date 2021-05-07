namespace GitUIPluginInterfaces
{
    public interface IConfigValueStore
    {
        string GetValue(string setting);
        void SetPathValue(string setting, string? value);
        void SetValue(string setting, string? value);
    }
}
