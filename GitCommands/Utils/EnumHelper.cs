using System.ComponentModel;
using System.Reflection;

namespace GitCommands.Utils
{
    public static class EnumHelper
    {
        public static string GetDescription<T>(this T value) where T : Enum
        {
            Type type = typeof(T);
            MemberInfo[] memInfo = type.GetMember(value.ToString());
            IEnumerable<DescriptionAttribute> attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>();

            return attributes.FirstOrDefault()?.Description ?? value.ToString();
        }

        public static T[] GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
