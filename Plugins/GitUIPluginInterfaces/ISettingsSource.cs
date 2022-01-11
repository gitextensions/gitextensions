using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public abstract class ISettingsSource
    {
        public virtual SettingLevel SettingLevel { get; set; } = SettingLevel.Unknown;

        public abstract string? GetValue(string name);

        public abstract void SetValue(string name, string? value);

        public bool? GetBool(string name)
        {
            string? stringValue = GetValue(name);

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

        public bool GetBool(string name, bool defaultValue)
        {
            return GetBool(name) ?? defaultValue;
        }

        public void SetBool(string name, bool? value)
        {
            string? stringValue = value.HasValue ? (value.Value ? "true" : "false") : null;

            SetValue(name, stringValue);
        }

        public void SetInt(string name, int? value)
        {
            string? stringValue = value.HasValue ? value.ToString() : null;

            SetValue(name, stringValue);
        }

        public int? GetInt(string name)
        {
            string? stringValue = GetValue(name);

            if (int.TryParse(stringValue, out var result))
            {
                return result;
            }

            return null;
        }

        public void SetFloat(string name, float? value)
        {
            string? stringValue = value.HasValue ? value.ToString() : null;

            SetValue(name, stringValue);
        }

        public float? GetFloat(string name)
        {
            string? stringValue = GetValue(name);

            if (float.TryParse(stringValue, out var result))
            {
                return result;
            }

            return null;
        }

        public DateTime GetDate(string name, DateTime defaultValue)
        {
            return GetDate(name) ?? defaultValue;
        }

        public void SetDate(string name, DateTime? value)
        {
            string? stringValue = value?.ToString("yyyy/M/dd", CultureInfo.InvariantCulture);

            SetValue(name, stringValue);
        }

        public DateTime? GetDate(string name)
        {
            string? stringValue = GetValue(name);

            if (DateTime.TryParseExact(stringValue, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }

        public int GetInt(string name, int defaultValue)
        {
            return GetInt(name) ?? defaultValue;
        }

        public void SetFont(string name, Font value)
        {
            string? stringValue = value.AsString();

            SetValue(name, stringValue);
        }

        public Font GetFont(string name, Font defaultValue)
        {
            string? stringValue = GetValue(name);

            return stringValue.Parse(defaultValue);
        }

        public Color GetColor(string name, Color defaultValue)
        {
            string? stringValue = GetValue(name);

            try
            {
                return ColorTranslator.FromHtml(stringValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        public void SetEnum<T>(string name, T value) where T : Enum
        {
            string? stringValue = value.ToString();

            SetValue(name, stringValue);
        }

        public T GetEnum<T>(string name, T defaultValue) where T : struct, Enum
        {
            string? stringValue = GetValue(name);

            if (Enum.TryParse(stringValue, true, out T result))
            {
                return result;
            }

            return defaultValue;
        }

        public void SetNullableEnum<T>(string name, T? value) where T : struct, Enum
        {
            string? stringValue = value.HasValue ? value.ToString() : string.Empty;

            SetValue(name, stringValue);
        }

        public T? GetNullableEnum<T>(string name) where T : struct
        {
            string? stringValue = GetValue(name);

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

        public void SetString(string name, string? value)
            => SetValue(name, value);

        [return: NotNullIfNotNull("defaultValue")]
        public string? GetString(string name, string? defaultValue)
        {
            string? stringValue = GetValue(name);

            return stringValue ?? defaultValue;
        }
    }
}
