using System;
using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public abstract class ISettingsSource
    {
        public abstract T GetValue<T>(string name, T defaultValue, Func<string, T> decode);

        public abstract void SetValue<T>(string name, T value, Func<T, string> encode);

        public bool? GetBool(string name)
        {
            return GetValue<bool?>(name, null, x =>
            {
                var val = x.ToString().ToLower();
                if (val == "true")
                {
                    return true;
                }

                if (val == "false")
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

        public void SetColor(string name, Color? value)
        {
            SetValue(name, value, x => x.HasValue ? ColorTranslator.ToHtml(x.Value) : null);
        }

        public Color? GetColor(string name)
        {
            return GetValue<Color?>(name, null, x => ColorTranslator.FromHtml(x));
        }

        public Color GetColor(string name, Color defaultValue)
        {
            return GetColor(name) ?? defaultValue;
        }

        public void SetEnum<T>(string name, T value)
        {
            SetValue(name, value, x => x.ToString());
        }

        public T GetEnum<T>(string name, T defaultValue) where T : struct
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

        public void SetNullableEnum<T>(string name, T? value) where T : struct
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

        public void SetString(string name, string value)
        {
            SetValue(name, value, s => string.IsNullOrEmpty(s) ? null : s);
        }

        public string GetString(string name, string defaultValue)
        {
            return GetValue(name, defaultValue, x => x);
        }
    }

    public static class FontParser
    {
        private const string InvariantCultureId = "_IC_";

        public static string AsString(this Font value)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0};{1};{2}", value.FontFamily.Name, value.Size, InvariantCultureId);
        }

        public static Font Parse(this string value, Font defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            string[] parts = value.Split(';');

            if (parts.Length < 2)
            {
                return defaultValue;
            }

            try
            {
                string fontSize;
                if (parts.Length == 3 && parts[2] == InvariantCultureId)
                {
                    fontSize = parts[1];
                }
                else
                {
                    fontSize = parts[1].Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    fontSize = fontSize.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                }

                return new Font(parts[0], float.Parse(fontSize, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}