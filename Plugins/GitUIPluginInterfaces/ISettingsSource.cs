using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface ISettingsSource
    {
        T GetValue<T>(string name, T defaultValue, Func<string, T> decode);

        void SetValue<T>(string name, T value, Func<T, string> encode);
    }
}
