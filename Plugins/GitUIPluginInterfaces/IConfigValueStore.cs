using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public interface IConfigValueStore
    {
        string GetValue(string setting);
        void SetPathValue(string setting, [CanBeNull] string value);
        void SetValue(string setting, [CanBeNull] string value);
    }
}