namespace GitExtensions.Core.Settings
{
    public interface ISettingsValueGetter
    {
        string GetValue(string setting);
    }
}