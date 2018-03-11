using System;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class SettingsPath : ISettingsSource
    {
        private const string PathSep = ".";
        public readonly ISettingsSource Parent;
        public readonly string PathName;

        public SettingsPath(ISettingsSource parent, string pathName)
        {
            Parent = parent;
            PathName = pathName;
        }

        public virtual string PathFor(string subPath)
        {
            return PathName + PathSep + subPath;
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            return Parent.GetValue(PathFor(name), defaultValue, decode);
        }

        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            Parent.SetValue(PathFor(name), value, encode);
        }
    }
}
