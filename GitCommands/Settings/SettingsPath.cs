using System;
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

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            Validates.NotNull(_parent);
            return _parent.GetValue(PathFor(name), defaultValue, decode);
        }

        public override void SetValue<T>(string name, T value, Func<T, string?> encode)
        {
            Validates.NotNull(_parent);
            _parent.SetValue(PathFor(name), value, encode);
        }
    }
}
