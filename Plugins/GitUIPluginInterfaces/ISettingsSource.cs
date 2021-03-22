using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;

namespace GitUIPluginInterfaces
{
    public abstract class ISettingsSource : Extensibility.ISettingsSource
    {
        private static readonly Type BoolType = typeof(bool);
        private static readonly Type IntType = typeof(int);
        private static readonly Type FloatType = typeof(float);
        private static readonly Type DateTimeType = typeof(DateTime);
        private static readonly Type ColorType = typeof(Color);
        private static readonly Type StringType = typeof(string);
        private static readonly Type FontType = typeof(Font);

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

        public string GetValue(string key)
        {
            return GetValue(key, string.Empty);
        }

        public string GetValue(string key, string defaultValue)
        {
            return GetValue(key, defaultValue, x => x);
        }

        public T GetValue<T>(string key)
            where T : new()
        {
            return GetValue(key, new T());
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            var type = typeof(T);
            var baseType = Nullable.GetUnderlyingType(type) ?? type;

            if (baseType == BoolType)
            {
                return (T)((object?)GetBool(key) ?? defaultValue!);
            }

            if (baseType == IntType)
            {
                return (T)((object?)GetInt(key) ?? defaultValue!);
            }

            if (baseType == FloatType)
            {
                return (T)((object?)GetFloat(key) ?? defaultValue!);
            }

            if (baseType == DateTimeType)
            {
                return (T)((object?)GetDate(key) ?? defaultValue!);
            }

            if (baseType == ColorType)
            {
                return (T)(object)GetColor(key, (Color)(object)defaultValue!);
            }

            if (baseType.IsEnum)
            {
                return GetValue(key, defaultValue, x =>
                {
                    var val = x.ToString();

                    try
                    {
                        return (T)Enum.Parse(baseType, val, true);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                });
            }

            if (baseType == StringType)
            {
                return (T)(object)GetString(key, (string)(object)defaultValue!);
            }

            if (baseType == FontType)
            {
                return (T)(object)GetFont(key, (Font)(object)defaultValue!);
            }

            throw new InvalidOperationException($"Unknown type: {type}.");
        }

        public void SetValue<T>(string key, T value)
        {
            switch (value)
            {
                case null:
                    SetValue(key, value, x => null);
                    return;
                case bool boolValue:
                    SetBool(key, boolValue);
                    return;
                case int intValue:
                    SetInt(key, intValue);
                    return;
                case float floatValue:
                    SetFloat(key, floatValue);
                    return;
                case DateTime dateTimeValue:
                    SetDate(key, dateTimeValue);
                    return;
                case Enum enumValue:
                    SetEnum(key, enumValue);
                    return;
                case string stringValue:
                    SetString(key, stringValue);
                    return;
                case Font fontValue:
                    SetFont(key, fontValue);
                    return;
                default:
                    throw new InvalidOperationException($"Unknown type: {typeof(T)}.");
            }
        }
    }
}
