namespace GitExtensions.Extensibility.Settings;

public interface ISettingsValueGetter
{
    string GetValue(string setting);

    /// <summary>
    ///  Gets the config setting from git converted in an expected C# value type (bool, int, etc.).
    /// </summary>
    /// <typeparam name="T">The expected type of the git setting.</typeparam>
    /// <param name="setting">The git setting key.</param>
    /// <returns>The value converted to the <typeparamref name="T" /> type; <see langword="null"/> if the settings is not set.</returns>
    /// <exception cref="GitConfigFormatException">
    ///  The value of the git setting <paramref name="setting" /> cannot be converted in the specified type <typeparamref name="T" />.
    /// </exception>
    T? GetValue<T>(string setting) where T : struct;
}
