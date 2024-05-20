using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GitExtensions.Extensibility.Settings;

public abstract class SettingsSource
{
    public virtual SettingLevel SettingLevel { get; set; } = SettingLevel.Unknown;

    public abstract string? GetValue(string name);

    public abstract void SetValue(string name, string? value);

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public string? GetString(string name, string? defaultValue) => GetValue(name) ?? defaultValue;

    public void SetString(string name, string? value) => SetValue(name, value);

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

    public bool GetBool(string name, bool defaultValue) => GetBool(name) ?? defaultValue;

    public void SetBool(string name, bool? value)
    {
        string? stringValue = value.HasValue ? value.Value ? "true" : "false" : null;

        SetValue(name, stringValue);
    }

    public int? GetInt(string name)
    {
        string? stringValue = GetValue(name);

        if (int.TryParse(stringValue, out int result))
        {
            return result;
        }

        return null;
    }

    public int GetInt(string name, int defaultValue) => GetInt(name) ?? defaultValue;

    public void SetInt(string name, int? value)
    {
        string? stringValue = value.HasValue ? value.ToString() : null;

        SetValue(name, stringValue);
    }

    public float? GetFloat(string name)
    {
        string? stringValue = GetValue(name);

        if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }

        if (float.TryParse(stringValue, out result))
        {
            return result;
        }

        return null;
    }

    public float GetFloat(string name, float defaultValue) => GetFloat(name) ?? defaultValue;

    public void SetFloat(string name, float? value)
    {
        string? stringValue = value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : null;

        SetValue(name, stringValue);
    }

    public DateTime? GetDate(string name)
    {
        string? stringValue = GetValue(name);

        if (DateTime.TryParseExact(stringValue, "yyyy/M/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }

        return null;
    }

    public DateTime GetDate(string name, DateTime defaultValue) => GetDate(name) ?? defaultValue;

    public void SetDate(string name, DateTime? value)
    {
        string? stringValue = value?.ToString("yyyy/M/dd", CultureInfo.InvariantCulture);

        SetValue(name, stringValue);
    }

    public Font GetFont(string name, Font defaultValue) => GetValue(name).Parse(defaultValue);

    public void SetFont(string name, Font value) => SetValue(name, value.AsString());

    public Color GetColor(string name, Color defaultValue)
    {
        string? stringValue = GetValue(name);

        if (!string.IsNullOrWhiteSpace(stringValue))
        {
            try
            {
                Color result = ColorTranslator.FromHtml(stringValue);
                if (result != Color.Empty)
                {
                    return result;
                }
            }
            catch
            {
                // ignore invalid color values (return the default value)
            }
        }

        return defaultValue;
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

    public void SetEnum<T>(string name, T value) where T : Enum
    {
        string? stringValue = value.ToString();

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

    public void SetNullableEnum<T>(string name, T? value) where T : struct, Enum
    {
        string? stringValue = value.HasValue ? value.ToString() : string.Empty;

        SetValue(name, stringValue);
    }
}
