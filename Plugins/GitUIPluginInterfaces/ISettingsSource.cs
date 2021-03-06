using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public abstract class ISettingsSource
    {
        public virtual SettingLevel SettingLevel { get; set; } = SettingLevel.Unknown;

        public abstract T GetValue<T>(string name, T defaultValue, Func<string, T> decode);

        public abstract void SetValue<T>(string name, T value, Func<T, string?> encode);

        public bool? GetBool(string name)
        {
            return GetValue<bool?>(name, null, x =>
            {
                if (string.Equals(x, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(x, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return null;
            });
        }

        public bool GetBool(string name, bool defaultValue)
        {
            return GetBool(name) ?? defaultValue;
        }

        public void SetBool(string name, bool? value)
        {
            SetValue(name, value, b => b.HasValue ? (b.Value ? "true" : "false") : null);
        }

        public void SetInt(string name, int? value)
        {
            SetValue(name, value, b => b.HasValue ? b.ToString() : null);
        }

        public int? GetInt(string name)
        {
            return GetValue<int?>(name, null, x =>
            {
                if (int.TryParse(x, out var result))
                {
                    return result;
                }

                return null;
            });
        }

        public void SetFloat(string name, float? value)
        {
            SetValue(name, value, b => b.HasValue ? b.ToString() : null);
        }

        public float? GetFloat(string name)
        {
            return GetValue<float?>(name, null, x =>
            {
                if (float.TryParse(x, out var result))
                {
                    return result;
                }

                return null;
            });
        }

        public DateTime GetDate(string name, DateTime defaultValue)
        {
            return GetDate(name) ?? defaultValue;
        }

        public void SetDate(string name, DateTime? value)
        {
            SetValue(name, value, b => b?.ToString("yyyy/M/dd", CultureInfo.InvariantCulture));
        }

        public DateTime? GetDate(string name)
        {
            return GetValue<DateTime?>(name, null, x =>
            {
                if (DateTime.TryParseExact(x, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                return null;
            });
        }

        public int GetInt(string name, int defaultValue)
        {
            return GetInt(name) ?? defaultValue;
        }

        public void SetFont(string name, Font value)
        {
            SetValue(name, value, x => x.AsString());
        }

        public Font GetFont(string name, Font defaultValue)
        {
            return GetValue(name, defaultValue, x => x.Parse(defaultValue));
        }

        public Color GetColor(string name, Color defaultValue)
        {
            return GetValue<Color?>(name, null, x => ColorTranslator.FromHtml(x)) ?? defaultValue;
        }

        public void SetEnum<T>(string name, T value) where T : Enum
        {
            SetValue(name, value, x => x.ToString());
        }

        public T GetEnum<T>(string name, T defaultValue) where T : struct, Enum
        {
            return GetValue(name, defaultValue, x =>
            {
                var val = x.ToString();

                if (Enum.TryParse(val, true, out T result))
                {
                    return result;
                }

                return defaultValue;
            });
        }

        public void SetNullableEnum<T>(string name, T? value) where T : struct, Enum
        {
            SetValue(name, value, x => x.HasValue ? x.ToString() : string.Empty);
        }

        public T? GetNullableEnum<T>(string name) where T : struct
        {
            return GetValue<T?>(name, null, x =>
            {
                var val = x.ToString();

                if (string.IsNullOrEmpty(val))
                {
                    return null;
                }

                if (Enum.TryParse(val, true, out T result))
                {
                    return result;
                }

                return null;
            });
        }

        public void SetString(string name, string? value)
        {
            SetValue(name, value, s => string.IsNullOrEmpty(s) ? null : s);
        }

        [return: NotNullIfNotNull("defaultValue")]
        public string? GetString(string name, string? defaultValue)
        {
            return GetValue(name, defaultValue, x => x);
        }
    }
}
