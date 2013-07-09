using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GitCommands.Settings
{

    public class StringSetting : Setting<string>
    {
        public StringSetting(string aName, ISettingsSource settingsSource, string aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        {        
        }

        public override string Value
        {
            get 
            {
                return SettingsSource.GetString(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetString(Name, value);
            }
        }
    }

    public class BoolNullableSetting : Setting<bool?>
    {
        public BoolNullableSetting(string aName, ISettingsSource settingsSource, bool aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override bool? Value
        {
            get
            {
                return SettingsSource.GetBool(Name);
            }

            set
            {
                SettingsSource.SetBool(Name, value);
            }
        }

        public bool ValueOrDefault
        {
            get
            {
                if (Value.HasValue)
                    return Value.Value;

                return DefaultValue.Value;                
            }
        }

    }

    public class BoolSetting : Setting<bool>
    {
        public BoolSetting(string aName, ISettingsSource settingsSource, bool aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override bool Value
        {
            get
            {
                return SettingsSource.GetBool(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetBool(Name, value);
            }
        }
    }

    public class EnumSetting<T> : Setting<T> where T : struct
    {
        public EnumSetting(string aName, ISettingsSource settingsSource, T aDefaultValue)
            : base(aName, settingsSource, aDefaultValue)
        { }

        public override T Value
        {
            get
            {
                return SettingsSource.GetEnum(Name, DefaultValue);
            }

            set
            {
                SettingsSource.SetEnum(Name, value);
            }
        }
    }

    public class EnumNullableSetting<T> : Setting<T?> where T : struct
    {
        public EnumNullableSetting(string aName, ISettingsSource settingsSource)
            : base(aName, settingsSource, null)
        { }

        public override T? Value
        {
            get
            {
                return SettingsSource.GetNullableEnum<T>(Name);
            }

            set
            {
                SettingsSource.SetNullableEnum(Name, value);
            }
        }
    }

    public static class ISettingsSourceBasicExt
    {

        public static bool? GetBool(this ISettingsSource iss, string name)
        {
            return iss.GetValue<bool?>(name, null, x =>
            {
                var val = x.ToString().ToLower();
                if (val == "true") return true;
                if (val == "false") return false;
                return null;
            });
        }

        public static bool GetBool(this ISettingsSource iss, string name, bool defaultValue)
        {
            return iss.GetBool(name) ?? defaultValue;
        }

        public static void SetBool(this ISettingsSource iss, string name, bool? value)
        {
            iss.SetValue<bool?>(name, value, (bool? b) => b.HasValue ? (b.Value ? "true" : "false") : null);
        }

        public static void SetInt(this ISettingsSource iss, string name, int? value)
        {
            iss.SetValue<int?>(name, value, (int? b) => b.HasValue ? b.ToString() : null);
        }

        public static int? GetInt(this ISettingsSource iss, string name)
        {
            return iss.GetValue<int?>(name, null, x =>
            {
                int result;
                if (int.TryParse(x, out result))
                {
                    return result;
                }

                return null;
            });
        }

        public static DateTime GetDate(this ISettingsSource iss, string name, DateTime defaultValue)
        {
            return iss.GetDate(name) ?? defaultValue;
        }

        public static void SetDate(this ISettingsSource iss, string name, DateTime? value)
        {
            iss.SetValue<DateTime?>(name, value, (DateTime? b) => b.HasValue ? b.Value.ToString("yyyy/M/dd", CultureInfo.InvariantCulture) : null);
        }

        public static DateTime? GetDate(this ISettingsSource iss, string name)
        {
            return iss.GetValue<DateTime?>(name, null, x =>
            {
                DateTime result;
                if (DateTime.TryParseExact(x, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return result;

                return null;
            });
        }

        public static int GetInt(this ISettingsSource iss, string name, int defaultValue)
        {
            return iss.GetInt(name) ?? defaultValue;
        }

        public static void SetFont(this ISettingsSource iss, string name, Font value)
        {
            iss.SetValue<Font>(name, value, x => x.AsString());
        }

        public static Font GetFont(this ISettingsSource iss, string name, Font defaultValue)
        {
            return iss.GetValue<Font>(name, defaultValue, x => x.Parse(defaultValue));
        }

        public static void SetColor(this ISettingsSource iss, string name, Color? value)
        {
            iss.SetValue<Color?>(name, value, x => x.HasValue ? ColorTranslator.ToHtml(x.Value) : null);
        }

        public static Color? GetColor(this ISettingsSource iss, string name)
        {
            return iss.GetValue<Color?>(name, null, x => ColorTranslator.FromHtml(x));
        }

        public static Color GetColor(this ISettingsSource iss, string name, Color defaultValue)
        {
            return iss.GetColor(name) ?? defaultValue;
        }

        public static void SetEnum<T>(this ISettingsSource iss, string name, T value)
        {
            iss.SetValue<T>(name, value, x => x.ToString());
        }

        public static T GetEnum<T>(this ISettingsSource iss, string name, T defaultValue) where T : struct
        {
            return iss.GetValue<T>(name, defaultValue, x =>
            {
                var val = x.ToString();

                T result;
                if (Enum.TryParse(val, true, out result))
                    return result;

                return defaultValue;
            });
        }

        public static void SetNullableEnum<T>(this ISettingsSource iss, string name, T? value) where T : struct
        {
            iss.SetValue<T?>(name, value, x => x.HasValue ? x.ToString() : string.Empty);
        }

        public static T? GetNullableEnum<T>(this ISettingsSource iss, string name) where T : struct
        {
            return iss.GetValue<T?>(name, null, x =>
            {
                var val = x.ToString();

                if (val.IsNullOrEmpty())
                    return null;

                T result;
                if (Enum.TryParse(val, true, out result))
                    return result;

                return null;
            });
        }

        public static void SetString(this ISettingsSource iss, string name, string value)
        {
            iss.SetValue<string>(name, value, s => s);
        }

        public static string GetString(this ISettingsSource iss, string name, string defaultValue)
        {
            return iss.GetValue<string>(name, defaultValue, x => x);
        }
    }


    public static class FontParser
    {

        private static readonly string InvariantCultureId = "_IC_";
        public static string AsString(this Font value)
        {
            return String.Format(CultureInfo.InvariantCulture,
                "{0};{1};{2}", value.FontFamily.Name, value.Size, InvariantCultureId);
        }

        public static Font Parse(this string value, Font defaultValue)
        {
            if (value == null)
                return defaultValue;

            string[] parts = value.Split(';');

            if (parts.Length < 2)
                return defaultValue;

            try
            {
                string fontSize;
                if (parts.Length == 3 && InvariantCultureId.Equals(parts[2]))
                    fontSize = parts[1];
                else
                {
                    fontSize = parts[1].Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                    fontSize = fontSize.Replace(".", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                }

                return new Font(parts[0], Single.Parse(fontSize, CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
