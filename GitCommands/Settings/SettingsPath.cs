using System;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Settings
{
    public class SettingsPath : ISettingsSource
    {
        [CanBeNull] private readonly ISettingsSource _parent;
        [NotNull] private readonly string _pathName;

        public SettingsPath([CanBeNull] ISettingsSource parent, [NotNull] string pathName)
        {
            _parent = parent;
            _pathName = pathName;
        }

        [NotNull]
        public string PathFor([NotNull] string subPath)
        {
            return $"{_pathName}.{subPath}";
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            return _parent.GetValue(PathFor(name), defaultValue, decode);
        }

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            _parent.SetValue(PathFor(name), value, encode);
        }
    }
}
