using System;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    public abstract class ISettingsSource
    {
        public abstract T GetValue<T>([NotNull] string name, T defaultValue, [NotNull] Func<string, T> decode);

        public abstract void SetValue<T>([NotNull] string name, T value, [NotNull] Func<T, string> encode);

        public bool? GetBool([NotNull] string name)
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

        public bool GetBool([NotNull] string name, bool defaultValue)
        {
            return GetBool(name) ?? defaultValue;
        }

        public void SetBool([NotNull] string name, bool? value)
        {
            SetValue(name, value, b => b.HasValue ? (b.Value ? "true" : "false") : null);
        }

        public void SetInt([NotNull] string name, int? value)
        {
            SetValue(name, value, b => b.HasValue ? b.ToString() : null);
        }

        public int? GetInt([NotNull] string name)
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

        public void SetFloat([NotNull] string name, float? value)
        {
            SetValue(name, value, b => b.HasValue ? b.ToString() : null);
        }

        public float? GetFloat([NotNull] string name)
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

        public DateTime GetDate([NotNull] string name, DateTime defaultValue)
        {
            return GetDate(name) ?? defaultValue;
        }

        public void SetDate([NotNull] string name, DateTime? value)
        {
            SetValue(name, value, b => b?.ToString("yyyy/M/dd", CultureInfo.InvariantCulture));
        }

        public DateTime? GetDate([NotNull] string name)
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

        public int GetInt([NotNull] string name, int defaultValue)
        {
            return GetInt(name) ?? defaultValue;
        }

        public void SetFont([NotNull] string name, Font value)
        {
            SetValue(name, value, x => x.AsString());
        }

        public Font GetFont([NotNull] string name, Font defaultValue)
        {
            return GetValue(name, defaultValue, x => x.Parse(defaultValue));
        }

        public void SetColor([NotNull] string name, Color? value)
        {
            SetValue(name, value, x => x.HasValue ? ColorTranslator.ToHtml(x.Value) : null);
        }

        public Color? GetColor([NotNull] string name)
        {
            return GetValue<Color?>(name, null, x => ColorTranslator.FromHtml(x));
        }

        public Color GetColor([NotNull] string name, Color defaultValue)
        {
            return GetColor(name) ?? defaultValue;
        }

        public void SetEnum<T>([NotNull] string name, T value)
        {
            SetValue(name, value, x => x.ToString());
        }

        public T GetEnum<T>([NotNull] string name, T defaultValue) where T : struct, Enum
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

        public void SetNullableEnum<T>([NotNull] string name, T? value) where T : struct, Enum
        {
            SetValue(name, value, x => x.HasValue ? x.ToString() : string.Empty);
        }

        public T? GetNullableEnum<T>([NotNull] string name) where T : struct
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

        public void SetString([NotNull] string name, [CanBeNull] string value)
        {
            SetValue(name, value, s => string.IsNullOrEmpty(s) ? null : s);
        }

        public string GetString([NotNull] string name, string defaultValue)
        {
            return GetValue(name, defaultValue, x => x);
        }
    }
}