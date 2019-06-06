using System;
using System.ComponentModel;
using System.Linq;

namespace GitCommands.Utils
{
    public static class EnumHelper
    {
        public static string GetDescription<T>(this T value) where T : Enum
        {
            var type = typeof(T);
            var memInfo = type.GetMember(value.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>();

            return attributes.FirstOrDefault()?.Description ?? value.ToString();
        }

        public static T[] GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
