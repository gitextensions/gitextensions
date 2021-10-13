using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public interface ISettingsSource
    {
        SettingLevel SettingLevel { get => SettingLevel.Unknown; }

        string? GetValue(string name);

        void SetValue(string name, string? value);
    }

    public static class ISettingsSourceExtensions
    {
        public static bool? GetBool(this ISettingsSource source, string name)
        {
            string? stringValue = source.GetValue(name);

            if (string.Equals(stringValue, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return null;
        }

        public static bool GetBool(this ISettingsSource source, string name, bool defaultValue)
        {
            return source.GetBool(name) ?? defaultValue;
        }

        public static void SetBool(this ISettingsSource source, string name, bool? value)
        {
            string? stringValue = value.HasValue ? (value.Value ? "true" : "false") : null;

            source.SetValue(name, stringValue);
        }

        public static int? GetInt(this ISettingsSource source, string name)
        {
            string? stringValue = source.GetValue(name);

            if (int.TryParse(stringValue, out var result))
            {
                return result;
            }

            return null;
        }

        public static int GetInt(this ISettingsSource source, string name, int defaultValue)
        {
            return source.GetInt(name) ?? defaultValue;
        }

        public static void SetInt(this ISettingsSource source, string name, int? value)
        {
            string? stringValue = value.HasValue ? value.ToString() : null;

            source.SetValue(name, stringValue);
        }

        public static float? GetFloat(this ISettingsSource source, string name)
        {
            string? stringValue = source.GetValue(name);

            if (float.TryParse(stringValue, out var result))
            {
                return result;
            }

            return null;
        }

        public static void SetFloat(this ISettingsSource source, string name, float? value)
        {
            string? stringValue = value.HasValue ? value.ToString() : null;

            source.SetValue(name, stringValue);
        }

        public static DateTime? GetDate(this ISettingsSource source, string name)
        {
            string? stringValue = source.GetValue(name);

            if (DateTime.TryParseExact(stringValue, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            return null;
        }

        public static DateTime GetDate(this ISettingsSource source, string name, DateTime defaultValue)
        {
            return source.GetDate(name) ?? defaultValue;
        }

        public static void SetDate(this ISettingsSource source, string name, DateTime? value)
        {
            string? stringValue = value?.ToString("yyyy/M/dd", CultureInfo.InvariantCulture);

            source.SetValue(name, stringValue);
        }

        public static Font GetFont(this ISettingsSource source, string name, Font defaultValue)
        {
            string? stringValue = source.GetValue(name);

            return stringValue.Parse(defaultValue);
        }

        public static void SetFont(this ISettingsSource source, string name, Font value)
        {
            string? stringValue = value.AsString();

            source.SetValue(name, stringValue);
        }

        public static Color GetColor(this ISettingsSource source, string name, Color defaultValue)
        {
            string? stringValue = source.GetValue(name);

            try
            {
                return ColorTranslator.FromHtml(stringValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T GetEnum<T>(this ISettingsSource source, string name, T defaultValue) where T : struct, Enum
        {
            string? stringValue = source.GetValue(name);

            if (Enum.TryParse(stringValue, true, out T result))
            {
                return result;
            }

            return defaultValue;
        }

        public static void SetEnum<T>(this ISettingsSource source, string name, T value) where T : Enum
        {
            string? stringValue = value.ToString();

            source.SetValue(name, stringValue);
        }

        public static T? GetNullableEnum<T>(this ISettingsSource source, string name) where T : struct
        {
            string? stringValue = source.GetValue(name);

            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            if (Enum.TryParse(stringValue, true, out T result))
            {
                return result;
            }

            return null;
        }

        public static void SetNullableEnum<T>(this ISettingsSource source, string name, T? value) where T : struct, Enum
        {
            string? stringValue = value.HasValue ? value.ToString() : string.Empty;

            source.SetValue(name, stringValue);
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static string? GetString(this ISettingsSource source, string name, string? defaultValue)
        {
            string? stringValue = source.GetValue(name);

            return stringValue ?? defaultValue;
        }

        public static void SetString(this ISettingsSource source, string name, string? value)
        {
            string? stringValue = value;

            source.SetValue(name, stringValue);
        }
    }
}
