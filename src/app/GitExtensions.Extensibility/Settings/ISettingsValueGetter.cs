#nullable enable

namespace GitExtensions.Extensibility.Settings;

public interface ISettingsValueGetter
{
    string? GetValue(string setting);

    /// <summary>
    ///  Gets the config setting from git converted in an expected C# value type (bool, int, etc.).
    /// </summary>
    /// <typeparam name="T">The expected type of the git setting.</typeparam>
    /// <param name="setting">The git setting key.</param>
    /// <returns>The value converted to the <typeparamref name="T" /> type; <see langword="null"/> if the settings is not set.</returns>
    /// <exception cref="GitConfigFormatException">
    ///  The value of the git setting <paramref name="setting" /> cannot be converted in the specified type <typeparamref name="T" />.
    /// </exception>
    T? GetValue<T>(string setting) where T : struct
    {
        string? value = GetValue(setting);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        Type targetType = typeof(T);

        if (targetType.IsEnum && Enum.TryParse(value, true, out T result))
        {
            return result;
        }

        try
        {
            return (T)Convert.ChangeType(value, targetType);
        }
        catch (Exception)
        {
            throw new GitConfigFormatException($"Git setting '{setting}': failed to convert value '{value}' into type '{targetType}'");
        }
    }

    /// <summary>
    ///  Invalidates the data in order to trigger a reload on next access or in the background.
    /// </summary>
    void Invalidate()
    {
    }
}
