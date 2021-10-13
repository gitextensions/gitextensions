using GitUIPluginInterfaces;
using Microsoft;

namespace GitCommands.Settings
{
    public class SettingsPath : ISettingsSource
    {
        private readonly ISettingsSource? _parent;
        private readonly string _pathName;

        public SettingsPath(ISettingsSource? parent, string pathName)
        {
            _parent = parent;
            _pathName = pathName;
        }

        public string PathFor(string subPath)
        {
            return $"{_pathName}.{subPath}";
        }

        public virtual string? GetValue(string name)
        {
            Validates.NotNull(_parent);
            return _parent.GetValue(PathFor(name));
        }

        public virtual void SetValue(string name, string? value)
        {
            Validates.NotNull(_parent);
            _parent.SetValue(PathFor(name), value);
        }
    }
}
