using System.Collections.Generic;
using System.Windows.Forms;

namespace GitExtUtils.GitUI
{
    public static class ControlTagExtensions
    {
        public static bool HasTag<TValue>(this Control control) =>
            control.HasTag<TValue>(typeof(TValue).FullName);

        public static bool HasTag<TValue>(this Control control, string key)
        {
            if (control.Tag == null)
            {
                return false;
            }

            return control.Tag is Dictionary<string, object> dict &&
                dict.TryGetValue(key, out var value) &&
                value is TValue;
        }

        public static TValue GetTag<TValue>(this Control control) =>
            GetTag<TValue>(control, typeof(TValue).FullName);

        public static TValue GetTag<TValue>(this Control control, string key)
        {
            if (control.Tag == null ||
                !(control.Tag is Dictionary<string, object> dict) ||
                !dict.TryGetValue(key, out var result))
            {
                return default;
            }

            return result is TValue typed
                ? typed
                : default;
        }

        public static void SetTag<TValue>(this Control control, TValue value) =>
            control.SetTag(typeof(TValue).FullName, value);

        public static void SetTag<TValue>(this Control control, string key, TValue value)
        {
            switch (control.Tag)
            {
                case Dictionary<string, object> dict:
                    dict[key] = value;
                    break;
                default:
                    control.Tag = new Dictionary<string, object> { { key, value } };
                    break;
            }
        }
    }
}
