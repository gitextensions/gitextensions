using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class SettingsPath : ISettingsSource
    {
        private const string PathSep = ".";
        public readonly ISettingsSource Parent;
        public readonly string PathName;

        public SettingsPath(ISettingsSource aParent, string aPathName)
        {
            Parent = aParent;
            PathName = aPathName;
        }

        public virtual string PathFor(string subPath)
        {
            return PathName + PathSep + subPath;
        }

        public T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            return Parent.GetValue(PathFor(name), defaultValue, decode);
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            Parent.SetValue(PathFor(name), value, encode);
        }
    }
}
